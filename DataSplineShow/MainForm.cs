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

        private ChartControl LenthFFTChartControl;

        private Socket dataSocket = null;
        private Thread tcpRcvThread = null;
        private bool   isStarted = false;

        //定义一个1M的内存缓冲区，用于临时性存储接收到的消息  
        byte[] arrRecvmsg = new byte[1024 * 1024];
        ArrayList arrRecvmsglist = new ArrayList(0);

        private byte[] lenthFFT1PacketHead = new byte[2] { 0xFA, 0xFA };
        private byte[] lenthFFT2PacketHead = new byte[2] { 0xFB, 0xFB };

        private byte[] lenthFFTPacketTail  = new byte[2] { 0xEB, 0xEB };

        private static int LeastPacketLenth = 518; // 256*2 + 1*2 + 4
        private static int iPointNum        = 257; //256 + 1

        private byte[] lenthFFT1Data = new byte[LeastPacketLenth];
        private byte[] lenthFFT2Data = new byte[LeastPacketLenth];

        private Int32[] lenthFFT1value = new Int32[iPointNum];
        private Int32[] lenthFFT2value = new Int32[iPointNum];

        //private EventWaitHandle ReadyEvent;

        private bool no_debug_output = false;

        private Thread LenthFFTDrawThread;

        private InDataProcess AllInDataProcess = null;

        #endregion

        public MainForm()
        {
            SuspendLayout();

            InitializeComponent();

            comboBoxEx1.Items.AddRange(new object[] { eStyle.Office2013, eStyle.OfficeMobile2014, eStyle.Office2010Blue,
                eStyle.Office2010Silver, eStyle.Office2010Black, eStyle.VisualStudio2010Blue, eStyle.VisualStudio2012Light,
                eStyle.VisualStudio2012Dark, eStyle.Office2007Blue, eStyle.Office2007Silver, eStyle.Office2007Black});
            comboBoxEx1.SelectedIndex = 0;

            sideNav1.EnableMaximize = false;
            sideNav1.EnableClose = false;

            LenthFFTNavInit();

            ResumeLayout(false);

            //ReadyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, "READY");

            LenthFFTDrawThread = new Thread(LenthFFTDrawThreadFunction);
            LenthFFTDrawThread.Start();

            AllInDataProcess = new InDataProcess();

        }

        private void ComboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.SelectedItem == null) return;
            eStyle style = (eStyle)comboBoxEx1.SelectedItem;
            if (styleManager1.ManagerStyle != style)
                styleManager1.ManagerStyle = style;
        }

        #region LenthFFTNavInit
        private void LenthFFTNavInit()
        {
            LenthFFTChartControlInit();

            this.lenthFFTNavPanel.Controls.Add(this.LenthFFTChartControl);
        }
        #endregion

        #region LenthFFTChartControlInit
        /// <summary>
        /// Initializes the LenthFFTChartControlInit.
        /// </summary>
        private void LenthFFTChartControlInit()
        {
            this.LenthFFTChartControl = new ChartControl();
            this.LenthFFTChartControl.Name = "LenthFFTChartControl";
            this.LenthFFTChartControl.Location = new System.Drawing.Point(10, 15);
            this.LenthFFTChartControl.Size = new System.Drawing.Size(750, 500);
            this.LenthFFTChartControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.LenthFFTChartControl.MinimumSize = new System.Drawing.Size(480, 320);

            this.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            this.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            this.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ArrowBackground = new Background(Color.AliceBlue);
            this.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.MouseOver.ThumbBackground = new Background(Color.AliceBlue);
            this.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            this.LenthFFTChartControl.DefaultVisualStyles.HScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            this.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ArrowBackground = new Background(Color.White);
            this.LenthFFTChartControl.DefaultVisualStyles.VScrollBarVisualStyles.SelectedMouseOver.ThumbBackground = new Background(Color.White);
            this.LenthFFTChartControl.Padding = new System.Windows.Forms.Padding(5);

            LenthFFTChartXyInit();
        }
        #endregion

        #region LenthFFTChartXyInit

        /// <summary>
        /// Adds a new chart to the ChartControl
        /// </summary>
        private void LenthFFTChartXyInit()
        {
            ChartXy LenthFFTChartXy = new ChartXy();

            LenthFFTChartXy.Name = "LenthFFTChart";
            LenthFFTChartXy.MinContentSize = new Size(480, 320);
            LenthFFTChartXy.ChartLineDisplayMode = ChartLineDisplayMode.DisplaySpline;

            // Setup our Crosshair display.
            LenthFFTChartXy.ChartCrosshair.HighlightPoints = true;
            LenthFFTChartXy.ChartCrosshair.AxisOrientation = AxisOrientation.X;
            LenthFFTChartXy.ChartCrosshair.ShowValueXLine = true;
            LenthFFTChartXy.ChartCrosshair.ShowValueYLine = true;
            LenthFFTChartXy.ChartCrosshair.ShowCrosshairLabels = true;

            // Let's only display the point nearest to the mouse cursor
            // that intersects with the crosshair line.
            LenthFFTChartXy.ChartCrosshair.CrosshairLabelMode = CrosshairLabelMode.NearestSeries;
            LenthFFTChartXy.ChartCrosshair.CrosshairVisualStyle.Background = new Background(Color.White);

            // Setup various styles for the chart...
            SetupChartStyle(LenthFFTChartXy);
            SetupContainerStyle(LenthFFTChartXy);
            SetupChartAxes(LenthFFTChartXy);
            SetupChartLegend(LenthFFTChartXy);

            // Add a chart title and associated series.

            AddChartTitle(LenthFFTChartXy);
            InitChartSeries(LenthFFTChartXy);

            // And finally, add the chart to the ChartContainers
            // collection of chart elements.

            this.LenthFFTChartControl.ChartPanel.ChartContainers.Add(LenthFFTChartXy);
        }

        #endregion

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
            ChartSeries lenthFF1ChartSeries = new ChartSeries("lenthFFT1", SeriesType.Line);
            lenthFF1ChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFF1ChartSeries.SeriesPoints.Clear();
            lenthFF1ChartSeries.SeriesPoints.Add(new SeriesPoint(0, 0));
            lenthFF1ChartSeries.SeriesPoints.Add(new SeriesPoint(150000, 65535));
            lenthFF1ChartSeries.SeriesPoints.Add(new SeriesPoint(300000, 0));
            lenthFF1ChartSeries.ChartSeriesVisualStyle.SplineAreaBackground = new Background(Color.Black);
            lenthFF1ChartSeries.ShowCheckBoxInLegend = true;
            lenthFF1ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
            lenthFF1ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Green;
            lenthFF1ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Blue);
            lenthFF1ChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Green;
            chartXy.ChartSeries.Add(lenthFF1ChartSeries);

            ChartSeries lenthFFT1AverageChartSeries = new ChartSeries("lenthFFT1Average", SeriesType.Line);
            lenthFFT1AverageChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFFT1AverageChartSeries.SeriesPoints.Clear();
            lenthFFT1AverageChartSeries.ChartSeriesVisualStyle.SplineAreaBackground = new Background(Color.Black);
            lenthFFT1AverageChartSeries.ShowCheckBoxInLegend = true;
            //lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Type = PointMarkerType.Ellipse;
            //lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Red;
            //lenthPosChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Red);
            lenthFFT1AverageChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Green;
            chartXy.ChartSeries.Add(lenthFFT1AverageChartSeries);

            ChartSeries lenthFFT2ChartSeries = new ChartSeries("lenthFFT2", SeriesType.Line);
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFFT2ChartSeries.ShowCheckBoxInLegend = true;
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.MarkerEmptyVisualStyle.Type = PointMarkerType.Star;
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.BorderColor = Color.Orange;
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.MarkerVisualStyle.Background = new Background(Color.Blue);
            lenthFFT2ChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Orange;
            chartXy.ChartSeries.Add(lenthFFT2ChartSeries);

            ChartSeries lenthFFT2AverageChartSeries = new ChartSeries("lenthFFT2Average", SeriesType.Line);
            lenthFFT2AverageChartSeries.ChartSeriesVisualStyle.LineStyle.LineWidth = 2;
            lenthFFT2AverageChartSeries.ShowCheckBoxInLegend = true;
            lenthFFT2AverageChartSeries.ChartSeriesVisualStyle.SplineStyle.LineColor = Color.Orange;
            chartXy.ChartSeries.Add(lenthFFT2AverageChartSeries);

        }

        #endregion

        private void SocketRecvProcess()
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
                            CheckAnalysisRcvPacket(arrRecvmsg, length);
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

        private void StartBtn_Click(object sender, EventArgs e)
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

                            tcpRcvThread = new Thread(SocketRecvProcess);
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

        private bool CheckAnalysisRcvPacket(byte[] rcvPacket, int len)
        {
            Console.WriteLine("arrRecvmsglist.Count = " + arrRecvmsglist.Count + ", len = " + len);

            for (int i = 0; i < len; i++)
                arrRecvmsglist.Add(rcvPacket[i]);

            if( arrRecvmsglist.Count < LeastPacketLenth)
            {
                Console.WriteLine("len {0} < LeastPacketLenth {1}", len, LeastPacketLenth);
                return false;
            }

            int index, head_index, lenthFFT2HeadIndex;
            for (index = 0; index < arrRecvmsglist.Count; index++)
            {
                head_index = arrRecvmsglist.IndexOf(lenthFFT1PacketHead[0]);

                if( head_index + LeastPacketLenth > arrRecvmsglist.Count )
                {
                    Console.WriteLine("data lenth is too short, head_index = " + len + "Count = " + arrRecvmsglist.Count);
                    //AppendColorText2RichBox("effective data is too short, count: " + (arrRecvmsglist.Count - head_index) + "\r\n", false);
                    //return false;
                }
                else
                {
                    arrRecvmsglist.CopyTo(head_index, lenthFFT1Data, 0, LeastPacketLenth);

                    if (lenthFFT1Data[0] == lenthFFT1PacketHead[0] && lenthFFT1Data[1] == lenthFFT1PacketHead[1] &&
                         lenthFFT1Data[LeastPacketLenth - 2] == lenthFFTPacketTail[0] &&
                         lenthFFT1Data[LeastPacketLenth - 1] == lenthFFTPacketTail[1])
                    {
                        LenthFFTPacket lenthFFT1DrawData = new LenthFFTPacket();
                        for (int i = 0; i < iPointNum; i++)
                        {
                            lenthFFT1DrawData.InDataArray[i] = (Int32)((lenthFFT1Data[2 + i * 2]) | (Int32)(lenthFFT1Data[2 + i * 2 + 1] << 8));
                        }
                        AllInDataProcess.EnQueueLenthFFT1Pkt(lenthFFT1DrawData);
                        
                        if (no_debug_output == false)
                        {
                            for (int i = 0; i < iPointNum + 2; i++)
                            {
                                AppendColorText2RichBox(lenthFFT1Data[i * 2].ToString("X2") + " " + lenthFFT1Data[i * 2 + 1].ToString("X2") + " ");
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

                lenthFFT2HeadIndex = arrRecvmsglist.IndexOf(lenthFFT2PacketHead[0]);
                if (lenthFFT2HeadIndex + LeastPacketLenth > arrRecvmsglist.Count)
                {
                    Console.WriteLine("data lenth is too short, lenthFFT2HeadIndex = " + len + "Count = " + arrRecvmsglist.Count);
                    //AppendColorText2RichBox("effective data is too short, count: " + (arrRecvmsglist.Count - lenthFFT2HeadIndex) + "\r\n", false);
                    //return false;
                }
                else
                {
                    arrRecvmsglist.CopyTo(lenthFFT2HeadIndex, lenthFFT2Data, 0, LeastPacketLenth);

                    if (lenthFFT2Data[0] == lenthFFT2PacketHead[0] && lenthFFT2Data[1] == lenthFFT2PacketHead[1] &&
                         lenthFFT2Data[LeastPacketLenth - 2] == lenthFFTPacketTail[0] &&
                         lenthFFT2Data[LeastPacketLenth - 1] == lenthFFTPacketTail[1])
                    {
                        LenthFFTPacket lenthFFT2DrawData = new LenthFFTPacket();
                        for (int i = 0; i < iPointNum; i++)
                        {
                            lenthFFT2DrawData.InDataArray[i] = (Int32)((lenthFFT2Data[2 + i * 2]) | (Int32)(lenthFFT2Data[2 + i * 2 + 1] << 8));
                        }
                        AllInDataProcess.EnQueueLenthFFT2Pkt(lenthFFT2DrawData);

                        if (no_debug_output == false)
                        {
                            for (int i = 0; i < iPointNum + 2; i++)
                            {
                                AppendColorText2RichBox(lenthFFT2Data[i * 2].ToString("X2") + " " + lenthFFT2Data[i * 2 + 1].ToString("X2") + " ");
                            }
                            AppendColorText2RichBox("\r\n\r\n");
                        }

                        arrRecvmsglist.RemoveRange(0, lenthFFT2HeadIndex + LeastPacketLenth);
                    }
                    else
                    {
                        if (lenthFFT2HeadIndex == 0)
                            arrRecvmsglist.RemoveAt(0);
                        else
                            arrRecvmsglist.RemoveRange(0, lenthFFT2HeadIndex);
                    }
                }
            }
            return true;
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            Random lenthFFT1DataRandom = new Random(DateTime.Now.Millisecond);

            lenthFFT1DataRandom.NextBytes(lenthFFT1Data);
            lenthFFT1Data[0] = lenthFFT1PacketHead[0];
            lenthFFT1Data[1] = lenthFFT1PacketHead[1];
            lenthFFT1Data[LeastPacketLenth - 2] = lenthFFTPacketTail[0];
            lenthFFT1Data[LeastPacketLenth - 1] = lenthFFTPacketTail[1];

            Random lenthFFT2DataRandom = new Random(DateTime.Now.Millisecond + 100);
            lenthFFT2DataRandom.NextBytes(lenthFFT2Data);
            lenthFFT2Data[0] = lenthFFT2PacketHead[0];
            lenthFFT2Data[1] = lenthFFT2PacketHead[1];
            lenthFFT2Data[LeastPacketLenth - 2] = lenthFFTPacketTail[0];
            lenthFFT2Data[LeastPacketLenth - 1] = lenthFFTPacketTail[1];

            LenthFFTPacket lenthFFT1DrawData = new LenthFFTPacket();
            LenthFFTPacket lenthFFT2DrawData = new LenthFFTPacket();
            for (int i = 0; i < iPointNum; i++)
            {
                lenthFFT1DrawData.InDataArray[i] = (Int32)((lenthFFT1Data[2 + i * 2]) | (Int32)(lenthFFT1Data[2 + i * 2 + 1] << 8));
                lenthFFT2DrawData.InDataArray[i] = (Int32)((lenthFFT2Data[2 + i * 2]) | (Int32)(lenthFFT2Data[2 + i * 2 + 1] << 8));
            }
            AllInDataProcess.EnQueueLenthFFT1Pkt(lenthFFT1DrawData);
            AllInDataProcess.EnQueueLenthFFT2Pkt(lenthFFT2DrawData);

            for (int i = 0; i < iPointNum + 2; i++)
            {
                AppendColorText2RichBox(lenthFFT1Data[i * 2].ToString("X2") + " " + lenthFFT1Data[i * 2 + 1].ToString("X2") + " ");
            }
            AppendColorText2RichBox("\r\n\r\n");

            for (int i = 0; i < iPointNum + 2; i++)
            {
                AppendColorText2RichBox(lenthFFT2Data[i * 2].ToString("X2") + " " + lenthFFT2Data[i * 2 + 1].ToString("X2") + " ");
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

        public void DrawSplineBaseRcvData()
        {
            ChartXy localChartXy = LenthFFTChartControl.ChartPanel.ChartContainers[0] as ChartXy;

            LenthFFTPacket outLenthFFT1Pkt = AllInDataProcess.DeQueueLenthFFT1Pkt();
            if (outLenthFFT1Pkt != null)
            {
                SeriesPoint[] lenthFFT1DrawData = new SeriesPoint[iPointNum - 1];
                SeriesPoint[] lenthFFT1AverageDrawData = new SeriesPoint[iPointNum - 1];
                for (int i = 0; i < iPointNum - 1; i++)
                {
                    double xPointValue = (300000 / iPointNum) * i;
                    lenthFFT1DrawData[i] = new SeriesPoint(xPointValue, outLenthFFT1Pkt.InDataArray[i]);
                    lenthFFT1AverageDrawData[i] = new SeriesPoint(xPointValue, outLenthFFT1Pkt.InDataArray[iPointNum - 1]);
                }
                Console.WriteLine("lenthFFT1 draw, data[0] = " + outLenthFFT1Pkt.InDataArray[iPointNum - 1]);

                ChartSeries lenthFF1ChartSeries = localChartXy.ChartSeries["lenthFFT1"];
                ChartSeries lenthFFT1AverageChartSeries = localChartXy.ChartSeries["lenthFFT1Average"];
                this.Invoke((EventHandler)(delegate
                {
                    lenthFF1ChartSeries.SeriesPoints.Clear();
                    lenthFFT1AverageChartSeries.SeriesPoints.Clear();

                    lenthFF1ChartSeries.SeriesPoints.AddRange(lenthFFT1DrawData);
                    lenthFFT1AverageChartSeries.SeriesPoints.AddRange(lenthFFT1AverageDrawData);
                }));
            }

            LenthFFTPacket outLenthFFT2Pkt = AllInDataProcess.DeQueueLenthFFT2Pkt();
            if (outLenthFFT2Pkt != null)
            {
                SeriesPoint[] lenthFFT2DrawData = new SeriesPoint[iPointNum - 1];
                SeriesPoint[] lenthFFT2AverageDrawData = new SeriesPoint[iPointNum - 1];
                for (int i = 0; i < iPointNum - 1; i++)
                {
                    double xPointValue = (300000 / iPointNum) * i;
                    lenthFFT2DrawData[i] = new SeriesPoint(xPointValue, outLenthFFT2Pkt.InDataArray[i]);
                    lenthFFT2AverageDrawData[i] = new SeriesPoint(xPointValue, outLenthFFT2Pkt.InDataArray[iPointNum - 1]);
                }
                Console.WriteLine("lenthFFT1 draw, data[0] = " + outLenthFFT2Pkt.InDataArray[iPointNum - 1]);

                ChartSeries lenthFF2ChartSeries = localChartXy.ChartSeries["lenthFFT2"];
                ChartSeries lenthFFT2AverageChartSeries = localChartXy.ChartSeries["lenthFFT2Average"];
                this.Invoke((EventHandler)(delegate
                {
                    lenthFF2ChartSeries.SeriesPoints.Clear();
                    lenthFFT2AverageChartSeries.SeriesPoints.Clear();

                    lenthFF2ChartSeries.SeriesPoints.AddRange(lenthFFT2DrawData);
                    lenthFFT2AverageChartSeries.SeriesPoints.AddRange(lenthFFT2AverageDrawData);
                }));
            }

        }

        private void LenthFFTDrawThreadFunction()
        {
            while (true)
            {
                DrawSplineBaseRcvData();
                Thread.Sleep(20);
            }
            
        }

        protected override void OnClosed(EventArgs e)
        {
            LenthFFTDrawThread.Abort();
            Dispose();
            Application.Exit();
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

        private void ExpandableSplitter1_ExpandedChanged(object sender, ExpandedChangeEventArgs e)
        {
            no_debug_output = (no_debug_output == true) ? false : true ;
        }
    }
}
