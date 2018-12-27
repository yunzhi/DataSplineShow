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
using System.Windows.Forms.DataVisualization.Charting;

namespace DataSplineShow
{
    public partial class MainForm : OfficeForm
    {

        #region Private variables

        private Socket dataSocket = null;
        private Thread tcpRcvThread = null;
        private bool   isStarted = false;

        //定义一个1M的内存缓冲区，用于临时性存储接收到的消息  
        byte[] arrRecvmsg = new byte[1024 * 1024];
        ArrayList arrRecvmsglist = new ArrayList(0);

        private byte[] lenthFFT1PacketHead = new byte[2] { 0xFA, 0xFA };
        private byte[] lenthFFT2PacketHead = new byte[2] { 0xFB, 0xFB };
        private byte[] lenthSpeedPacketHead = new byte[2] { 0xFC, 0xFC };
        private byte[] lenthPosPacketHead = new byte[2] { 0xFD, 0xFD };
        private byte[] targetPosPacketHead = new byte[2] { 0xFE, 0xFE };

        private byte[] lenthFFTPacketTail  = new byte[2] { 0xEB, 0xEB };
        private byte[] lenthSpeedPacketTail = new byte[2] { 0xED, 0xED };
        private byte[] lenthPosPacketTail = new byte[2] { 0xEA, 0xEA };
        private byte[] targetPosPacketTail = new byte[2] { 0xEC, 0xEC };

        private const int LeastPacketLenth = 4102; // 256*2 + 1*2 + 4
        private const int iPointNum        = 2049; //256 + 1

        private const int LeastLenthSpeedPosPktLenth = 52; // 12*4 + 4
        private const int LeastTargetPosPktLenth = 16; // 12+4

        private byte[] lenthFFT1Data = new byte[LeastPacketLenth];
        private byte[] lenthFFT2Data = new byte[LeastPacketLenth];

        private byte[] lenthPosData = new byte[LeastLenthSpeedPosPktLenth];
        private byte[] lenthSpeedData = new byte[LeastLenthSpeedPosPktLenth];

        private byte[] targetPosData = new byte[LeastTargetPosPktLenth];

        private bool no_debug_output = false;

        private Thread LenthFFTDrawThread;

        private InDataProcess AllInDataProcess = null;

        private object showDataFlag = new object();

        private int justProcessSomeData = 0; // 2-- distanceAzimuth 数据

        private object noClearOldDataObjact = new object();
        private bool notClearHistoryDataFlag = false;
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

            ResumeLayout(false);
            
            AllInDataProcess = new InDataProcess();

            LenthFFTDrawThread = new Thread(LenthFFTDrawThreadFunction);
            LenthFFTDrawThread.Start();
        }

