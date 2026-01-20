using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WaterSamplerDashboardPlugin
{
    partial class DashboardPanel
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();

                // Dispose custom resources if they exist
      
                if (pictureBoxVideo != null && pictureBoxVideo.Image != null) pictureBoxVideo.Image.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.trendUpdateTimer = new System.Windows.Forms.Timer(this.components);



            // Main Layout
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // Top Bar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));  // Middle Stats
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));  // Bottom Map/Video
            mainLayout.BackColor = Color.FromArgb(45, 45, 48);
            mainLayout.Padding = new Padding(0, 5, 0, 0);




            // =================================================================
            // TOP BAR
            // =================================================================
            this.panelTopBar = new Panel();
            this.panelTopBar.Dock = DockStyle.Fill;
            this.panelTopBar.BackColor = Color.FromArgb(28, 58, 95);
            // FIX 1: Increased Right Padding to 30 to prevent "Connected" label clipping
            this.panelTopBar.Padding = new Padding(10, 5, 30, 8);

            TableLayoutPanel topBarLayout = new TableLayoutPanel();
            topBarLayout.Dock = DockStyle.Fill;
            topBarLayout.ColumnCount = 7;
            topBarLayout.RowCount = 1;
            topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Title
            topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Toggle
            topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Spacer
            topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // COM
            topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F)); // Baud
            topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F)); // Connect
            topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F)); // Status
            this.panelTopBar.Controls.Add(topBarLayout);

            // Top Bar Controls
            this.lblTitle = new Label { Text = "Water Sampler Dashboard", Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 20, 0) };
            this.btnModeToggle = new Button { Text = "Sampling: OFF", Size = new Size(160, 32), BackColor = Color.FromArgb(0, 150, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Anchor = AnchorStyles.Left };
            this.btnModeToggle.FlatAppearance.BorderSize = 0;

            this.comboPorts = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110, Font = new Font("Segoe UI", 9F), Anchor = AnchorStyles.Right };
            this.comboBaud = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80, Font = new Font("Segoe UI", 9F), Anchor = AnchorStyles.Left };
            this.comboBaud.Items.AddRange(new object[] { "9600", "57600", "115200" });
            this.comboBaud.SelectedItem = "115200";

            this.btnConnect = new Button { Text = "Connect", Size = new Size(90, 32), Font = new Font("Segoe UI", 10F, FontStyle.Bold), Anchor = AnchorStyles.Left, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.btnConnect.FlatAppearance.BorderSize = 0;

            FlowLayoutPanel statusPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Anchor = AnchorStyles.Right };
            this.lblConnection = new Label { Text = "Disconnected", ForeColor = Color.Red, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true };
            this.lblSystemTime = new Label { Text = "00:00:00", ForeColor = Color.White, Font = new Font("Consolas", 11F, FontStyle.Bold), AutoSize = true };
            statusPanel.Controls.Add(lblConnection);
            statusPanel.Controls.Add(lblSystemTime);

            this.updateTimer.Tick += (s, e) => {
                if (lblSystemTime != null)
                    lblSystemTime.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            this.updateTimer.Start();


            topBarLayout.Controls.Add(lblTitle, 0, 0);
            topBarLayout.Controls.Add(btnModeToggle, 1, 0);
            topBarLayout.Controls.Add(comboPorts, 3, 0);
            topBarLayout.Controls.Add(comboBaud, 4, 0);
            topBarLayout.Controls.Add(btnConnect, 5, 0);
            topBarLayout.Controls.Add(statusPanel, 6, 0);

            // =================================================================
            // MIDDLE SECTION
            // =================================================================
            TableLayoutPanel middleLayout = new TableLayoutPanel();
            middleLayout.Dock = DockStyle.Fill;
            middleLayout.ColumnCount = 3;
            middleLayout.RowCount = 1;
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // Status
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F)); // Control
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); // Sensors
            middleLayout.Padding = new Padding(5);

            // --- LEFT: SAMPLER STATUS ---
            this.panelSamplerStatus = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(37, 37, 38), BorderStyle = BorderStyle.FixedSingle };
            TableLayoutPanel statusLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 9 };
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 9; i++) statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 11.1F));

            this.panelSamplerStatus.Controls.Add(statusLayout);
            this.lblSamplerTitle = new Label { Text = "Sampler Status", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(255, 180, 0), AutoSize = true };
            statusLayout.Controls.Add(lblSamplerTitle, 0, 0);

            this.panelWorkflowVisual = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 48), BorderStyle = BorderStyle.FixedSingle };
            TableLayoutPanel flowLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) flowLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            this.lblStateIdle = CreateWorkflowLabel("Idle", true);
            this.lblStateRinsing = CreateWorkflowLabel("Flush", false);
            this.lblStateFilling = CreateWorkflowLabel("Fill", false);
            this.lblStateSealed = CreateWorkflowLabel("Seal", false);
            flowLayout.Controls.AddRange(new Control[] { lblStateIdle, lblStateRinsing, lblStateFilling, lblStateSealed });
            this.panelWorkflowVisual.Controls.Add(flowLayout);
            statusLayout.Controls.Add(panelWorkflowVisual, 0, 1);

            statusLayout.Controls.Add(CreateStatusLabel(out lblCurrentBottle, "Current Bottle: --", Color.Cyan, 11F), 0, 2);
            statusLayout.Controls.Add(CreateStatusLabel(out lblBottleProgress, "Bottles Filled: 0 / 4", Color.White, 10F), 0, 3);
            statusLayout.Controls.Add(CreateStatusLabel(out lblCurrentLat, "Lat: 0.000000", Color.LightGray, 10F), 0, 4);
            statusLayout.Controls.Add(CreateStatusLabel(out lblCurrentLon, "Lon: 0.000000", Color.LightGray, 10F), 0, 5);
            statusLayout.Controls.Add(CreateStatusLabel(out lblCurrentAlt, "Alt: 0.0 m", Color.LightGray, 10F), 0, 6);
            statusLayout.Controls.Add(CreateStatusLabel(out lblNextWaypoint, "Next Waypoint: --", Color.Yellow, 10F), 0, 7);
            statusLayout.Controls.Add(CreateStatusLabel(out lblWaypointDistance, "Distance: -- m", Color.Yellow, 10F), 0, 8);

            // --- CENTER: BOTTLE CONTROL ---
            this.panelBottleControl = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(37, 37, 38), BorderStyle = BorderStyle.FixedSingle };
            TableLayoutPanel controlLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 4 };
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            controlLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            this.panelBottleControl.Controls.Add(controlLayout);

            this.lblBottleControlTitle = new Label { Text = "Bottle Control", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 150, 255), AutoSize = true };
            controlLayout.Controls.Add(lblBottleControlTitle, 0, 0);

            FlowLayoutPanel modePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            this.lblModeSelector = new Label { Text = "Mode:", ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 5, 10, 0) };
            this.radioAuto = new RadioButton { Text = "Auto", ForeColor = Color.White, Checked = true, Font = new Font("Segoe UI", 10F), AutoSize = true };
            this.radioManual = new RadioButton { Text = "Manual", ForeColor = Color.White, Font = new Font("Segoe UI", 10F), AutoSize = true };
            modePanel.Controls.AddRange(new Control[] { lblModeSelector, radioAuto, radioManual });
            controlLayout.Controls.Add(modePanel, 0, 1);

            TableLayoutPanel bottleGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            for (int i = 0; i < 4; i++) bottleGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            this.btnBottle1 = new Button(); this.lblBottle1Status = new Label();
            this.btnBottle2 = new Button(); this.lblBottle2Status = new Label();
            this.btnBottle3 = new Button(); this.lblBottle3Status = new Label();
            this.btnBottle4 = new Button(); this.lblBottle4Status = new Label();
            AddBottleToGrid(bottleGrid, 0, btnBottle1, lblBottle1Status, "1");
            AddBottleToGrid(bottleGrid, 1, btnBottle2, lblBottle2Status, "2");
            AddBottleToGrid(bottleGrid, 2, btnBottle3, lblBottle3Status, "3");
            AddBottleToGrid(bottleGrid, 3, btnBottle4, lblBottle4Status, "4");
            controlLayout.Controls.Add(bottleGrid, 0, 2);

            TableLayoutPanel infoLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            infoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.lblLastUpdate = new Label { Text = "Last Update: -- s ago", ForeColor = Color.FromArgb(180, 180, 180), Font = new Font("Consolas", 9F), AutoSize = true };
            this.lblMissionControl = new Label { Text = "Mission Status: No Mission Currently\nWaypoint: 0/4 | Next Sample: --m", ForeColor = Color.FromArgb(150, 200, 255), Font = new Font("Consolas", 10F), BackColor = Color.FromArgb(45, 45, 48), Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(5) };
            infoLayout.Controls.Add(lblLastUpdate, 0, 0);
            infoLayout.Controls.Add(lblMissionControl, 0, 1);
            controlLayout.Controls.Add(infoLayout, 0, 3);

            // --- RIGHT: SENSOR READINGS ---
            this.panelSensorReadings = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(37, 37, 38), BorderStyle = BorderStyle.FixedSingle };
            TableLayoutPanel sensorLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 7 };
            for (int i = 0; i < 7; i++) sensorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));

            this.panelSensorReadings.Controls.Add(sensorLayout);
            this.lblSensorTitle = new Label { Text = "Sensor Readings", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(0, 220, 100), AutoSize = true };
            this.lblSelectedBottleInfo = new Label { Text = "Viewing: Bottle --", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.Cyan, AutoSize = true };
            sensorLayout.Controls.Add(lblSensorTitle, 0, 0);
            sensorLayout.Controls.Add(lblSelectedBottleInfo, 0, 1);

            this.lblPh = new Label(); this.lblPhValue = new Label(); this.lblPhQuality = new Label(); this.panelPhTrend = new Panel();
            AddSensorRow(sensorLayout, 2, "pH", "--", "--", lblPh, lblPhValue, lblPhQuality, panelPhTrend, Color.LimeGreen);
            this.lblTurbidity = new Label(); this.lblTurbidityValue = new Label(); this.lblTurbidityQuality = new Label(); this.panelTurbidityTrend = new Panel();
            AddSensorRow(sensorLayout, 3, "Turbidity", "-- NTU", "--", lblTurbidity, lblTurbidityValue, lblTurbidityQuality, panelTurbidityTrend, Color.Yellow);
            this.lblTemperature = new Label(); this.lblTemperatureValue = new Label(); this.lblTemperatureQuality = new Label(); this.panelTemperatureTrend = new Panel();
            AddSensorRow(sensorLayout, 4, "Temperature", "-- °C", "--", lblTemperature, lblTemperatureValue, lblTemperatureQuality, panelTemperatureTrend, Color.LimeGreen);

            Panel altPanel = new Panel { Dock = DockStyle.Fill };
            this.lblAltitudeSensor = new Label { Text = "Altitude: ", ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Left };
            this.lblAltitudeValue = new Label { Text = "0.0 m", ForeColor = Color.Cyan, Font = new Font("Consolas", 11F, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Right };
            altPanel.Controls.Add(lblAltitudeSensor); altPanel.Controls.Add(lblAltitudeValue);
            sensorLayout.Controls.Add(altPanel, 0, 5);

            Panel footerPanel = new Panel { Dock = DockStyle.Fill };
            this.lblDataLogging = new Label { Text = "Data Logging: Active", ForeColor = Color.LimeGreen, Font = new Font("Segoe UI", 10F), AutoSize = true, Dock = DockStyle.Left };
            this.lblSampleCount = new Label { Text = "Samples: 0", ForeColor = Color.LightGray, Font = new Font("Consolas", 10F), AutoSize = true, Dock = DockStyle.Right };
            footerPanel.Controls.Add(lblDataLogging); footerPanel.Controls.Add(lblSampleCount);
            sensorLayout.Controls.Add(footerPanel, 0, 6);

            middleLayout.Controls.Add(this.panelSamplerStatus, 0, 0);
            middleLayout.Controls.Add(this.panelBottleControl, 1, 0);
            middleLayout.Controls.Add(this.panelSensorReadings, 2, 0);


            // =================================================================
            // BOTTOM SECTION
            // =================================================================
            TableLayoutPanel bottomLayout = new TableLayoutPanel();
            bottomLayout.Dock = DockStyle.Fill;
            bottomLayout.ColumnCount = 2;
            bottomLayout.RowCount = 1;
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F)); // Map
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F)); // Video
            bottomLayout.Padding = new Padding(5);

            // --- MAP ---
            this.panelMapContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black, BorderStyle = BorderStyle.FixedSingle };
            this.panelMapControls = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(45, 45, 48) };

            this.btnZoomIn = new Button { Text = "+", Size = new Size(30, 28), Location = new Point(5, 4) }; StyleSmallButton(btnZoomIn);
            this.btnZoomOut = new Button { Text = "-", Size = new Size(30, 28), Location = new Point(40, 4) }; StyleSmallButton(btnZoomOut);
            this.btnCenterDrone = new Button { Text = "Center", Size = new Size(60, 28), Location = new Point(75, 4) }; StyleSmallButton(btnCenterDrone);

            this.comboMapType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F), Size = new Size(100, 28), Location = new Point(140, 4) };
            this.comboMapType.Items.AddRange(new object[] { "Street", "Satellite", "Hybrid", "Terrain" });
            this.comboMapType.SelectedIndex = 1;

            this.lblWaypointProgress = new Label { Text = "WP: 0/4", ForeColor = Color.Yellow, Font = new Font("Consolas", 10F, FontStyle.Bold), AutoSize = true, Location = new Point(250, 8) };
            this.btnFullscreenMap = new Button { Text = "⛶", Size = new Size(30, 28), Font = new Font("Segoe UI", 12F, FontStyle.Bold), BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Right };

            this.panelMapControls.Controls.Add(btnZoomIn);
            this.panelMapControls.Controls.Add(btnZoomOut);
            this.panelMapControls.Controls.Add(btnCenterDrone);
            this.panelMapControls.Controls.Add(comboMapType);
            this.panelMapControls.Controls.Add(lblWaypointProgress);
            this.panelMapControls.Controls.Add(btnFullscreenMap);

            this.gmapControl = new GMapControl { Dock = DockStyle.Fill, MapProvider = GMap.NET.MapProviders.GMapProviders.GoogleSatelliteMap, Position = new PointLatLng(13.0114, 80.0591), MinZoom = 5, MaxZoom = 20, Zoom = 17, ShowCenter = false };

            this.panelMapContainer.Controls.Add(this.gmapControl);
            this.panelMapContainer.Controls.Add(this.panelMapControls);
            this.panelMapContainer.Controls.SetChildIndex(this.panelMapControls, 0);

            // --- VIDEO ---
            this.panelVideoFeed = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(37, 37, 38), BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(5) };
            TableLayoutPanel videoLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4 };
            videoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            videoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            videoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            videoLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));

            this.lblVideoTitle = new Label { Text = "Live Feed", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(255, 80, 80), AutoSize = true, Anchor = AnchorStyles.Left };
            this.btnFullscreenVideo = new Button { Text = "⛶", Size = new Size(30, 25), Font = new Font("Segoe UI", 8F, FontStyle.Bold), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            Panel vidHeader = new Panel { Dock = DockStyle.Fill, Height = 30 };
            vidHeader.Controls.Add(lblVideoTitle);
            vidHeader.Controls.Add(btnFullscreenVideo);
            btnFullscreenVideo.Dock = DockStyle.Right;

            this.pictureBoxVideo = new PictureBox();
            this.pictureBoxVideo.Dock = DockStyle.Fill;
            this.pictureBoxVideo.BackColor = Color.FromArgb(20, 20, 20);
            this.pictureBoxVideo.BorderStyle = BorderStyle.FixedSingle;
            this.pictureBoxVideo.SizeMode = PictureBoxSizeMode.Zoom;

            // FIX 2: Added specific Margin for the video box to create a "gap" from the edges
            this.pictureBoxVideo.Margin = new Padding(10, 5, 10, 5);

            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVideo)).BeginInit();

            this.lblVideoQuality = new Label { Text = "Quality: --", ForeColor = Color.FromArgb(180, 180, 180), Font = new Font("Consolas", 8F), AutoSize = true };

            FlowLayoutPanel vidControls = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 3, 0, 0) };
            this.btnSnapshot = new Button { Text = "Snap", Size = new Size(60, 26), Enabled = false }; StyleSmallButton(btnSnapshot);
            this.lblConnectionQuality = new Label { Text = "No Signal", ForeColor = Color.Red, Font = new Font("Consolas", 9F, FontStyle.Bold), AutoSize = true, Padding = new Padding(5, 5, 0, 0) };
            vidControls.Controls.Add(btnSnapshot);
            vidControls.Controls.Add(lblConnectionQuality);

            videoLayout.Controls.Add(vidHeader, 0, 0);
            videoLayout.Controls.Add(pictureBoxVideo, 0, 1);
            videoLayout.Controls.Add(lblVideoQuality, 0, 2);
            videoLayout.Controls.Add(vidControls, 0, 3);
            this.panelVideoFeed.Controls.Add(videoLayout);

            bottomLayout.Controls.Add(panelMapContainer, 0, 0);
            bottomLayout.Controls.Add(panelVideoFeed, 1, 0);

            mainLayout.Controls.Add(panelTopBar, 0, 0);
            mainLayout.Controls.Add(middleLayout, 0, 1);
            mainLayout.Controls.Add(bottomLayout, 0, 2);

            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 9.5F);
            this.Size = new Size(1380, 780);
            this.Name = "DashboardPanel";
            this.Controls.Add(mainLayout);

            this.updateTimer.Interval = 100;
            this.updateTimer.Start();
            this.trendUpdateTimer.Interval = 1000;
            this.trendUpdateTimer.Start();

            this.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxVideo)).EndInit();
        }

        #endregion

        // Methods for creating repetitive UI elements
        private Label CreateStatusLabel(out Label lbl, string text, Color color, float fontSize)
        {
            lbl = new Label { Text = text, ForeColor = color, Font = new Font("Consolas", fontSize), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 3, 0, 3) };
            return lbl;
        }

        private Label CreateWorkflowLabel(string text, bool active)
        {
            return new Label { Text = text, ForeColor = active ? Color.White : Color.Gray, Font = new Font("Segoe UI", 9F, active ? FontStyle.Bold : FontStyle.Regular), TextAlign = ContentAlignment.MiddleCenter, BackColor = active ? Color.FromArgb(200, 0, 0) : Color.FromArgb(60, 60, 60), Dock = DockStyle.Fill, Margin = new Padding(2) };
        }

        private void AddBottleToGrid(TableLayoutPanel grid, int col, Button btn, Label statusLbl, string number)
        {
            Panel container = new Panel { Dock = DockStyle.Fill, Margin = new Padding(2) };
            btn.Text = number; btn.Dock = DockStyle.Fill; btn.BackColor = Color.FromArgb(60, 60, 65); btn.ForeColor = Color.White; btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderColor = Color.Gray; btn.FlatAppearance.BorderSize = 3; btn.Font = new Font("Segoe UI", 24F, FontStyle.Bold); btn.Cursor = Cursors.Hand;
            statusLbl.Text = "Empty"; statusLbl.Dock = DockStyle.Bottom; statusLbl.Height = 18; statusLbl.ForeColor = Color.Gray; statusLbl.Font = new Font("Segoe UI", 8F, FontStyle.Bold); statusLbl.TextAlign = ContentAlignment.MiddleCenter;
            container.Controls.Add(btn); container.Controls.Add(statusLbl);
            grid.Controls.Add(container, col, 0);
        }

        private void AddSensorRow(TableLayoutPanel parent, int row, string name, string valText, string qualText, Label lblName, Label lblVal, Label lblQual, Panel trend, Color color)
        {
            Panel cell = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 5) };
            TableLayoutPanel itemLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2 };
            itemLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); itemLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            itemLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F)); itemLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            lblName.Text = name; lblName.ForeColor = Color.White; lblName.Font = new Font("Segoe UI", 10F, FontStyle.Bold); lblName.AutoSize = true;
            lblVal.Text = valText; lblVal.ForeColor = Color.Cyan; lblVal.Font = new Font("Consolas", 11F, FontStyle.Bold); lblVal.TextAlign = ContentAlignment.MiddleRight; lblVal.Dock = DockStyle.Right; lblVal.AutoSize = true;
            lblQual.Text = qualText; lblQual.ForeColor = color; lblQual.Font = new Font("Segoe UI", 9F, FontStyle.Bold); lblQual.AutoSize = true;
            trend.BackColor = Color.FromArgb(45, 45, 48); trend.BorderStyle = BorderStyle.FixedSingle; trend.Dock = DockStyle.Fill;

            itemLayout.Controls.Add(lblName, 0, 0); itemLayout.Controls.Add(lblVal, 1, 0);
            itemLayout.Controls.Add(lblQual, 0, 1); itemLayout.Controls.Add(trend, 1, 1);
            cell.Controls.Add(itemLayout); parent.Controls.Add(cell, 0, row);
        }

        private void StyleSmallButton(Button btn)
        {
            btn.BackColor = Color.FromArgb(60, 60, 65); btn.ForeColor = Color.White; btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 1; btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100); btn.Font = new Font("Segoe UI", 8F, FontStyle.Bold); btn.Cursor = Cursors.Hand;
        }

        // Variable Declarations
        private Panel panelTopBar, panelSamplerStatus, panelBottleControl, panelSensorReadings;
        private Panel panelMapContainer, panelVideoFeed;
        private Panel panelWorkflowVisual, panelMapControls;
        private Panel panelPhTrend, panelTurbidityTrend, panelTemperatureTrend;
        private Label lblTitle, lblConnection, lblSystemTime;
        private Label lblSamplerTitle, lblCurrentBottle, lblBottleProgress;
        private Label lblCurrentLat, lblCurrentLon, lblCurrentAlt, lblNextWaypoint, lblWaypointDistance;
        private Label lblStateIdle, lblStateRinsing, lblStateFilling, lblStateSealed;
        private Label lblBottleControlTitle, lblModeSelector;
        private Label lblBottle1Status, lblBottle2Status, lblBottle3Status, lblBottle4Status;
        private Label lblSensorTitle, lblSelectedBottleInfo, lblPh, lblPhValue, lblPhQuality;
        private Label lblTurbidity, lblTurbidityValue, lblTurbidityQuality;
        private Label lblTemperature, lblTemperatureValue, lblTemperatureQuality;
        private Label lblAltitudeSensor, lblAltitudeValue;
        private Label lblDataLogging, lblLastUpdate, lblSampleCount;
        private Label lblWaypointProgress, lblVideoTitle, lblVideoQuality, lblConnectionQuality;
        private Label lblMissionControl;

        public Button btnModeToggle;
        public Button btnBottle1, btnBottle2, btnBottle3, btnBottle4;
        private Button btnZoomIn, btnZoomOut, btnCenterDrone, btnSnapshot;
        private RadioButton radioAuto, radioManual;
        private ComboBox comboMapType;
        public GMapControl gmapControl;
        public PictureBox pictureBoxVideo;
        private Timer updateTimer, trendUpdateTimer;
        private Button btnFullscreenMap;
        private Button btnFullscreenVideo;
        private ComboBox comboPorts, comboBaud;
        private Button btnConnect;

        private Button btnCenterMap; // Make sure this is NOT btnCenterDrone
       // private Button btnStartSim;

    }
}