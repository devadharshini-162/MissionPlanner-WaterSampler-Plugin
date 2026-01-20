using GMap.NET;
using MissionPlanner;
using MissionPlanner.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace WaterSamplerDashboardPlugin
{
    public class WaterSamplerPlugin : Plugin
    {
        private DashboardPanel dashboard;
        private Form dashboardWindow;
        private SerialPort samplerPort = null;
        private readonly object portLock = new object();
        private string serialBuffer = "";
        public MissionState missionState = new MissionState();
        private string stateFilePath;

        private Timer mainLoopTimer;
        private DateTime lastRealDataTime = DateTime.MinValue;
        private bool isSimulationRunning = false;

        private int simBottleIndex = 1;
        private DateTime simStateStartTime = DateTime.Now;
        private string simWorkflowState = "Idle";
        private readonly Random simRnd = new Random();
        private bool isMissionComplete = false;

        private readonly PointLatLng homePos = new PointLatLng(13.0114, 80.0591);
        private readonly PointLatLng[] simWaypoints = new[]
        {
            new PointLatLng(13.0120, 80.0585),
            new PointLatLng(13.0118, 80.0595),
            new PointLatLng(13.0109, 80.0593),
            new PointLatLng(13.0108, 80.0586)
        };

        public override string Name => "Water Sampling Dashboard";
        public override string Version => "2.1-FINAL";
        public override string Author => "Dharshu";
        public override bool Init() => true;

        private PointLatLng simCurrentPos;
        private bool simReturningHome = false;

        public override bool Loaded()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MissionPlanner", "WaterSampler");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            stateFilePath = Path.Combine(appData, "mission_state.json");
            LoadMissionState();
            AddMenuButton();
            mainLoopTimer = new Timer { Interval = 100 };
            mainLoopTimer.Tick += MainLoopTimer_Tick;
            mainLoopTimer.Start();
            return true;
        }

        public void ToggleSimulation()
        {
            isSimulationRunning = !isSimulationRunning;
            if (isSimulationRunning)
            {
                simCurrentPos = homePos;
                simStateStartTime = DateTime.Now;
                simWorkflowState = "Idle";
                simBottleIndex = 1;
                isMissionComplete = false;
                simReturningHome = false;
                if (dashboard != null) dashboard.UpdateModeLabel("Sampling: ON");
            }
            else
            {
                if (dashboard != null) dashboard.UpdateModeLabel("Sampling: OFF");
            }
        }

        private void MainLoopTimer_Tick(object sender, EventArgs e)
        {
            if (dashboard == null || dashboard.IsDisposed) return;

            double realLat = 0, realLon = 0, realAlt = 0;
            if (MainV2.comPort.MAV.cs != null)
            {
                realLat = MainV2.comPort.MAV.cs.lat;
                realLon = MainV2.comPort.MAV.cs.lng;
                realAlt = MainV2.comPort.MAV.cs.alt;
            }

            if (realLat != 0 && realLon != 0) dashboard.UpdateDroneMarker(realLat, realLon);

            bool hardwareConnected = (samplerPort != null && samplerPort.IsOpen);

            if (isSimulationRunning)
            {
                RunSimulationSequence(realLat, realLon, realAlt);
            }

            if (hardwareConnected)
            {
                TimeSpan timeSinceLastData = DateTime.Now - lastRealDataTime;
                if (timeSinceLastData.TotalSeconds > 3 && isSimulationRunning)
                {
                    RunSimulationSequence(realLat, realLon, realAlt);
                }
            }
        }

        private void RunSimulationSequence(double lat, double lon, double alt)
        {
            if (simCurrentPos.Lat == 0 && simCurrentPos.Lng == 0) simCurrentPos = homePos;
            if (isMissionComplete) return;

            PointLatLng target = simReturningHome ? homePos : simWaypoints[simBottleIndex - 1];

            simCurrentPos = dashboard.MoveTowards(simCurrentPos, target, 0.000005);

            double curLat = simCurrentPos.Lat;
            double curLon = simCurrentPos.Lng;

            // SIMULATE ALTITUDE
            // If in "Fill" mode, pretend to be low (1m), else high (20m)
            double fakeAlt = (simWorkflowState == "Fill") ? 1.5 : 20.0;

            dashboard.UpdateDroneMarker(curLat, curLon);

            double dLat = target.Lat - curLat;
            double dLon = target.Lng - curLon;
            double fakeDist = Math.Sqrt(dLat * dLat + dLon * dLon) * 111000.0;
            if (fakeDist > 2000000) fakeDist = 0;

            bool arrived = (fakeDist < 3.0);

            switch (simWorkflowState)
            {
                case "Idle":
                    dashboard.UpdateSamplerStatus("Idle", simBottleIndex, simBottleIndex - 1, 4, curLat, curLon, fakeAlt, $"WP{simBottleIndex}", fakeDist);
                    if (!arrived) simWorkflowState = "Travel";
                    break;

                case "Travel":
                    dashboard.UpdateSamplerStatus("Travel", simBottleIndex, simBottleIndex - 1, 4, curLat, curLon, fakeAlt, $"WP{simBottleIndex}", fakeDist);
                    if (arrived)
                    {
                        if (simReturningHome)
                        {
                            isMissionComplete = true;
                            dashboard.UpdateSamplerStatus("Idle", 4, 4, 4, curLat, curLon, fakeAlt, "MISSION COMPLETE", 0);
                        }
                        else
                        {
                            simWorkflowState = "Fill";
                            simStateStartTime = DateTime.Now;
                        }
                    }
                    break;

                case "Fill":
                    dashboard.UpdateSamplerStatus("Fill", simBottleIndex, simBottleIndex - 1, 4, curLat, curLon, fakeAlt, "Sampling...", 0);
                    TimeSpan duration = DateTime.Now - simStateStartTime;

                    if (duration.TotalSeconds >= 3 && duration.TotalSeconds < 4)
                    {
                        float ph = 6.5f + (float)(simRnd.NextDouble() * 2);
                        float turb = 5f + (float)(simRnd.NextDouble() * 15);
                        float temp = 24f + (float)(simRnd.NextDouble() * 3);
                        dashboard.UpdateSensorData(simBottleIndex, ph, turb, temp);
                    }

                    if (duration.TotalSeconds >= 5)
                    {
                        simWorkflowState = "Seal";
                        simStateStartTime = DateTime.Now;
                    }
                    break;

                case "Seal":
                    dashboard.UpdateSamplerStatus("Seal", simBottleIndex, simBottleIndex, 4, curLat, curLon, fakeAlt, "Sealing Bottle...", fakeDist);
                    if ((DateTime.Now - simStateStartTime).TotalSeconds >= 2)
                    {
                        simBottleIndex++;
                        if (simBottleIndex > 4)
                        {
                            simReturningHome = true;
                            simWorkflowState = "Travel";
                        }
                        else
                        {
                            simWorkflowState = "Travel";
                        }
                    }
                    break;
            }

            int completed = Math.Min(simBottleIndex - 1, 4);
            dashboard.UpdateWaypointProgress(completed, 4);
        }

        // ... (Keep hardware connection, Menu Button, Data Classes code same) ...
        public void ConnectSampler(string portName, int baudRate) { lock (portLock) { try { if (samplerPort != null && samplerPort.IsOpen) samplerPort.Close(); samplerPort = new SerialPort(portName, baudRate) { ReadTimeout = 1000, WriteTimeout = 1000, NewLine = "\n", DtrEnable = false, RtsEnable = false }; samplerPort.DataReceived += SamplerPort_DataReceived; samplerPort.Open(); if (dashboard != null) dashboard.UpdateConnectionStatus(true); } catch (Exception ex) { if (dashboard != null) dashboard.UpdateConnectionStatus(false); MessageBox.Show($"Connection Failed: {ex.Message}", "Error"); } } }
        public void DisconnectSampler() { lock (portLock) { try { if (samplerPort != null && samplerPort.IsOpen) samplerPort.Close(); samplerPort?.Dispose(); samplerPort = null; } catch { } finally { if (dashboard != null) dashboard.UpdateConnectionStatus(false); } } }
        private void SamplerPort_DataReceived(object sender, SerialDataReceivedEventArgs e) { try { if (samplerPort == null || !samplerPort.IsOpen) return; string newData = samplerPort.ReadExisting(); serialBuffer += newData; lastRealDataTime = DateTime.Now; while (serialBuffer.Contains("\n")) { int newlineIndex = serialBuffer.IndexOf("\n"); string line = serialBuffer.Substring(0, newlineIndex).Trim(); serialBuffer = serialBuffer.Substring(newlineIndex + 1); if (line.StartsWith("Packet:")) { string csvData = line.Substring(7).Trim(); string[] parts = csvData.Split(','); if (parts.Length >= 3) { float temp = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture); float ph = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture); float turb = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture); int target = (simWorkflowState == "Fill") ? simBottleIndex : 1; if (target > 4) target = 1; if (dashboard != null && !dashboard.IsDisposed) { dashboard.BeginInvoke(new Action(() => { dashboard.UpdateSensorData(target, ph, turb, temp); })); } } } } } catch { } }

        private void AddMenuButton() { var mainMenu = Host.MainForm.MainMenuStrip; if (mainMenu == null) return; var btn = new ToolStripMenuItem { Text = "SAMPLER", Name = "btnWaterSampler", ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Image = CreateDropletIcon(), ImageScaling = ToolStripItemImageScaling.None, TextImageRelation = TextImageRelation.ImageAboveText, ToolTipText = "Open Water Sampling Dashboard", BackColor = Color.Transparent }; btn.Click += (s, e) => OpenDashboard(); mainMenu.Items.Add(btn); }
        private Bitmap CreateDropletIcon() { int size = 32; Bitmap bmp = new Bitmap(size, size); using (Graphics g = Graphics.FromImage(bmp)) { g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Color.Transparent); using (GraphicsPath path = new GraphicsPath()) { path.AddLine(size / 2, 2, size - 6, size - 10); path.AddArc(6, size - 16, size - 12, size - 12, 0, 180); path.AddLine(6, size - 10, size / 2, 2); path.CloseFigure(); using (Brush brush = new LinearGradientBrush(new Point(0, 0), new Point(0, size), Color.Cyan, Color.Blue)) g.FillPath(brush, path); } } return bmp; }
        private void OpenDashboard() { if (dashboardWindow == null || dashboardWindow.IsDisposed) { dashboard = new DashboardPanel { Dock = DockStyle.Fill, Backend = this }; dashboardWindow = new Form { Text = "Water Sampling Dashboard", Width = 1450, Height = 950, StartPosition = FormStartPosition.CenterScreen, BackColor = Color.FromArgb(30, 30, 30) }; dashboardWindow.Controls.Add(dashboard); dashboardWindow.FormClosed += (s, e) => { SaveMissionState(); dashboardWindow = null; }; dashboardWindow.Shown += (s, ev) => { RestoreMissionStateToDashboard(); dashboard.ConnectCamera(); }; dashboardWindow.Show(); } else { dashboardWindow.BringToFront(); } }
        private void LoadMissionState() { try { if (File.Exists(stateFilePath)) missionState = JsonConvert.DeserializeObject<MissionState>(File.ReadAllText(stateFilePath)); } catch { } }
        private void SaveMissionState() { try { if (dashboard != null) missionState.MapView = dashboard.GetMapViewState(); File.WriteAllText(stateFilePath, JsonConvert.SerializeObject(missionState)); } catch { } }
        private void RestoreMissionStateToDashboard() { if (dashboard == null) return; try { if (missionState.Samples != null) { foreach (var s in missionState.Samples) dashboard.UpdateSensorData(s.BottleID, s.pH, s.Turbidity, s.Temperature); } } catch { } }
        public override bool Loop() => true;
        public override bool Exit() { mainLoopTimer?.Stop(); DisconnectSampler(); if (dashboardWindow != null) dashboardWindow.Close(); return true; }

        public class WaypointData { public int ID { get; set; } public double Lat { get; set; } public double Lon { get; set; } public double Alt { get; set; } }
        public class SampleData { public int BottleID { get; set; } public float pH { get; set; } public float Turbidity { get; set; } public float Temperature { get; set; } public double Lat { get; set; } public double Lon { get; set; } public DateTime Timestamp { get; set; } }
        public class DronePathData { public List<PathPoint> Points { get; set; } = new List<PathPoint>(); }
        public class PathPoint { public double Lat { get; set; } public double Lon { get; set; } public DateTime Timestamp { get; set; } }
        public class MissionState { public string MissionName { get; set; } = "Water Sampling Mission"; public DateTime MissionStartTime { get; set; } = DateTime.Now; public List<WaypointData> Waypoints { get; set; } = new List<WaypointData>(); public List<SampleData> Samples { get; set; } = new List<SampleData>(); public DronePathData DronePath { get; set; } = new DronePathData(); public int CurrentBottle { get; set; } = 1; public int SamplesCollected { get; set; } = 0; public string WorkflowState { get; set; } = "Idle"; public MapViewState MapView { get; set; } = new MapViewState(); }
        public class MapViewState { public double CenterLat { get; set; } = 13.0114; public double CenterLon { get; set; } = 80.0591; public int ZoomLevel { get; set; } = 15; public string MapType { get; set; } = "Satellite"; public DateTime LastUpdated { get; set; } = DateTime.Now; }
    }
}