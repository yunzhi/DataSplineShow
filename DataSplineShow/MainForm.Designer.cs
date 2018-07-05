namespace DataSplineShow
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.styleManager1 = new DevComponents.DotNetBar.StyleManager(this.components);
            this.oriDataPanel = new DevComponents.DotNetBar.PanelEx();
            this.panelEx1 = new DevComponents.DotNetBar.PanelEx();
            this.TestBtn = new DevComponents.DotNetBar.ButtonX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.ipTextBox = new DevComponents.Editors.IpAddressInput();
            this.startBtn = new DevComponents.DotNetBar.ButtonX();
            this.labelX3 = new DevComponents.DotNetBar.LabelX();
            this.portTextBox = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.groupPanel1 = new DevComponents.DotNetBar.Controls.GroupPanel();
            this.rcvRichTextBox = new DevComponents.DotNetBar.Controls.RichTextBoxEx();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            this.sideNav1 = new DevComponents.DotNetBar.Controls.SideNav();
            this.lenthFFTNavPanel = new DevComponents.DotNetBar.Controls.SideNavPanel();
            this.lenthPosNavPanel = new DevComponents.DotNetBar.Controls.SideNavPanel();
            this.lenthSpeedNavPanel = new DevComponents.DotNetBar.Controls.SideNavPanel();
            this.fullChartNavPanel = new DevComponents.DotNetBar.Controls.SideNavPanel();
            this.sideNavPanel5 = new DevComponents.DotNetBar.Controls.SideNavPanel();
            this.labelX4 = new DevComponents.DotNetBar.LabelX();
            this.comboBoxEx1 = new DevComponents.DotNetBar.Controls.ComboBoxEx();
            this.sideNavItem1 = new DevComponents.DotNetBar.Controls.SideNavItem();
            this.separator1 = new DevComponents.DotNetBar.Separator();
            this.FullChartNav = new DevComponents.DotNetBar.Controls.SideNavItem();
            this.separator2 = new DevComponents.DotNetBar.Separator();
            this.LenthFFTNav = new DevComponents.DotNetBar.Controls.SideNavItem();
            this.LenthPosNav = new DevComponents.DotNetBar.Controls.SideNavItem();
            this.lenthSpeedNav = new DevComponents.DotNetBar.Controls.SideNavItem();
            this.setMentNav = new DevComponents.DotNetBar.Controls.SideNavItem();
            this.expandableSplitter1 = new DevComponents.DotNetBar.ExpandableSplitter();
            this.oriDataPanel.SuspendLayout();
            this.panelEx1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ipTextBox)).BeginInit();
            this.groupPanel1.SuspendLayout();
            this.sideNav1.SuspendLayout();
            this.sideNavPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // styleManager1
            // 
            this.styleManager1.ManagerStyle = DevComponents.DotNetBar.eStyle.Metro;
            this.styleManager1.MetroColorParameters = new DevComponents.DotNetBar.Metro.ColorTables.MetroColorGeneratorParameters(System.Drawing.Color.White, System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154))))));
            // 
            // oriDataPanel
            // 
            this.oriDataPanel.CanvasColor = System.Drawing.SystemColors.Control;
            this.oriDataPanel.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.oriDataPanel.Controls.Add(this.panelEx1);
            this.oriDataPanel.Controls.Add(this.groupPanel1);
            this.oriDataPanel.Controls.Add(this.labelX1);
            this.oriDataPanel.DisabledBackColor = System.Drawing.Color.Empty;
            this.oriDataPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.oriDataPanel.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.oriDataPanel.Location = new System.Drawing.Point(834, 0);
            this.oriDataPanel.Name = "oriDataPanel";
            this.oriDataPanel.Size = new System.Drawing.Size(278, 567);
            this.oriDataPanel.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.oriDataPanel.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.oriDataPanel.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.oriDataPanel.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.oriDataPanel.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.oriDataPanel.Style.GradientAngle = 90;
            this.oriDataPanel.TabIndex = 1;
            // 
            // panelEx1
            // 
            this.panelEx1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelEx1.CanvasColor = System.Drawing.SystemColors.Control;
            this.panelEx1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelEx1.Controls.Add(this.TestBtn);
            this.panelEx1.Controls.Add(this.labelX2);
            this.panelEx1.Controls.Add(this.ipTextBox);
            this.panelEx1.Controls.Add(this.startBtn);
            this.panelEx1.Controls.Add(this.labelX3);
            this.panelEx1.Controls.Add(this.portTextBox);
            this.panelEx1.DisabledBackColor = System.Drawing.Color.Empty;
            this.panelEx1.Location = new System.Drawing.Point(0, 405);
            this.panelEx1.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
            this.panelEx1.Name = "panelEx1";
            this.panelEx1.Size = new System.Drawing.Size(278, 162);
            this.panelEx1.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelEx1.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.panelEx1.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.panelEx1.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.panelEx1.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.panelEx1.Style.GradientAngle = 90;
            this.panelEx1.TabIndex = 8;
            // 
            // TestBtn
            // 
            this.TestBtn.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.TestBtn.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.TestBtn.Location = new System.Drawing.Point(19, 106);
            this.TestBtn.Name = "TestBtn";
            this.TestBtn.Size = new System.Drawing.Size(88, 32);
            this.TestBtn.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.TestBtn.TabIndex = 7;
            this.TestBtn.Text = "测试";
            this.TestBtn.Click += new System.EventHandler(this.TestBtn_Click);
            // 
            // labelX2
            // 
            // 
            // 
            // 
            this.labelX2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX2.Location = new System.Drawing.Point(3, 23);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(104, 23);
            this.labelX2.TabIndex = 2;
            this.labelX2.Text = "服务器IP地址";
            // 
            // ipTextBox
            // 
            this.ipTextBox.AutoOverwrite = true;
            // 
            // 
            // 
            this.ipTextBox.BackgroundStyle.Class = "DateTimeInputBackground";
            this.ipTextBox.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ipTextBox.ButtonFreeText.Shortcut = DevComponents.DotNetBar.eShortcut.F2;
            this.ipTextBox.ButtonFreeText.Visible = true;
            this.ipTextBox.Location = new System.Drawing.Point(104, 23);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.Size = new System.Drawing.Size(159, 26);
            this.ipTextBox.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ipTextBox.TabIndex = 1;
            this.ipTextBox.Value = "127.0.0.1";
            // 
            // startBtn
            // 
            this.startBtn.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.startBtn.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.startBtn.Location = new System.Drawing.Point(153, 106);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(107, 32);
            this.startBtn.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.startBtn.TabIndex = 6;
            this.startBtn.Text = "启动";
            this.startBtn.Click += new System.EventHandler(this.StartBtn_Click);
            // 
            // labelX3
            // 
            // 
            // 
            // 
            this.labelX3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX3.Location = new System.Drawing.Point(3, 61);
            this.labelX3.Name = "labelX3";
            this.labelX3.Size = new System.Drawing.Size(85, 23);
            this.labelX3.TabIndex = 3;
            this.labelX3.Text = "端口号";
            // 
            // portTextBox
            // 
            this.portTextBox.BackColor = System.Drawing.Color.White;
            // 
            // 
            // 
            this.portTextBox.Border.Class = "TextBoxBorder";
            this.portTextBox.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.portTextBox.DisabledBackColor = System.Drawing.Color.White;
            this.portTextBox.ForeColor = System.Drawing.Color.Black;
            this.portTextBox.Location = new System.Drawing.Point(104, 58);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.PreventEnterBeep = true;
            this.portTextBox.Size = new System.Drawing.Size(159, 26);
            this.portTextBox.TabIndex = 4;
            this.portTextBox.Text = "6801";
            // 
            // groupPanel1
            // 
            this.groupPanel1.BackColor = System.Drawing.Color.White;
            this.groupPanel1.CanvasColor = System.Drawing.SystemColors.Control;
            this.groupPanel1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
            this.groupPanel1.Controls.Add(this.rcvRichTextBox);
            this.groupPanel1.DisabledBackColor = System.Drawing.Color.Empty;
            this.groupPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupPanel1.Location = new System.Drawing.Point(0, 41);
            this.groupPanel1.Name = "groupPanel1";
            this.groupPanel1.Size = new System.Drawing.Size(278, 526);
            // 
            // 
            // 
            this.groupPanel1.Style.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground2;
            this.groupPanel1.Style.BackColorGradientAngle = 90;
            this.groupPanel1.Style.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.groupPanel1.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderBottomWidth = 1;
            this.groupPanel1.Style.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.groupPanel1.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderLeftWidth = 1;
            this.groupPanel1.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderRightWidth = 1;
            this.groupPanel1.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.groupPanel1.Style.BorderTopWidth = 1;
            this.groupPanel1.Style.CornerDiameter = 4;
            this.groupPanel1.Style.CornerType = DevComponents.DotNetBar.eCornerType.Rounded;
            this.groupPanel1.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.groupPanel1.Style.TextColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.groupPanel1.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // 
            // 
            this.groupPanel1.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.groupPanel1.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.groupPanel1.TabIndex = 7;
            this.groupPanel1.Text = "接收数据16进制显示";
            // 
            // rcvRichTextBox
            // 
            this.rcvRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // 
            // 
            this.rcvRichTextBox.BackgroundStyle.Class = "RichTextBoxBorder";
            this.rcvRichTextBox.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.rcvRichTextBox.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rcvRichTextBox.Location = new System.Drawing.Point(3, 0);
            this.rcvRichTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
            this.rcvRichTextBox.Name = "rcvRichTextBox";
            this.rcvRichTextBox.Rtf = "{\\rtf1\\ansi\\ansicpg936\\deff0\\deflang1033\\deflangfe2052{\\fonttbl{\\f0\\fnil\\fcharset" +
    "134 \\\'cb\\\'ce\\\'cc\\\'e5;}}\r\n\\viewkind4\\uc1\\pard\\lang2052\\f0\\fs18\\par\r\n}\r\n";
            this.rcvRichTextBox.ScrollBarAppearance = DevComponents.DotNetBar.eScrollBarAppearance.ApplicationScroll;
            this.rcvRichTextBox.Size = new System.Drawing.Size(267, 350);
            this.rcvRichTextBox.TabIndex = 0;
            // 
            // labelX1
            // 
            this.labelX1.BackColor = System.Drawing.SystemColors.ControlDark;
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelX1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelX1.ForeColor = System.Drawing.Color.White;
            this.labelX1.Location = new System.Drawing.Point(0, 0);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(278, 41);
            this.labelX1.TabIndex = 0;
            this.labelX1.Text = "数据显示";
            this.labelX1.TextAlignment = System.Drawing.StringAlignment.Center;
            // 
            // sideNav1
            // 
            this.sideNav1.Controls.Add(this.lenthFFTNavPanel);
            this.sideNav1.Controls.Add(this.lenthPosNavPanel);
            this.sideNav1.Controls.Add(this.lenthSpeedNavPanel);
            this.sideNav1.Controls.Add(this.fullChartNavPanel);
            this.sideNav1.Controls.Add(this.sideNavPanel5);
            this.sideNav1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sideNav1.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.sideNavItem1,
            this.separator1,
            this.FullChartNav,
            this.separator2,
            this.LenthFFTNav,
            this.LenthPosNav,
            this.lenthSpeedNav,
            this.setMentNav});
            this.sideNav1.Location = new System.Drawing.Point(0, 0);
            this.sideNav1.Name = "sideNav1";
            this.sideNav1.Padding = new System.Windows.Forms.Padding(1);
            this.sideNav1.Size = new System.Drawing.Size(834, 567);
            this.sideNav1.TabIndex = 9;
            this.sideNav1.Text = "sideNav1";
            // 
            // lenthFFTNavPanel
            // 
            this.lenthFFTNavPanel.AllowDrop = true;
            this.lenthFFTNavPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lenthFFTNavPanel.Location = new System.Drawing.Point(123, 38);
            this.lenthFFTNavPanel.Name = "lenthFFTNavPanel";
            this.lenthFFTNavPanel.Size = new System.Drawing.Size(706, 528);
            this.lenthFFTNavPanel.TabIndex = 6;
            // 
            // lenthPosNavPanel
            // 
            this.lenthPosNavPanel.AllowDrop = true;
            this.lenthPosNavPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lenthPosNavPanel.Location = new System.Drawing.Point(122, 39);
            this.lenthPosNavPanel.Name = "lenthPosNavPanel";
            this.lenthPosNavPanel.Size = new System.Drawing.Size(707, 527);
            this.lenthPosNavPanel.TabIndex = 10;
            this.lenthPosNavPanel.Visible = false;
            // 
            // lenthSpeedNavPanel
            // 
            this.lenthSpeedNavPanel.AllowDrop = true;
            this.lenthSpeedNavPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lenthSpeedNavPanel.Location = new System.Drawing.Point(122, 39);
            this.lenthSpeedNavPanel.Name = "lenthSpeedNavPanel";
            this.lenthSpeedNavPanel.Size = new System.Drawing.Size(707, 527);
            this.lenthSpeedNavPanel.TabIndex = 14;
            this.lenthSpeedNavPanel.Visible = false;
            // 
            // fullChartNavPanel
            // 
            this.fullChartNavPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fullChartNavPanel.Location = new System.Drawing.Point(122, 39);
            this.fullChartNavPanel.Name = "fullChartNavPanel";
            this.fullChartNavPanel.Size = new System.Drawing.Size(707, 527);
            this.fullChartNavPanel.TabIndex = 2;
            this.fullChartNavPanel.Visible = false;
            // 
            // sideNavPanel5
            // 
            this.sideNavPanel5.Controls.Add(this.labelX4);
            this.sideNavPanel5.Controls.Add(this.comboBoxEx1);
            this.sideNavPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sideNavPanel5.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.sideNavPanel5.Location = new System.Drawing.Point(123, 38);
            this.sideNavPanel5.Name = "sideNavPanel5";
            this.sideNavPanel5.Size = new System.Drawing.Size(660, 575);
            this.sideNavPanel5.TabIndex = 18;
            this.sideNavPanel5.Visible = false;
            // 
            // labelX4
            // 
            this.labelX4.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX4.Location = new System.Drawing.Point(28, 29);
            this.labelX4.Name = "labelX4";
            this.labelX4.Size = new System.Drawing.Size(99, 34);
            this.labelX4.TabIndex = 1;
            this.labelX4.Text = "界面风格";
            // 
            // comboBoxEx1
            // 
            this.comboBoxEx1.DisplayMember = "Text";
            this.comboBoxEx1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxEx1.ForeColor = System.Drawing.Color.Black;
            this.comboBoxEx1.FormattingEnabled = true;
            this.comboBoxEx1.ItemHeight = 28;
            this.comboBoxEx1.Location = new System.Drawing.Point(146, 29);
            this.comboBoxEx1.Name = "comboBoxEx1";
            this.comboBoxEx1.Size = new System.Drawing.Size(269, 34);
            this.comboBoxEx1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.comboBoxEx1.TabIndex = 0;
            this.comboBoxEx1.SelectedIndexChanged += new System.EventHandler(this.ComboBoxEx1_SelectedIndexChanged);
            // 
            // sideNavItem1
            // 
            this.sideNavItem1.IsSystemMenu = true;
            this.sideNavItem1.Name = "sideNavItem1";
            this.sideNavItem1.Symbol = "";
            this.sideNavItem1.Text = "Menu";
            // 
            // separator1
            // 
            this.separator1.FixedSize = new System.Drawing.Size(3, 1);
            this.separator1.Name = "separator1";
            this.separator1.Padding.Bottom = 2;
            this.separator1.Padding.Left = 6;
            this.separator1.Padding.Right = 6;
            this.separator1.Padding.Top = 2;
            this.separator1.SeparatorOrientation = DevComponents.DotNetBar.eDesignMarkerOrientation.Vertical;
            // 
            // FullChartNav
            // 
            this.FullChartNav.Name = "FullChartNav";
            this.FullChartNav.Panel = this.fullChartNavPanel;
            this.FullChartNav.Symbol = "";
            this.FullChartNav.Text = "所有数据图";
            this.FullChartNav.Visible = false;
            // 
            // separator2
            // 
            this.separator2.FixedSize = new System.Drawing.Size(3, 1);
            this.separator2.Name = "separator2";
            this.separator2.Padding.Bottom = 2;
            this.separator2.Padding.Left = 6;
            this.separator2.Padding.Right = 6;
            this.separator2.Padding.Top = 2;
            this.separator2.SeparatorOrientation = DevComponents.DotNetBar.eDesignMarkerOrientation.Vertical;
            // 
            // LenthFFTNav
            // 
            this.LenthFFTNav.Checked = true;
            this.LenthFFTNav.Name = "LenthFFTNav";
            this.LenthFFTNav.Panel = this.lenthFFTNavPanel;
            this.LenthFFTNav.Symbol = "";
            this.LenthFFTNav.Text = "距离FFT图";
            // 
            // LenthPosNav
            // 
            this.LenthPosNav.Name = "LenthPosNav";
            this.LenthPosNav.Panel = this.lenthPosNavPanel;
            this.LenthPosNav.Symbol = "";
            this.LenthPosNav.Text = "距离方位图";
            // 
            // lenthSpeedNav
            // 
            this.lenthSpeedNav.Name = "lenthSpeedNav";
            this.lenthSpeedNav.Panel = this.lenthSpeedNavPanel;
            this.lenthSpeedNav.Symbol = "";
            this.lenthSpeedNav.Text = "距离速度图";
            // 
            // setMentNav
            // 
            this.setMentNav.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
            this.setMentNav.Name = "setMentNav";
            this.setMentNav.Panel = this.sideNavPanel5;
            this.setMentNav.Symbol = "";
            this.setMentNav.Text = "设置";
            // 
            // expandableSplitter1
            // 
            this.expandableSplitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.expandableSplitter1.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.expandableSplitter1.BackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.expandableSplitter1.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBackground;
            this.expandableSplitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.expandableSplitter1.ExpandableControl = this.oriDataPanel;
            this.expandableSplitter1.ExpandFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.expandableSplitter1.ExpandFillColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.expandableSplitter1.ExpandLineColor = System.Drawing.Color.Black;
            this.expandableSplitter1.ExpandLineColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.expandableSplitter1.ForeColor = System.Drawing.Color.Black;
            this.expandableSplitter1.GripDarkColor = System.Drawing.Color.Black;
            this.expandableSplitter1.GripDarkColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.expandableSplitter1.GripLightColor = System.Drawing.Color.White;
            this.expandableSplitter1.GripLightColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.expandableSplitter1.HotBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.expandableSplitter1.HotBackColor2 = System.Drawing.Color.Empty;
            this.expandableSplitter1.HotBackColor2SchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemPressedBackground2;
            this.expandableSplitter1.HotBackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemPressedBackground;
            this.expandableSplitter1.HotExpandFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.expandableSplitter1.HotExpandFillColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.expandableSplitter1.HotExpandLineColor = System.Drawing.Color.Black;
            this.expandableSplitter1.HotExpandLineColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.expandableSplitter1.HotGripDarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.expandableSplitter1.HotGripDarkColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.expandableSplitter1.HotGripLightColor = System.Drawing.Color.White;
            this.expandableSplitter1.HotGripLightColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.expandableSplitter1.Location = new System.Drawing.Point(828, 0);
            this.expandableSplitter1.Name = "expandableSplitter1";
            this.expandableSplitter1.Size = new System.Drawing.Size(6, 567);
            this.expandableSplitter1.Style = DevComponents.DotNetBar.eSplitterStyle.Office2007;
            this.expandableSplitter1.TabIndex = 10;
            this.expandableSplitter1.TabStop = false;
            this.expandableSplitter1.ExpandedChanged += new DevComponents.DotNetBar.ExpandChangeEventHandler(this.ExpandableSplitter1_ExpandedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1112, 567);
            this.Controls.Add(this.expandableSplitter1);
            this.Controls.Add(this.sideNav1);
            this.Controls.Add(this.oriDataPanel);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "DataSplineShow";
            this.oriDataPanel.ResumeLayout(false);
            this.panelEx1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ipTextBox)).EndInit();
            this.groupPanel1.ResumeLayout(false);
            this.sideNav1.ResumeLayout(false);
            this.sideNav1.PerformLayout();
            this.sideNavPanel5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DevComponents.DotNetBar.StyleManager styleManager1;
        private DevComponents.DotNetBar.PanelEx oriDataPanel;
        private DevComponents.DotNetBar.Controls.SideNav sideNav1;
        private DevComponents.DotNetBar.Controls.SideNavPanel fullChartNavPanel;
        private DevComponents.DotNetBar.Controls.SideNavItem sideNavItem1;
        private DevComponents.DotNetBar.Separator separator1;
        private DevComponents.DotNetBar.Controls.SideNavItem FullChartNav;
        private DevComponents.DotNetBar.ExpandableSplitter expandableSplitter1;
        private DevComponents.DotNetBar.LabelX labelX1;
        private DevComponents.Editors.IpAddressInput ipTextBox;
        private DevComponents.DotNetBar.Controls.TextBoxX portTextBox;
        private DevComponents.DotNetBar.LabelX labelX3;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.ButtonX startBtn;
        private DevComponents.DotNetBar.Controls.GroupPanel groupPanel1;
        private DevComponents.DotNetBar.PanelEx panelEx1;
        private DevComponents.DotNetBar.Controls.RichTextBoxEx rcvRichTextBox;
        private DevComponents.DotNetBar.ButtonX TestBtn;
        private DevComponents.DotNetBar.Separator separator2;
        private DevComponents.DotNetBar.Controls.SideNavPanel lenthSpeedNavPanel;
        private DevComponents.DotNetBar.Controls.SideNavPanel lenthPosNavPanel;
        private DevComponents.DotNetBar.Controls.SideNavPanel lenthFFTNavPanel;
        private DevComponents.DotNetBar.Controls.SideNavItem LenthFFTNav;
        private DevComponents.DotNetBar.Controls.SideNavItem LenthPosNav;
        private DevComponents.DotNetBar.Controls.SideNavItem lenthSpeedNav;
        private DevComponents.DotNetBar.Controls.SideNavPanel sideNavPanel5;
        private DevComponents.DotNetBar.Controls.SideNavItem setMentNav;
        private DevComponents.DotNetBar.LabelX labelX4;
        private DevComponents.DotNetBar.Controls.ComboBoxEx comboBoxEx1;
    }
}