        private void ComboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.SelectedItem == null) return;
            eStyle style = (eStyle)comboBoxEx1.SelectedItem;
            if (styleManager1.ManagerStyle != style)
                styleManager1.ManagerStyle = style;
        }

        private void SocketRecvProcess()
        {
            //持续监听服务端发来的消息 
            while (true)
            {
                //Thread.Sleep(10);
                try
                {
                    if (dataSocket != null && dataSocket.Connected)
                    {
                        //将客户端套接字接收到的数据存入内存缓冲区，并获取长度  
                        int length = dataSocket.Receive(arrRecvmsg);
                        if (length != 0)
                        {
                            CheckAnalysisRcvPacket(arrRecvmsg, length);
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

            for (int i = 0; i < len; i++)
                arrRecvmsglist.Add(rcvPacket[i]);

            //Console.WriteLine("arrRecvmsglist.Count = " + arrRecvmsglist.Count + ", len = " + len);

            StringBuilder displayRcvBytesString = new StringBuilder();
            //if( arrRecvmsglist.Count < LeastPacketLenth)
            //{
            //    Console.WriteLine("len {0} < LeastPacketLenth {1}", len, LeastPacketLenth);
            //    return false;
            //}

            int index;

            lock (showDataFlag)
            {
                
                for (index = 0; index < arrRecvmsglist.Count; index++)
                {

                    int lenthPosHeadIndex = arrRecvmsglist.IndexOf(lenthPosPacketHead[0]);
                    if (lenthPosHeadIndex == -1 || lenthPosHeadIndex + LeastLenthSpeedPosPktLenth > arrRecvmsglist.Count)
                    {
                        Console.WriteLine("too short, lenthPosHeadIndex = " + lenthPosHeadIndex);
                        return false;
                    }
                    else
                    {
                        arrRecvmsglist.CopyTo(lenthPosHeadIndex, lenthPosData, 0, LeastLenthSpeedPosPktLenth);

                        if (lenthPosData[0] == lenthPosPacketHead[0] && lenthPosData[1] == lenthPosPacketHead[1] &&
                             lenthPosData[LeastLenthSpeedPosPktLenth - 2] == lenthPosPacketTail[0] &&
                             lenthPosData[LeastLenthSpeedPosPktLenth - 1] == lenthPosPacketTail[1])
                        {
                            DistancePosSpeedPacket DistancePosDrawData = new DistancePosSpeedPacket();
                            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i++)
                            {
                                DistancePosDrawData.InDataArray[i] = (Int16)((lenthPosData[2 + i * 2]) | (lenthPosData[2 + i * 2 + 1] << 8));
                            }
                            AllInDataProcess.EnQueueLenthPosPkt(DistancePosDrawData);

                            if (no_debug_output == false)
                            {
                                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
                                {
                                    displayRcvBytesString.Append(lenthPosData[i * 2].ToString("X2") + " " + lenthPosData[i * 2 + 1].ToString("X2") + " ");
                                }
                                displayRcvBytesString.Append("\r\n\r\n");
                            }

                            //Console.WriteLine(DateTime.Now.Millisecond.ToString() + " arrRecvmsglist.Count before : " + arrRecvmsglist.Count.ToString());
                            arrRecvmsglist.RemoveRange(0, lenthPosHeadIndex + LeastLenthSpeedPosPktLenth);
                            //Console.WriteLine(DateTime.Now.Millisecond.ToString() + "arrRecvmsglist.Count after : " + arrRecvmsglist.Count.ToString());
                        }
                        else
                        {
                            //Console.WriteLine(DateTime.Now.Millisecond.ToString() + "arrRecvmsglist.Count : " + arrRecvmsglist.Count.ToString());
                            if (lenthPosHeadIndex == 0)
                                arrRecvmsglist.RemoveAt(0);
                            else
                                arrRecvmsglist.RemoveRange(0, lenthPosHeadIndex);
                        }
                    }
                }
            }

            if (no_debug_output == false)
            {
                if (displayRcvBytesString != null)
                    AppendColorText2RichBox(displayRcvBytesString.ToString());
            }

            return true;
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            StringBuilder testBytesBuilder = new StringBuilder();

            Random lenthPosDataRandom = new Random(DateTime.Now.Millisecond + 200);
            lenthPosDataRandom.NextBytes(lenthPosData);

            lenthPosData[0] = lenthPosPacketHead[0];
            lenthPosData[1] = lenthPosPacketHead[1];

            /*
            for (int i = 2; i < LeastLenthSpeedPosPktLenth - 2; i += 4)
            {
                lenthPosData[i] = (byte)lenthPosDataRandom.Next(255);
                lenthPosData[i + 1] = (byte)lenthPosDataRandom.Next(255);
                lenthPosData[i + 2] = (byte)lenthPosDataRandom.Next(255);
                lenthPosData[i + 3] = (byte)lenthPosDataRandom.Next(255);
            }
            */

            lenthPosData[2] = 0x0a;
            lenthPosData[3] = 0x02;
            lenthPosData[4] = 0x18;
            lenthPosData[5] = 0xfc;

            lenthPosData[LeastLenthSpeedPosPktLenth - 2] = lenthPosPacketTail[0];
            lenthPosData[LeastLenthSpeedPosPktLenth - 1] = lenthPosPacketTail[1];

            CheckAnalysisRcvPacket(lenthPosData, LeastLenthSpeedPosPktLenth);

            /*

            DistancePosSpeedPacket lenthPosDrawData = new DistancePosSpeedPacket();
            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i++)
            {
                lenthPosDrawData.InDataArray[i] = (Int16)( (lenthPosData[2 + i * 2] ) | (lenthPosData[2 + i * 2 + 1] << 8) );
            }

            AllInDataProcess.EnQueueLenthPosPkt(lenthPosDrawData);

            testBytesBuilder.Append("\r\n\r\n");
            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
            {
                testBytesBuilder.Append(lenthPosData[i * 2].ToString("X2") + " " + lenthPosData[i * 2 + 1].ToString("X2") + " ");
            }
            testBytesBuilder.Append("\r\n\r\n");


            if ( testBytesBuilder != null)
            {
                rcvRichTextBox.AppendText(testBytesBuilder.ToString());
            }

            */
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
            DistancePosSpeedPacket outLenthPosPkt = null;
            if (DistancePosContainer.distancePosContainer.Count > 0)
            {
                //Console.WriteLine("distanceAzimuth count: " + DistancePosContainer.distancePosContainer.Count);
                outLenthPosPkt = AllInDataProcess.DeQueueLenthPosPkt();
            }

            if (outLenthPosPkt != null)
            {
                Series distanceAzimuthSeries = this.chart1.Series["DistanceAzimuth"];

                if (distanceAzimuthSeries != null)
                {
                    if (distanceAzimuthSeries.Points.Count != 0)
                    {
                        this.Invoke((EventHandler)(delegate
                        {
                            if (notClearHistoryDataFlag == false)
                            {
                                distanceAzimuthSeries.Points.Clear();
                                distanceAzimuthSeries.Points.AddXY(0, 0);
                            }

                            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i += 2)
                            {
                                double lenthPosPointY = Convert.ToDouble(outLenthPosPkt.InDataArray[i]) / 100.0; // distance
                                double lenthPosPointX = Convert.ToDouble(outLenthPosPkt.InDataArray[i + 1]) / 100.0; //azimuth

                                //rcvRichTextBox.AppendText("(" + lenthPosPointX + "," + lenthPosPointY + ")");
                                //rcvRichTextBox.Select(rcvRichTextBox.TextLength, 0);
                                //rcvRichTextBox.ScrollToCaret();

                                if (lenthPosPointY == 0 && lenthPosPointX == 0)
                                {
                                    continue;
                                }
                                distanceAzimuthSeries.Points.AddXY(lenthPosPointX, lenthPosPointY);

                                //allPoint.Add(new DrawPoint(lenthPosPointX, lenthPosPointY));
                            }
                        }));
                    }
                }
            }
        }

        private void LenthFFTDrawThreadFunction()
        {
            while (true)
            {
                Thread.Sleep(10);
                DrawSplineBaseRcvData();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            LenthFFTDrawThread.Abort();
            LenthFFTDrawThread.Join();
            Dispose();
            Application.Exit();
            System.Environment.Exit(0);
        }

        private void ExpandableSplitter1_ExpandedChanged(object sender, ExpandedChangeEventArgs e)
        {
            no_debug_output = (no_debug_output == true) ? false : true ;
        }

        #region SetupDataLabelStyle

        /// <summary>
        /// Creates a DataLabelStyle from the given connector angle and color.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private DataLabelVisualStyle SetupDataLabelStyle(int angle, Color color)
        {
            DataLabelVisualStyle dtStyle = new DataLabelVisualStyle();

            dtStyle.Background = new Background(Color.FromArgb(170, color));
            dtStyle.Padding = new DevComponents.DotNetBar.Charts.Style.Padding(3);
            dtStyle.Font = new System.Drawing.Font("Arial", 8, FontStyle.Italic);
            dtStyle.TextAlignment = LineAlignment.Center;
            dtStyle.DropShadow.ShadowColor = Color.Crimson;

            dtStyle.DrawConnector = Tbool.True;
            dtStyle.ConnectorLineStyle.DefaultAngle = angle;

            dtStyle.ApplyDefaults();

            return (dtStyle);
        }

        #endregion

        private void AddSeriesPointLabel(ChartSeries drawSeries, SeriesPoint[] pointLabel, Color color, DistancePosSpeedPacket lenthPosValue, DistancePosSpeedPacket lenthSpeedValue)
        {
            return;
            drawSeries.DataLabels.Clear();

            int PosSpeedPointIndex = 0;
            for ( int i = 0; i < pointLabel.Length; i++)
            {
                PosSpeedPointIndex = i * 2;

                DataLabel dl = new DataLabel(pointLabel[i]);

                double distance = lenthSpeedValue.InDataArray[PosSpeedPointIndex];
                double speed = Convert.ToDouble(lenthSpeedValue.InDataArray[PosSpeedPointIndex + 1])/100;
                double pos = Convert.ToDouble(lenthPosValue.InDataArray[PosSpeedPointIndex + 1])/100;

                dl.Text = "X:" + pointLabel[i].ValueX.ToString() + "\nY:" + pointLabel[i].ValueY[0].ToString()
                    + "\nDistance: " + distance.ToString() + "\nSpeed: " + speed.ToString() + "\nAzimuth:" + pos.ToString();
                dl.DataLabelVisualStyle = SetupDataLabelStyle(90, color); ;

                drawSeries.DataLabels.Add(dl);
            }
        }
        
        private void LenthFFTNav_Click(object sender, EventArgs e)
        {
            lock (showDataFlag)
            {
                justProcessSomeData = 1;
            }
        }

        private void LenthPosNav_Click(object sender, EventArgs e)
        {
            lock (showDataFlag)
            {
                justProcessSomeData = 2;
            }
        }

        private void lenthSpeedNav_Click(object sender, EventArgs e)
        {
            lock (showDataFlag)
            {
                justProcessSomeData = 3;
            }
        }

        private void ShowAllPointsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            lock (noClearOldDataObjact)
            {
                notClearHistoryDataFlag = ShowAllPointsCheckBox.Checked;
            }
        }

        private void ShowLabelCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Series distanceAzimuthSeries = this.chart1.Series["DistanceAzimuth"];

            distanceAzimuthSeries.IsValueShownAsLabel = ShowLabelCheckBox.Checked;
            distanceAzimuthSeries.SmartLabelStyle.Enabled = ShowLabelCheckBox.Checked;

            if(distanceAzimuthSeries.IsValueShownAsLabel == true)
            {
                distanceAzimuthSeries.Label = "#VALX{N2},#VAL{N2}";
            }
            else
            {
                distanceAzimuthSeries.Label = "";
            }
        }

        private void TargetSize_ValueChanged(object sender, EventArgs e)
        {
            Series distanceAzimuthSeries = this.chart1.Series["DistanceAzimuth"];

            distanceAzimuthSeries.MarkerSize = TargetSize.Value;
        }
    }
}
