using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Charts;
using DevComponents.DotNetBar.Charts.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataSplineShow
{
    public partial class MainForm : OfficeForm
    {

        #region Private variables

        private ChartControl LenthPosChartControl = new ChartControl();
        private ChartSeries lenthPosChartSeries = new ChartSeries();

        private Socket dataSocket = null;
        private Thread tcpRcvThread = null;
        private bool isStarted = false;

        //定义一个1M的内存缓冲区，用于临时性存储接收到的消息  
        byte[] arrRecvmsg = new byte[1024 * 1024];
        ArrayList arrRecvmsglist = new ArrayList(0);

        //private int iDataLenth;
        private byte[] bPacketHead = new byte[2] { 0xFA, 0xFA };
        private byte[] bLenthFFTData = new byte[516]; //256*2 + 4 = 516
        private byte[] bPacketTail = new byte[2] { 0xEB, 0xEB };
        private static int LeastPacketLenth = 516;
        private static int iPointNum = 256;

        private Int32[] fftData = new Int32[256];

        private EventWaitHandle ReadyEvent;

        private bool no_debug_output = false;

        #endregion

        public MainForm()
        {
            SuspendLayout();

            InitializeComponent();

            //Control.CheckForIllegalCrossThreadCalls = false;

            comboBoxEx1.Items.AddRange(new object[] { eStyle.Office2013, eStyle.OfficeMobile2014, eStyle.Office2010Blue,
                eStyle.Office2010Silver, eStyle.Office2010Black, eStyle.VisualStudio2010Blue, eStyle.VisualStudio2012Light,
                eStyle.VisualStudio2012Dark, eStyle.Office2007Blue, eStyle.Office2007Silver, eStyle.Office2007Black});
            comboBoxEx1.SelectedIndex = 0;

            sideNav1.EnableMaximize = false;
            sideNav1.EnableClose = false;

            LenthPosNavInit();

            ResumeLayout(false);

            ReadyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "READY");

            Thread LenthFFTDrawThread = new Thread(LenthFFTDrawThreadFunction);
            LenthFFTDrawThread.Start();

            //LenthPosChartControl.SeriesDataBindingComplete += ChartControl_SeriesDataBindingComplete;
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.SelectedItem == null) return;
            eStyle style = (eStyle)comboBoxEx1.SelectedItem;
            if (styleManager1.ManagerStyle != style)
                styleManager1.ManagerStyle = style;
        }

        #region LenthPosNavInit
        private void LenthPosNavInit()
        {
            LenthPosChartControlInit();

            this.lenthPosNavPanel.Controls.Add(this.LenthPosChartControl);
        }

        #region LenthPosChartControlInit
        /// <summary>
        /// Initializes the LenthPosChartControlInit.
        /// </summary>
        private void LenthPosChartControlInit()
        {
            this.LenthPosChartControl.Name = "LenthPosChartControl";
            this.LenthPosChartControl.Location = new System.Drawing.Point(10, 15);
            this.LenthPosChartControl.Size = new System.Drawing.Size(750, 500);
            this.LenthPosChartControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.LenthPosChartControl.MinimumSize = new System.Drawing.Size(480, 320);

            this.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            this.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            this.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            this.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            this.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            this.LenthPosChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            this.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            this.LenthPosChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            this.LenthPosChartControl.Padding = new System.Windows.Forms.Padding(5);

            LenthPosChartXyInit();
        }

        #region LenthPosChartXyInit

        /// <summary>
        /// Adds a new chart to the ChartControl
        /// </summary>
        private void LenthPosChartXyInit()
        {
            ChartXy lenthPosChartXy = new ChartXy();

            lenthPosChartXy.Name = "LenthFFTChart";
            lenthPosChartXy.MinContentSize = new Size(480, 320);
            lenthPosChartXy.ChartLineDisplayMode = ChartLineDisplayMode.DisplaySpline;

            // Setup our Crosshair display.
            lenthPosChartXy.ChartCrosshair.HighlightPoints = true;
            lenthPosChartXy.ChartCrosshair.AxisOrientation = AxisOrientation.X;
            lenthPosChartXy.ChartCrosshair.ShowValueXLine = true;
            lenthPosChartXy.ChartCrosshair.ShowValueYLine = true;
            lenthPosChartXy.ChartCrosshair.ShowCrosshairLabels = true;

            // Let's only display the point nearest to the mouse cursor
            // that intersects with the crosshair line.
            lenthPosChartXy.ChartCrosshair.CrosshairLabelMode = CrosshairLabelMode.NearestSeries;
            lenthPosChartXy.ChartCrosshair.CrosshairVisualStyle.Background = new Background(Color.White);

            // Setup various styles for the chart...
            SetupChartStyle(lenthPosChartXy);
            SetupContainerStyle(lenthPosChartXy);
            SetupChartAxes(lenthPosChartXy);
            SetupChartLegend(lenthPosChartXy);

            // Add a chart title and associated series.

            AddChartTitle(lenthPosChartXy);
            InitChartSeries(lenthPosChartXy);

            // And finally, add the chart to the ChartContainers
            // collection of chart elements.

            this.LenthPosChartControl.ChartPanel.ChartContainers.Add(lenthPosChartXy);
        }

        #region SetupChartAxes

        /// <summary>
        /// Sets up the chart axes.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupChartAxes(ChartXy chartXy)
        {
            // X Axis

            ChartAxis axis = chartXy.AxisX;

            //axis.GridSpacing = 10;
            axis.MinGridInterval = 20;

            axis.AxisMargins = 5;
            axis.AxisFarMargin = 5;
            axis.AxisNearMargin = 5;

            // Set our axis title appropriately.

            axis.Title.Text = "频率/256（频率=300K）";
            axis.Title.ChartTitleVisualStyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(4, 0, 4, 0);
            axis.Title.ChartTitleVisualStyle.Font = new Font("微软雅黑", 10);
            axis.Title.ChartTitleVisualStyle.TextColor = Color.Navy;
            axis.Title.ChartTitleVisualStyle.Alignment = Alignment.BottomCenter;

            axis.MinorTickmarks.TickmarkCount = 0;
            axis.MajorTickmarks.StaggerLabels = true;
            axis.MajorTickmarks.LabelSkip = 2;

            axis.MajorGridLines.GridLinesVisualStyle.LineColor = Color.Gainsboro;
            axis.MinorGridLines.GridLinesVisualStyle.LineColor = Color.WhiteSmoke;

            // Set our alternate background to a nice MidnightBlue.

            axis.ChartAxisVisualStyle.AlternateBackground = new Background(Color.FromArgb(20, Color.MidnightBlue));

            axis.UseAlternateBackground = true;

            // Y Axis

            axis = chartXy.AxisY;

            //axis.AxisMargins = 5;
            axis.AxisFarMargin = 30;
            axis.AxisNearMargin = 10;

            axis.GridSpacing = 20;
            axis.MinGridInterval = 50;

            axis.AxisAlignment = AxisAlignment.Far;
            axis.MinorTickmarks.TickmarkCount = 0;

            // Set our axis title appropriately.
            
            axis.Title.Text = "距离FFT值";
            axis.Title.ChartTitleVisualStyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(4, 0, 4, 0);
            axis.Title.ChartTitleVisualStyle.Font = new Font("微软雅黑", 10);
            axis.Title.ChartTitleVisualStyle.TextColor = Color.Navy;
            axis.Title.ChartTitleVisualStyle.Alignment = Alignment.MiddleCenter;

            axis.MajorGridLines.GridLinesVisualStyle.LineColor = Color.Gainsboro;
            axis.MinorGridLines.GridLinesVisualStyle.LineColor = Color.WhiteSmoke;

            axis.ChartAxisVisualStyle.AlternateBackground = new Background(Color.FromArgb(30, Color.DarkKhaki));
        }

        #endregion

        #region SetupChartStyle

        /// <summary>
        /// Sets up the chart style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupChartStyle(ChartXy chartXy)
        {
            ChartXyVisualStyle cstyle = chartXy.ChartVisualStyle;

            cstyle.Background = new Background(Color.White);
            cstyle.BorderThickness = new Thickness(1);
            cstyle.BorderColor = new BorderColor(Color.Navy);

            cstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(6);

            ChartSeriesVisualStyle sstyle = chartXy.ChartSeriesVisualStyle;
            PointMarkerVisualStyle pstyle = sstyle.MarkerHighlightVisualStyle;

            pstyle.Background = new Background(Color.Yellow);
            pstyle.Type = PointMarkerType.Ellipse;
            pstyle.Size = new Size(15, 15);

            CrosshairVisualStyle chstyle = chartXy.ChartCrosshair.CrosshairVisualStyle;

            chstyle.ValueXLineStyle.LineColor = Color.Navy;
            chstyle.ValueXLineStyle.LinePattern = LinePattern.Dot;

            chstyle.ValueYLineStyle.LineColor = Color.Navy;
            chstyle.ValueYLineStyle.LinePattern = LinePattern.Dot;
        }

        #endregion

        #region SetupContainerStyle

        /// <summary>
        /// Sets up the chart's container style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupContainerStyle(ChartXy chartXy)
        {
            ContainerVisualStyle dstyle = chartXy.ContainerVisualStyles.Default;

            dstyle.Background = new Background(Color.White);
            dstyle.BorderColor = new BorderColor(Color.DimGray);
            dstyle.BorderThickness = new Thickness(1);

            dstyle.DropShadow.Enabled = Tbool.True;
            dstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(1);
        }

        #endregion

        #region SetupChartLegend

        /// <summary>
        /// Sets up the Legend style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void SetupChartLegend(ChartXy chartXy)
        {
            ChartLegend legend = chartXy.Legend;

            legend.ShowCheckBoxes = true;

            legend.Placement = Placement.Inside;
            legend.Alignment = Alignment.TopRight;
            legend.Direction = Direction.LeftToRight;

            // Align vertical items, and permit the legend to only use
            // up to 50% of the available chart width;

            legend.AlignVerticalItems = true;
            legend.MaxHorizontalPct = 50;

            ChartLegendVisualStyle lstyle = legend.ChartLegendVisualStyles.Default;

            lstyle.BorderThickness = new Thickness(0);
            lstyle.BorderColor = new BorderColor(Color.White);

            lstyle.Margin = new DevComponents.DotNetBar.Charts.Style.Padding(8);
            lstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(4);

            //lstyle.Background = new Background(Color.FromArgb(200, Color.White));
        }

        #endregion

        #region AddChartTitle

        /// <summary>
        /// Sets up the chart title style.
        /// </summary>
        /// <param name="chartXy"></param>
        private void AddChartTitle(ChartXy chartXy)
        {
            // Add 2 titles for the chart.  They will both be centered and
            // set to automatically wrap if needed.

            // Title 1.

            ChartTitle title = new ChartTitle();

            title.Text = "距离FFT曲线";
            title.XyAlignment = XyAlignment.Top;

            ChartTitleVisualStyle tstyle = title.ChartTitleVisualStyle;

            tstyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(8, 8, 8, 0);
            tstyle.Font = new Font("微软雅黑", 16);
            tstyle.TextColor = Color.Navy;
            tstyle.Alignment = Alignment.MiddleCenter;

            chartXy.Titles.Add(title);
        }

        #endregion

        #region InitChartSeries

        /// <summary>
        /// Adds 4 food series to the given chart.
        /// </summary>
        /// <param name="chartXy"></param>
        private void InitChartSeries(ChartXy chartXy)
        {
            //UpdateChartSeries(chartXy);

            lenthPosChartSeries = new ChartSeries("lenthFFT", SeriesType.Line);

            chartXy.ChartSeries.Add(lenthPosChartSeries);

            lenthPosChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthPosChartSeries.SeriesPoints.Clear();
            lenthPosChartSeries.SeriesPoints.Add(new SeriesPoint(0,0));
            lenthPosChartSeries.SeriesPoints.Add(new SeriesPoint(150000, 65535));
            lenthPosChartSeries.SeriesPoints.Add(new SeriesPoint(300000, 0));

            lenthPosChartSeries.ChartSeriesVisualStyle.SplineAreaBackground = new Background(Color.Black);

            lenthPosChartSeries.ShowCheckBoxInLegend = true;
            lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
            lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Red;
            lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Red);

            lenthPosChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Green;

            /*
            Random ra = new Random();

            for (Int32 i = 0; i < 256; i++)
            {
                double xPointValue = (300000 / 256) * i;
                lenthPosChartSeries.SeriesPoints.Add(new SeriesPoint(xPointValue, ra.Next(Convert.ToInt32(i), 65535)));
            }
            */

        }

        #region UpdateChartSeries

        /// <summary>
        /// Updates series for each Biorhythm series.
        /// </summary>
        private void UpdateChartSeries(ChartXy chartXy)
        {
            //double dsb = GetDaysSinceBirth();

            //UpdateBioSeries(chartXy, dsb, "Physical", 23);

            // Add/update the Average series.

            //UpdateAverageSeries(chartXy);

            // Lets make sure the "Average" series is displayed first,
            // so thast it doesn't display on top of the other rhythm series.

            //chartXy.SeriesDisplayOrder = SeriesDisplayOrder.Reverse;

            // Since we went both forward and backward in the rhythm
            // display, let's center the display on today's date.

            //chartXy.AxisX.ReferenceLines["Today"].EnsureVisible(true);
        }

        #region GetDaysSinceBirth

        /// <summary>
        /// Gets the number ow whole days since birth.
        /// </summary>
        /// <returns></returns>
        private double GetDaysSinceBirth()
        {
            DateTime bdate = Convert.ToDateTime("2010-01-01 00:00:00");

            TimeSpan ts = DateTime.Now - bdate;

            return (Math.Floor(ts.TotalDays));
        }

        #endregion

        #region UpdateBioSeries

        /// <summary>
        /// Adds individual biorhythm series.
        /// </summary>
        private void UpdateBioSeries(
            ChartXy chartXy, double dsb, string rhythm, int cycle)
        {
            ChartSeries series = chartXy.ChartSeries[rhythm];

            if (series == null)
            {
                series = new ChartSeries(rhythm, SeriesType.Line);

                chartXy.ChartSeries.Add(series);
            }
            series.ChartSeriesVisualStyle.LineStyle.LineWidth = 5;
            series.SeriesPoints.Clear();

            // Add SeriesPoints for the rhythm.

            DateTime now = DateTime.Now.Date;
            DateTime start = now.AddDays(-60);
            Random ra = new Random();

            for (double i = dsb - 60; i < dsb + 60; i++)
            {
                double percent = Math.Sin((2 * Math.PI * i) / cycle) * 100;

                string s = start.Month + "/" + start.Day;

                series.SeriesPoints.Add(new SeriesPoint(s, ra.Next(Convert.ToInt32(i), 65535)));

                start = start.AddDays(1);
            }

            // Add the series to the chart.

        }

        #endregion

        #region UpdateAverageSeries

        private void UpdateAverageSeries(ChartXy chartXy)
        {
            ChartSeries series = chartXy.ChartSeries["Average"];

            if (series == null)
            {
                series = new ChartSeries("Average", SeriesType.Line);

                series.ChartLineDisplayMode = ChartLineDisplayMode.DisplayPoints | ChartLineDisplayMode.DisplaySpline;
                series.ChartLineAreaDisplayMode = ChartLineAreaDisplayMode.DisplaySpline;
                series.AreaBaseValue = 0;

                Background back = new Background(Color.FromArgb(255, Color.Red));
                back.HatchFillType = HatchFillType.LightVertical;

                series.ChartSeriesVisualStyle.SplineAreaBackground = back;

                series.ShowCheckBoxInLegend = true;
                series.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
                series.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Red;
                series.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Red);

                series.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Red;

                chartXy.ChartSeries.Add(series);
            }

            series.SeriesPoints.Clear();

            double[] valuesY = new double[60 * 2];

            int count = 0;

            foreach (ChartSeries cs in chartXy.ChartSeries)
            {
                if (cs != series)
                {
                    if (cs.IsDisplayed == true)
                    {
                        count++;

                        for (int i = 0; i < cs.SeriesPoints.Count; i++)
                            valuesY[i] += (double)cs.SeriesPoints[i].ValueY[0];
                    }
                }
            }

            if (count > 1)
            {
                ChartSeries csp = chartXy.ChartSeries["Physical"];

                for (int i = 0; i < valuesY.Length; i++)
                {
                    valuesY[i] = valuesY[i] / count;

                    SeriesPoint sp = new SeriesPoint(
                        csp.SeriesPoints[i].ValueX, valuesY[i]);

                    series.SeriesPoints.Add(sp);
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #endregion


        #endregion

        #endregion

        #region LenthFFTNavPanel
        /// <summary>
        /// lenthfft chart 位于 lenthFFTNavPanel中
        /// 这个部分的代码主要实现这个chart的绘制
        /// </summary>

        private void LenthFFTNavPanelInit()
        {

        }






        #endregion



        private void socket_rev()
        {
            //持续监听服务端发来的消息 
            while (true)
            {
                try
                {
                    if (dataSocket != null && dataSocket.Connected)
                    {
                        //将客户端套接字接收到的数据存入内存缓冲区，并获取长度  
                        int length = dataSocket.Receive(arrRecvmsg);
                        if (length != 0)
                        {
                            DateTime begin = DateTime.Now;
                            checkAnalysisRcvPacket(arrRecvmsg, length);
                            DateTime end = DateTime.Now;
                            Console.WriteLine("period secs: " + ExecDateDiff(begin, end));
                            //Console.WriteLine("rcv: " + length);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("远程服务器已经中断连接, " + ex.Message + "\r\n");
                }
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (ipTextBox.Text != "" || portTextBox.Text != "")
            {
                if (isStarted == false)
                {
                    try
                    {
                        dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        dataSocket.Connect(IPAddress.Parse(ipTextBox.Text), Convert.ToInt16(portTextBox.Text));

                        if (dataSocket.Connected == true)
                        {
                            startBtn.Text = "停止";
                            ipTextBox.Enabled = false;
                            portTextBox.Enabled = false;

                            tcpRcvThread = new Thread(socket_rev);
                            tcpRcvThread.IsBackground = true;
                            tcpRcvThread.Start();

                            isStarted = true;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("TCP建立连接失败");
                    }
                }
                else
                {
                    tcpRcvThread.Abort();
                    dataSocket.Close();

                    startBtn.Text = "开始";
                    ipTextBox.Enabled = true;
                    portTextBox.Enabled = true;

                    isStarted = false;
                }
            }
        }

        private bool checkAnalysisRcvPacket(byte[] rcvPacket, int len)
        {
            for(int i = 0; i < len; i++)
                arrRecvmsglist.Add(rcvPacket[i]);

            if( arrRecvmsglist.Count < LeastPacketLenth)
            {
                Console.WriteLine("len {0} < LeastPacketLenth {1}", len, LeastPacketLenth);
                return false;
            }

            int index, head_index;
            for (index = 0; index < arrRecvmsglist.Count; index++)
            {
                head_index = arrRecvmsglist.IndexOf(bPacketHead[0]);

                if( head_index + LeastPacketLenth > arrRecvmsglist.Count )
                {
                    Console.WriteLine("data lenth is too short, head_index = " + len + "Count = " + arrRecvmsglist.Count);
                    //AppendColorText2RichBox("effective data is too short, count: " + (arrRecvmsglist.Count - head_index) + "\r\n", false);
                    return false;
                }

                arrRecvmsglist.CopyTo(head_index, bLenthFFTData, 0, LeastPacketLenth);

                if ( bLenthFFTData[0] == bPacketHead[0] && bLenthFFTData[1] == bPacketHead[1] &&
                     bLenthFFTData[LeastPacketLenth - 2] == bPacketTail[0] &&
                     bLenthFFTData[LeastPacketLenth - 1] == bPacketTail[1] )
                {
                    for (int i = 0; i < iPointNum; i++)
                    {
                        fftData[i] = (Int32)((bLenthFFTData[2 + i * 2]) | (Int32)(bLenthFFTData[2 + i * 2 + 1] << 8));
                    }
                    ReadyEvent.Set();

                    if( no_debug_output == true )
                    {
                        Thread.Sleep(20);
                    }
                    else
                    {
                        for (int i = 0; i < iPointNum + 2; i++)
                        {
                            AppendColorText2RichBox(bLenthFFTData[i * 2].ToString("X2") + " " + bLenthFFTData[i * 2 + 1].ToString("X2") + " ");
                        }
                        AppendColorText2RichBox("\r\n\r\n");
                    }


                    arrRecvmsglist.RemoveRange(0, head_index + LeastPacketLenth);
                }
                else
                {
                    if (head_index == 0)
                        arrRecvmsglist.RemoveAt(0);
                    else
                        arrRecvmsglist.RemoveRange(0, head_index);
                }
            }


            return true;
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            Random ra = new Random(DateTime.Now.Second);

            ra.NextBytes(bLenthFFTData);
            bLenthFFTData[0] = bPacketHead[0];
            bLenthFFTData[1] = bPacketHead[1];
            bLenthFFTData[LeastPacketLenth - 2] = bPacketTail[0];
            bLenthFFTData[LeastPacketLenth - 1] = bPacketTail[1];
            
            for (int i = 0; i < iPointNum; i++)
            {
                fftData[i] = (Int32)((bLenthFFTData[2 + i * 2]) | (Int32)(bLenthFFTData[2 + i * 2 + 1] << 8));
            }
            ReadyEvent.Set();
            
            for (int i = 0; i < iPointNum + 2; i++)
            {
                AppendColorText2RichBox(bLenthFFTData[i * 2].ToString("X2") + " " + bLenthFFTData[i * 2 + 1].ToString("X2") + " ");
            }
            AppendColorText2RichBox("\r\n\r\n");

        }

        public void AppendColorText2RichBox(string text, bool useDefaultColorFlag = true)
        {
            this.Invoke((EventHandler)(delegate
            {
                if (useDefaultColorFlag == true)
                {
                    rcvRichTextBox.AppendText(text);
                }
                else
                {
                    rcvRichTextBox.SelectionStart = rcvRichTextBox.TextLength;
                    rcvRichTextBox.SelectionLength = 0;
                    rcvRichTextBox.SelectionColor = Color.Red; //设置发生错误时字体颜色为红色
                    rcvRichTextBox.AppendText(text);
                    rcvRichTextBox.SelectionColor = rcvRichTextBox.ForeColor;
                }

                rcvRichTextBox.Select(rcvRichTextBox.TextLength, 0);
                rcvRichTextBox.ScrollToCaret();
            }));
        }

        private void LenthFFTDrawThreadFunction()
        {
            while(true)
            {
                if ( ReadyEvent.WaitOne(500) )
                {
                    lenthPosChartSeries.SeriesPoints.Clear();
                    Int32[] drawData = new Int32[256];

                    fftData.CopyTo(drawData, 0);

                    drawData[0] = 1;
                    drawData[1] = 1;
                    
                    for (int i = 0; i < iPointNum; i++)
                    {
                        double xPointValue = (300000 / iPointNum) * i;
                        lenthPosChartSeries.SeriesPoints.Add(new SeriesPoint(xPointValue, 20*Math.Log10(drawData[i])));
                    }

                    ReadyEvent.Reset();
                }
            }
            
        }

        protected override void OnClosed(EventArgs e)
        {
            System.Environment.Exit(0);
        }

        /// <summary>
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.TotalMilliseconds.ToString();
        }

        private void expandableSplitter1_ExpandedChanged(object sender, ExpandedChangeEventArgs e)
        {
            no_debug_output = (no_debug_output == true) ? false : true ;
        }
    }
}
