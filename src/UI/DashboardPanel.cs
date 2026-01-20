using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using MissionPlanner;
using MissionPlanner.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using static WaterSamplerDashboardPlugin.WaterSamplerPlugin;

namespace WaterSamplerDashboardPlugin
{
    public partial class DashboardPanel : UserControl
    {
        public WaterSamplerPlugin Backend { get; set; }

        // --- BOTTLE & SENSOR DATA ---
        private float[,] bottleData = new float[4, 4];
        private int selectedBottle = 1;
        private BottleState[] bottleStates = new BottleState[4];
        private enum BottleState { Empty, Sealed }

        // --- MAP VARIABLES ---
        private GMapOverlay waypointsOverlay;
        private GMapOverlay droneOverlay;
        private GMapOverlay dronePathOverlay;
        private GMarkerGoogle currentDroneMarker;
        private double lastDroneLat = 0;
        private double lastDroneLon = 0;

        // --- VIDEO VARIABLES ---
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private VideoView _videoView;
        private bool _vlcInitialized = false;

        public DashboardPanel()
        {
            InitializeComponent();

            CreateNoSignalImage();
            SetupBottleButtons();
            SetupMapOverlays();
            PopulateComPorts();

            // HIDE AUTO/MANUAL BUTTONS (Requirement 2)
            if (radioAuto != null) radioAuto.Visible = false;
            if (radioManual != null) radioManual.Visible = false;
            if (lblModeSelector != null) lblModeSelector.Visible = false;

            // Clock Timer
            this.updateTimer.Tick += (s, e) =>
            {
                if (lblSystemTime != null && !lblSystemTime.IsDisposed)
                    lblSystemTime.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            this.updateTimer.Start();

            // Wiring
            if (btnModeToggle != null)
                btnModeToggle.Click += (s, e) => { if (Backend != null) Backend.ToggleSimulation(); };

            if (btnConnect != null)
                btnConnect.Click += BtnConnect_Click;

            // Map Controls - Enable Pan/Zoom
            if (gmapControl != null)
            {
                gmapControl.CanDragMap = true;
                gmapControl.DragButton = MouseButtons.Left;
                gmapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
                gmapControl.IgnoreMarkerOnMouseWheel = true;
                gmapControl.MinZoom = 2;
                gmapControl.MaxZoom = 20;
            }

            if (btnZoomIn != null)
                btnZoomIn.Click += (s, e) => { if (gmapControl != null && gmapControl.Zoom < gmapControl.MaxZoom) gmapControl.Zoom++; };

            if (btnZoomOut != null)
                btnZoomOut.Click += (s, e) => { if (gmapControl != null && gmapControl.Zoom > gmapControl.MinZoom) gmapControl.Zoom--; };

            if (btnCenterDrone != null)
                btnCenterDrone.Click += (s, e) => {
                    if (gmapControl != null && currentDroneMarker != null)
                        gmapControl.Position = currentDroneMarker.Position;
                };

            if (btnFullscreenMap != null) btnFullscreenMap.Click += ToggleMapFullscreen;
            if (btnFullscreenVideo != null) btnFullscreenVideo.Click += ToggleVideoFullscreen;
            if (btnSnapshot != null) btnSnapshot.Click += BtnSnapshot_Click;

            // Map Type
            if (comboMapType != null)
            {
                comboMapType.SelectedIndexChanged += (s, e) =>
                {
                    if (gmapControl != null)
                    {
                        if (comboMapType.SelectedIndex == 0) gmapControl.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
                        else if (comboMapType.SelectedIndex == 2) gmapControl.MapProvider = GMap.NET.MapProviders.GMapProviders.GoogleHybridMap;
                        else gmapControl.MapProvider = GMap.NET.MapProviders.GMapProviders.GoogleSatelliteMap;
                    }
                };
            }

            InitializeTrendData(panelPhTrend, 7.2f);
            InitializeTrendData(panelTurbidityTrend, 5f);
            InitializeTrendData(panelTemperatureTrend, 24f);

            if (panelPhTrend != null) panelPhTrend.Paint += TrendPanel_Paint;
            if (panelTurbidityTrend != null) panelTurbidityTrend.Paint += TrendPanel_Paint;
            if (panelTemperatureTrend != null) panelTemperatureTrend.Paint += TrendPanel_Paint;

            for (int i = 0; i < 4; i++) bottleStates[i] = BottleState.Empty;

            try { InitializeVideoFeed(); }
            catch { CreateNoSignalImage(); }
        }

        // --- MAP CRASH FIX & INTERACTION ---
        public void UpdateDroneMarker(double lat, double lng)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateDroneMarker(lat, lng))); return; }

            if (Math.Abs(lat - lastDroneLat) < 0.0000001 && Math.Abs(lng - lastDroneLon) < 0.0000001) return;

            lastDroneLat = lat;
            lastDroneLon = lng;
            var pos = new PointLatLng(lat, lng);

            try
            {
                if (currentDroneMarker == null)
                {
                    currentDroneMarker = new GMarkerGoogle(pos, GMarkerGoogleType.green_small);
                    if (droneOverlay != null) droneOverlay.Markers.Add(currentDroneMarker);
                }
                else
                {
                    currentDroneMarker.Position = pos;
                }
            }
            catch { }
        }

        // --- FULLSCREEN LOGIC ---
        private Form fullscreenMapForm = null;
        private void ToggleMapFullscreen(object sender, EventArgs e)
        {
            if (fullscreenMapForm == null || fullscreenMapForm.IsDisposed)
            {
                var savedPos = gmapControl.Position;
                var savedZoom = gmapControl.Zoom;

                fullscreenMapForm = new Form
                {
                    Text = "Map - Fullscreen (ESC to Exit)",
                    WindowState = FormWindowState.Maximized,
                    FormBorderStyle = FormBorderStyle.None,
                    BackColor = Color.Black
                };

                var tempMap = new GMapControl
                {
                    Dock = DockStyle.Fill,
                    MapProvider = gmapControl.MapProvider,
                    Position = savedPos,
                    Zoom = savedZoom,
                    MinZoom = 2,
                    MaxZoom = 20,
                    ShowCenter = false,
                    CanDragMap = true,
                    DragButton = MouseButtons.Left,
                    MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter
                };

                // Add close button
                Button closeBtn = new Button { Text = "X", BackColor = Color.Red, ForeColor = Color.White, Size = new Size(30, 30), Location = new Point(Screen.PrimaryScreen.Bounds.Width - 40, 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };
                closeBtn.Click += (s, ev) => fullscreenMapForm.Close();
                fullscreenMapForm.Controls.Add(closeBtn);

                // Copy overlays
                foreach (var overlay in gmapControl.Overlays)
                {
                    var newOverlay = new GMapOverlay(overlay.Id);
                    foreach (var marker in overlay.Markers)
                    {
                        var m = new GMarkerGoogle(marker.Position, GMarkerGoogleType.green_small);
                        newOverlay.Markers.Add(m);
                    }
                    tempMap.Overlays.Add(newOverlay);
                }

                fullscreenMapForm.Controls.Add(tempMap);
                tempMap.SendToBack(); // Ensure button is on top

                fullscreenMapForm.KeyDown += (s, ev) => { if (ev.KeyCode == Keys.Escape) fullscreenMapForm.Close(); };
                fullscreenMapForm.FormClosed += (s, ev) =>
                {
                    gmapControl.Position = tempMap.Position;
                    gmapControl.Zoom = tempMap.Zoom;
                };
                fullscreenMapForm.Show();
            }
            else
            {
                fullscreenMapForm.Close();
            }
        }

        private Form fullscreenVideoForm = null;
        private void ToggleVideoFullscreen(object sender, EventArgs e)
        {
            if (fullscreenVideoForm == null || fullscreenVideoForm.IsDisposed)
            {
                fullscreenVideoForm = new Form
                {
                    Text = "Live Feed - Fullscreen (ESC to Exit)",
                    WindowState = FormWindowState.Maximized,
                    FormBorderStyle = FormBorderStyle.None,
                    BackColor = Color.Black
                };

                // NOTE: LibVLC VideoView cannot be easily reparented without stopping. 
                // Creating a new view for fullscreen is safer.
                var tempVideo = new VideoView
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    MediaPlayer = _mediaPlayer // Share player
                };

                Button closeBtn = new Button { Text = "X", BackColor = Color.Red, ForeColor = Color.White, Size = new Size(30, 30), Location = new Point(Screen.PrimaryScreen.Bounds.Width - 40, 10), Anchor = AnchorStyles.Top | AnchorStyles.Right };
                closeBtn.Click += (s, ev) => fullscreenVideoForm.Close();
                fullscreenVideoForm.Controls.Add(closeBtn);

                fullscreenVideoForm.Controls.Add(tempVideo);
                tempVideo.SendToBack();

                fullscreenVideoForm.KeyDown += (s, ev) => { if (ev.KeyCode == Keys.Escape) fullscreenVideoForm.Close(); };
                fullscreenVideoForm.Show();
            }
            else
            {
                fullscreenVideoForm.Close();
            }
        }

        // --- VIDEO FIX: DRONE WATER FOOTAGE ---
        public void ConnectCamera(string pathOrUrl = null)
        {
            if (!_vlcInitialized || _mediaPlayer == null) return;

            try
            {
                // DRONE WATER FOOTAGE URL (Public test URL for "Ocean" or similar)
                // Using a reliable sample. You can replace this with a local file path if internet is slow.
                // "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4" (Reliable fallback)
                // Better realistic water option: 
                string src = pathOrUrl ?? "https://www.pexels.com/download/video/20176158/";
                // Note: Direct "drone water" URLs often expire. Using a reliable movie clip is safer for "test", 
                // BUT for "realistic water", you should download a clip from Pexels/Pixabay and use local path:
                // string src = @"C:\Users\YourUser\Videos\drone_water.mp4";

                var media = new Media(_libVLC, src, FromType.FromLocation);
                media.AddOption(":network-caching=1000");
                media.AddOption(":input-repeat=65535"); // Loop video
                _mediaPlayer.Play(media);

                if (btnSnapshot != null) btnSnapshot.Enabled = true;
                if (lblConnectionQuality != null) lblConnectionQuality.Text = "Streaming (Active)";
            }
            catch (Exception ex)
            {
                CreateNoSignalImage();
                if (lblConnectionQuality != null) lblConnectionQuality.Text = "Stream Error";
            }
        }

        // --- ALTITUDE FIX ---
        public void UpdateSamplerStatus(string workflowState, int currentBottle, int bottlesFilled, int totalBottles, double lat, double lon, double alt, string nextWaypoint, double nextWaypointDistance)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateSamplerStatus(workflowState, currentBottle, bottlesFilled, totalBottles, lat, lon, alt, nextWaypoint, nextWaypointDistance))); return; }

            if (lblStateIdle != null)
            {
                lblStateIdle.BackColor = workflowState == "Idle" ? Color.FromArgb(126, 211, 33) : Color.FromArgb(60, 60, 60);
                lblStateRinsing.BackColor = workflowState == "flush" ? Color.Red : Color.FromArgb(60, 60, 60);
                lblStateFilling.BackColor = workflowState == "Fill" ? Color.Red : Color.FromArgb(60, 60, 60);
                lblStateSealed.BackColor = workflowState == "Seal" ? Color.FromArgb(126, 211, 33) : Color.FromArgb(60, 60, 60);
            }

            if (lblCurrentBottle != null) lblCurrentBottle.Text = $"Current Bottle: #{currentBottle}";
            if (lblBottleProgress != null) lblBottleProgress.Text = $"Bottles Filled: {bottlesFilled} / {totalBottles}";
            if (lblSampleCount != null) lblSampleCount.Text = $"Samples: {bottlesFilled}";

            if (lat != 0 && lblCurrentLat != null)
            {
                lblCurrentLat.Text = $"Lat: {lat:0.000000}";
                lblCurrentLon.Text = $"Lon: {lon:0.000000}";
                // Fix Altitude Display - use altitude from sim or GPS
                lblCurrentAlt.Text = $"Alt: {alt:F1} m";
                // Also update the sensor panel altitude
                if (lblAltitudeValue != null) lblAltitudeValue.Text = $"{alt:F1} m";
            }

            if (lblMissionControl != null)
                lblMissionControl.Text = $"Mission Status: {workflowState.ToUpper()}\nWaypoint: {nextWaypoint} | Dist: {nextWaypointDistance:F0}m";

            if (lblNextWaypoint != null) lblNextWaypoint.Text = $"Next Waypoint: {nextWaypoint}";
            if (lblWaypointDistance != null) lblWaypointDistance.Text = $"Distance: {nextWaypointDistance:F0} m";
        }

        // ... (Keep existing UpdateConnectionStatus, UpdateModeLabel, etc.) ...
        public void UpdateConnectionStatus(bool connected)
        {
            if (InvokeRequired) { Invoke(new Action(() => UpdateConnectionStatus(connected))); return; }
            if (lblConnection == null || btnConnect == null || comboPorts == null) return;

            lblConnection.Text = connected ? "Connected" : "Disconnected";
            lblConnection.ForeColor = connected ? Color.LimeGreen : Color.Red;
            btnConnect.Text = connected ? "Disconnect" : "Connect";
            btnConnect.BackColor = connected ? Color.FromArgb(217, 83, 79) : Color.FromArgb(0, 122, 204);
            comboPorts.Enabled = !connected;
        }

        public void UpdateModeLabel(string text)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateModeLabel(text))); return; }
            if (btnModeToggle != null)
                btnModeToggle.Text = text;
        }

        public void UpdateSensorData(int bottleID, float pH, float turbidity, float temperature)
        {
            if (bottleID < 1 || bottleID > 4) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateSensorData(bottleID, pH, turbidity, temperature))); return; }

            int idx = bottleID - 1;
            bottleData[0, idx] = pH;
            bottleData[1, idx] = turbidity;
            bottleData[2, idx] = temperature;
            bottleData[3, idx] = 1;

            if (selectedBottle == bottleID) UpdateSensorDisplayForSelectedBottle();

            bottleStates[bottleID - 1] = BottleState.Sealed;

            var lblArr = new[] { lblBottle1Status, lblBottle2Status, lblBottle3Status, lblBottle4Status };
            var btnArr = new[] { btnBottle1, btnBottle2, btnBottle3, btnBottle4 };

            if (lblArr[idx] != null)
            {
                lblArr[idx].Text = "Sealed";
                lblArr[idx].ForeColor = Color.White;
            }

            if (btnArr[idx] != null)
                btnArr[idx].FlatAppearance.BorderColor = Color.White;
        }

        private void UpdateSensorDisplayForSelectedBottle()
        {
            float pH = bottleData[0, selectedBottle - 1];
            float turbidity = bottleData[1, selectedBottle - 1];
            float temperature = bottleData[2, selectedBottle - 1];
            bool hasData = (pH > 0.1 || turbidity > 0.1 || temperature > 0.1);

            if (!hasData)
            {
                if (lblPhValue != null) lblPhValue.Text = "No Data";
                if (lblTurbidityValue != null) lblTurbidityValue.Text = "No Data";
                if (lblTemperatureValue != null) lblTemperatureValue.Text = "No Data";
                if (lblPhQuality != null) lblPhQuality.Text = "";
                if (lblTurbidityQuality != null) lblTurbidityQuality.Text = "";
                if (lblTemperatureQuality != null) lblTemperatureQuality.Text = "";
            }
            else
            {
                if (lblPhValue != null) lblPhValue.Text = pH.ToString("0.00");
                if (lblTurbidityValue != null) lblTurbidityValue.Text = turbidity.ToString("0.0") + " NTU";
                if (lblTemperatureValue != null) lblTemperatureValue.Text = temperature.ToString("0.0") + " °C";

                if (lblPhQuality != null)
                {
                    lblPhQuality.Text = GetPhQuality(pH);
                    lblPhQuality.ForeColor = GetPhColor(pH);
                }

                if (lblTurbidityQuality != null)
                {
                    lblTurbidityQuality.Text = GetTurbidityQuality(turbidity);
                    lblTurbidityQuality.ForeColor = GetTurbidityColor(turbidity);
                }

                if (lblTemperatureQuality != null)
                {
                    lblTemperatureQuality.Text = "Normal";
                    lblTemperatureQuality.ForeColor = Color.White;
                }

                UpdateTrendWithActualValue(panelPhTrend, pH);
                UpdateTrendWithActualValue(panelTurbidityTrend, turbidity);
                UpdateTrendWithActualValue(panelTemperatureTrend, temperature);
            }
        }

        // ... (Keep existing Helper methods: GetPhQuality, GetTurbidityQuality, etc.) ...
        private string GetPhQuality(float ph) { if (ph < 6.5) return "Acidic"; if (ph > 8.5) return "Alkaline"; return "Neutral"; }
        private Color GetPhColor(float ph) { if (ph < 6.5 || ph > 8.5) return Color.Red; return Color.White; }
        private string GetTurbidityQuality(float t) { if (t < 5) return "Clear"; if (t < 50) return "Cloudy"; return "Turbid"; }
        private Color GetTurbidityColor(float t) { if (t < 5) return Color.White; if (t < 50) return Color.Yellow; return Color.Red; }

        private void UpdateTrendWithActualValue(Panel trendPanel, float actualValue)
        {
            if (trendPanel == null || trendPanel.Tag == null) return;
            var dataPoints = trendPanel.Tag as List<float>;
            if (dataPoints == null) return;
            dataPoints.Add(actualValue);
            if (dataPoints.Count > 50) dataPoints.RemoveAt(0);
            trendPanel.Invalidate();
        }

        private void TrendPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;
            if (p == null || p.Tag == null) return;
            var data = p.Tag as List<float>;
            if (data.Count < 2) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Color.Cyan, 2))
            {
                float stepX = (float)p.Width / (data.Count - 1);
                float maxVal = data.Max() + 1;
                float minVal = data.Min() - 1;
                float range = maxVal - minVal;
                if (range == 0) range = 1;
                PointF[] points = new PointF[data.Count];
                for (int i = 0; i < data.Count; i++)
                {
                    float x = i * stepX;
                    float y = p.Height - ((data[i] - minVal) / range * p.Height);
                    points[i] = new PointF(x, y);
                }
                e.Graphics.DrawLines(pen, points);
            }
        }

        private void InitializeTrendData(Panel p, float baseValue)
        {
            if (p == null) return;
            var dataPoints = new List<float>();
            var r = new Random();
            for (int i = 0; i < 30; i++) dataPoints.Add(baseValue + (float)(r.NextDouble() - 0.5));
            p.Tag = dataPoints;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (Backend == null) return;
            if (btnConnect.Text == "Connect")
            {
                string port = comboPorts.Text;
                int baud = 115200;
                if (comboBaud.SelectedItem != null) int.TryParse(comboBaud.SelectedItem.ToString(), out baud);
                Backend.ConnectSampler(port, baud);
            }
            else
            {
                Backend.DisconnectSampler();
            }
        }

        private void PopulateComPorts()
        {
            if (comboPorts == null) return;
            comboPorts.Items.Clear();
            comboPorts.Items.AddRange(SerialPort.GetPortNames());
            if (comboPorts.Items.Count > 0) comboPorts.SelectedIndex = 0;
        }

        private void BtnSnapshot_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MissionPlanner", "WaterSampler", "Snapshots");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                string file = Path.Combine(path, $"snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                if (_vlcInitialized && _mediaPlayer != null)
                {
                    _mediaPlayer.TakeSnapshot(0, file, 0, 0);
                    MessageBox.Show("Snapshot saved: " + file);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Snapshot failed: " + ex.Message);
            }
        }

        private void SetupBottleButtons()
        {
            if (btnBottle1 != null) btnBottle1.Click += (s, e) => SelectBottle(1);
            if (btnBottle2 != null) btnBottle2.Click += (s, e) => SelectBottle(2);
            if (btnBottle3 != null) btnBottle3.Click += (s, e) => SelectBottle(3);
            if (btnBottle4 != null) btnBottle4.Click += (s, e) => SelectBottle(4);
            SelectBottle(1);
        }

        private void SelectBottle(int id)
        {
            selectedBottle = id;
            if (lblCurrentBottle != null) lblCurrentBottle.Text = $"Current Bottle: #{id}";
            if (lblSelectedBottleInfo != null) lblSelectedBottleInfo.Text = $"Viewing: Bottle #{id}";
            UpdateSensorDisplayForSelectedBottle();
        }

        private void CreateNoSignalImage()
        {
            try
            {
                if (pictureBoxVideo == null) return;
                int width = Math.Max(320, pictureBoxVideo.Width);
                int height = Math.Max(180, pictureBoxVideo.Height);
                var bmp = new Bitmap(width, height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(30, 30, 30));
                    using (var f1 = new Font("Segoe UI", 20, FontStyle.Bold))
                    using (var b1 = new SolidBrush(Color.FromArgb(120, 120, 120)))
                    {
                        string msg = "NO VIDEO FEED";
                        var size = g.MeasureString(msg, f1);
                        g.DrawString(msg, f1, b1, (width - size.Width) / 2, (height - size.Height) / 2);
                    }
                }
                if (pictureBoxVideo.Image != null) pictureBoxVideo.Image.Dispose();
                pictureBoxVideo.Image = bmp;
            }
            catch { }
        }

        private void InitializeVideoFeed()
        {
            try
            {
                Core.Initialize();
                _libVLC = new LibVLC("--network-caching=150", "--rtsp-tcp", "--no-audio");
                _mediaPlayer = new MediaPlayer(_libVLC);
                _videoView = new VideoView
                {
                    Location = pictureBoxVideo.Location,
                    Size = pictureBoxVideo.Size,
                    BackColor = Color.Black,
                    MediaPlayer = _mediaPlayer,
                    Anchor = pictureBoxVideo.Anchor
                };
                pictureBoxVideo.Visible = false;
                panelVideoFeed.Controls.Add(_videoView);
                _videoView.BringToFront();
                _vlcInitialized = true;
            }
            catch
            {
                CreateNoSignalImage();
            }
        }

        private void SetupMapOverlays()
        {
            if (gmapControl == null) return;
            gmapControl.Overlays.Clear();
            waypointsOverlay = new GMapOverlay("waypoints");
            droneOverlay = new GMapOverlay("drone");
            dronePathOverlay = new GMapOverlay("path");
            gmapControl.Overlays.Add(dronePathOverlay);
            gmapControl.Overlays.Add(waypointsOverlay);
            gmapControl.Overlays.Add(droneOverlay);
        }

        public MapViewState GetMapViewState()
        {
            if (InvokeRequired) return (MapViewState)Invoke(new Func<MapViewState>(() => GetMapViewState()));
            return new MapViewState { CenterLat = gmapControl.Position.Lat, CenterLon = gmapControl.Position.Lng, ZoomLevel = (int)gmapControl.Zoom, MapType = "Satellite" };
        }

        public void SetMapViewState(MapViewState state)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetMapViewState(state))); return; }
            try { gmapControl.Position = new PointLatLng(state.CenterLat, state.CenterLon); gmapControl.Zoom = state.ZoomLevel; } catch { }
        }

        public PointLatLng MoveTowards(PointLatLng current, PointLatLng target, double step)
        {
            double dLat = target.Lat - current.Lat;
            double dLon = target.Lng - current.Lng;
            double dist = Math.Sqrt(dLat * dLat + dLon * dLon);
            if (dist < step || dist == 0) return target;
            double k = step / dist;
            return new PointLatLng(current.Lat + dLat * k, current.Lng + dLon * k);
        }

        public void UpdateWaypointProgress(int completed, int total)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => UpdateWaypointProgress(completed, total))); return; }
            if (lblWaypointProgress != null) lblWaypointProgress.Text = $"WP: {completed}/{total}";
        }

        public void AddPathPoint(double lat, double lon) { }
    }
}