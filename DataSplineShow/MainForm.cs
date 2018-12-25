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

            LenthFFTPosSpeedNavInit();
            
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

        #region LenthFFTPosSpeedNavInit
        private void LenthFFTPosSpeedNavInit()
        {
            LenthFFTChartControlDisplay LenthFFTChartControlInstance = new LenthFFTChartControlDisplay();
            //LenthPosChartControlDisplay LenthPosChartControlInstance = new LenthPosChartControlDisplay();
            LenthSpeedChartControlDisplay LenthSpeedChartControlInstance = new LenthSpeedChartControlDisplay();

            this.lenthFFTNavPanel.Controls.Add(LenthFFTChartControlDisplay.LenthFFTChartControl);
            //this.lenthPosNavPanel.Controls.Add(LenthPosChartControlDisplay.LenthPosChartControl);
            this.lenthSpeedNavPanel.Controls.Add(LenthSpeedChartControlDisplay.LenthSpeedChartControl);

            //DistanceAzimuth distanceAzimuthDraw = new DistanceAzimuth(this.chart1);
        }
        #endregion

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

            int index, head_index, lenthFFT2HeadIndex, targetPosHeadIndex, lenthSpeedHeadIndex;

            lock (showDataFlag)
            {
                
                for (index = 0; index < arrRecvmsglist.Count; index++)
                {
                    if(justProcessSomeData == 1)
                    {


                        //Console.WriteLine("justProcessSomeData : " + justProcessSomeData.ToString());
                        head_index = arrRecvmsglist.IndexOf(lenthFFT1PacketHead[0]);
                        if (head_index == -1 || head_index + LeastPacketLenth > arrRecvmsglist.Count)
                        {
                            Console.WriteLine("data lenth is too short, head_index = " + len + ", Count = " + arrRecvmsglist.Count);
                            //AppendColorText2RichBox("effective data is too short, count: " + (arrRecvmsglist.Count - head_index) + "\r\n", false);
                            return false;
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
                                    lenthFFT1DrawData.InDataArray[i] = (UInt16)((lenthFFT1Data[2 + i * 2]) | (UInt16)(lenthFFT1Data[2 + i * 2 + 1] << 8));
                                }
                                AllInDataProcess.EnQueueLenthFFT1Pkt(lenthFFT1DrawData);

                                if (no_debug_output == false)
                                {
                                    for (int i = 0; i < iPointNum + 2; i++)
                                    {
                                        displayRcvBytesString.Append(lenthFFT1Data[i * 2].ToString("X2") + " " + lenthFFT1Data[i * 2 + 1].ToString("X2") + " ");
                                    }
                                    displayRcvBytesString.Append("\r\n\r\n");
                                }

                                if (arrRecvmsglist.Count < (LeastPacketLenth * 2 + LeastTargetPosPktLenth))
                                {
                                    Console.WriteLine("rcv bytes not enough");
                                    return false;
                                }

                                arrRecvmsglist.RemoveRange(0, head_index + LeastPacketLenth);
                                Console.WriteLine("fft1 remove: " + (head_index + LeastPacketLenth).ToString());
                            }
                            else
                            {
                                Console.WriteLine("fft1 remove head_index: " + (head_index).ToString());
                                if (head_index == 0)
                                    arrRecvmsglist.RemoveAt(0);
                                else
                                    arrRecvmsglist.RemoveRange(0, head_index);

                                return false;
                            }
                        }


                        lenthFFT2HeadIndex = arrRecvmsglist.IndexOf(lenthFFT2PacketHead[0]);
                        if (lenthFFT2HeadIndex == -1 || lenthFFT2HeadIndex + LeastPacketLenth > arrRecvmsglist.Count)
                        {
                            Console.WriteLine("data lenth is too short, lenthFFT2HeadIndex = " + len + ", Count = " + arrRecvmsglist.Count);
                            //AppendColorText2RichBox("effective data is too short, count: " + (arrRecvmsglist.Count - lenthFFT2HeadIndex) + "\r\n", false);
                            return false;
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
                                    lenthFFT2DrawData.InDataArray[i] = (UInt16)((lenthFFT2Data[2 + i * 2]) | (UInt16)(lenthFFT2Data[2 + i * 2 + 1] << 8));
                                }
                                AllInDataProcess.EnQueueLenthFFT2Pkt(lenthFFT2DrawData);

                                if (no_debug_output == false)
                                {
                                    for (int i = 0; i < iPointNum + 2; i++)
                                    {
                                        displayRcvBytesString.Append(lenthFFT2Data[i * 2].ToString("X2") + " " + lenthFFT2Data[i * 2 + 1].ToString("X2") + " ");
                                    }
                                    displayRcvBytesString.Append("\r\n\r\n");
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


                        targetPosHeadIndex = arrRecvmsglist.IndexOf(targetPosPacketHead[0]);
                        if (targetPosHeadIndex == -1 || targetPosHeadIndex + LeastTargetPosPktLenth > arrRecvmsglist.Count)
                        {
                            Console.WriteLine("too short, targetPosHeadIndex = " + targetPosHeadIndex);
                        }
                        else
                        {
                            arrRecvmsglist.CopyTo(targetPosHeadIndex, targetPosData, 0, LeastTargetPosPktLenth);

                            if (targetPosData[0] == targetPosPacketHead[0] && targetPosData[1] == targetPosPacketHead[1] &&
                                 targetPosData[LeastTargetPosPktLenth - 2] == targetPosPacketTail[0] &&
                                 targetPosData[LeastTargetPosPktLenth - 1] == targetPosPacketTail[1])
                            {
                                TargetPosPacket targetPosDrawData = new TargetPosPacket();
                                for (int i = 0; i < TargetPosPacket.iPointCnt; i++)
                                {
                                    targetPosDrawData.InDataArray[i] = targetPosData[2 + i];
                                }
                                AllInDataProcess.EnQueueTargetPosPkt(targetPosDrawData);

                                if (no_debug_output == false)
                                {
                                    for (int i = 0; i < iPointNum + 4; i++)
                                    {
                                        displayRcvBytesString.Append(targetPosData[i].ToString("X2") + " ");
                                    }
                                    displayRcvBytesString.Append("\r\n\r\n");
                                }

                                arrRecvmsglist.RemoveRange(0, targetPosHeadIndex + LeastTargetPosPktLenth);
                            }
                            else
                            {
                                if (targetPosHeadIndex == 0)
                                    arrRecvmsglist.RemoveAt(0);
                                else
                                    arrRecvmsglist.RemoveRange(0, targetPosHeadIndex);
                            }
                        }

                    }
                    else if( justProcessSomeData == 3)
                    {
                        //Console.WriteLine("justProcessSomeData : " + justProcessSomeData.ToString());
                        lenthSpeedHeadIndex = arrRecvmsglist.IndexOf(lenthSpeedPacketHead[0]);
                        if (lenthSpeedHeadIndex == -1 || lenthSpeedHeadIndex + LeastLenthSpeedPosPktLenth > arrRecvmsglist.Count)
                        {
                            Console.WriteLine("too short, lenthSpeedHeadIndex = " + lenthSpeedHeadIndex);
                            return false;
                        }
                        else
                        {
                            arrRecvmsglist.CopyTo(lenthSpeedHeadIndex, lenthSpeedData, 0, LeastLenthSpeedPosPktLenth);

                            if (lenthSpeedData[0] == lenthSpeedPacketHead[0] && lenthSpeedData[1] == lenthSpeedPacketHead[1] &&
                                 lenthSpeedData[LeastLenthSpeedPosPktLenth - 2] == lenthSpeedPacketTail[0] &&
                                 lenthSpeedData[LeastLenthSpeedPosPktLenth - 1] == lenthSpeedPacketTail[1])
                            {
                                DistancePosSpeedPacket DistanceSpeedDrawData = new DistancePosSpeedPacket();
                                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i++)
                                {
                                    DistanceSpeedDrawData.InDataArray[i] = (byte)((lenthSpeedData[2 + i * 2]) | (byte)(lenthSpeedData[2 + i * 2 + 1] << 8));

                                }
                                AllInDataProcess.EnQueueLenthSpeedPkt(DistanceSpeedDrawData);

                                if (no_debug_output == false)
                                {
                                    for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
                                    {
                                        displayRcvBytesString.Append(lenthSpeedData[i * 2].ToString("X2") + " " + lenthSpeedData[i * 2 + 1].ToString("X2") + " ");
                                    }
                                    displayRcvBytesString.Append("\r\n\r\n");
                                }

                                arrRecvmsglist.RemoveRange(0, lenthSpeedHeadIndex + LeastLenthSpeedPosPktLenth);
                            }
                            else
                            {
                                if (lenthSpeedHeadIndex == 0)
                                    arrRecvmsglist.RemoveAt(0);
                                else
                                    arrRecvmsglist.RemoveRange(0, lenthSpeedHeadIndex);
                            }
                        }
                    }

                    if(justProcessSomeData == 2 || justProcessSomeData == 0)
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
            if (justProcessSomeData == 1)
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

                Random targetPosDataRandom = new Random(DateTime.Now.Millisecond + 100);
                targetPosDataRandom.NextBytes(targetPosData);
                targetPosData[0] = targetPosPacketHead[0];
                targetPosData[1] = targetPosPacketHead[1];
                targetPosData[LeastTargetPosPktLenth - 2] = targetPosPacketTail[0];
                targetPosData[LeastTargetPosPktLenth - 1] = targetPosPacketTail[1];

                LenthFFTPacket lenthFFT1DrawData = new LenthFFTPacket();
                LenthFFTPacket lenthFFT2DrawData = new LenthFFTPacket();
                for (int i = 0; i < iPointNum; i++)
                {
                    lenthFFT1DrawData.InDataArray[i] = (UInt16)((lenthFFT1Data[2 + i * 2]) | (UInt16)(lenthFFT1Data[2 + i * 2 + 1] << 8));
                    lenthFFT2DrawData.InDataArray[i] = (UInt16)((lenthFFT2Data[2 + i * 2]) | (UInt16)(lenthFFT2Data[2 + i * 2 + 1] << 8));
                }

                TargetPosPacket targetPosDrawData = new TargetPosPacket();
                for (int i = 0; i < TargetPosPacket.iPointCnt; i++)
                {
                    targetPosDrawData.InDataArray[i] = targetPosData[2 + i];
                }

                AllInDataProcess.EnQueueLenthFFT1Pkt(lenthFFT1DrawData);
                AllInDataProcess.EnQueueLenthFFT2Pkt(lenthFFT2DrawData);
                AllInDataProcess.EnQueueTargetPosPkt(targetPosDrawData);

                for (int i = 0; i < iPointNum + 2; i++)
                {
                    testBytesBuilder.Append(lenthFFT1Data[i * 2].ToString("X2") + " " + lenthFFT1Data[i * 2 + 1].ToString("X2") + " ");
                }
                testBytesBuilder.Append("\r\n\r\n");

                for (int i = 0; i < iPointNum + 2; i++)
                {
                    testBytesBuilder.Append(lenthFFT2Data[i * 2].ToString("X2") + " " + lenthFFT2Data[i * 2 + 1].ToString("X2") + " ");
                }
                testBytesBuilder.Append("\r\n\r\n");

                for (int i = 0; i < TargetPosPacket.iPointCnt + 4; i++)
                {
                    testBytesBuilder.Append(targetPosData[i].ToString("X2") + " ");
                }
                testBytesBuilder.Append("\r\n\r\n");
            }
            else if (justProcessSomeData == 3)
            {

                Random lenthSpeedDataRandom = new Random(DateTime.Now.Millisecond + 100);
                lenthSpeedDataRandom.NextBytes(lenthSpeedData);
                lenthSpeedData[0] = lenthSpeedPacketHead[0];
                lenthSpeedData[1] = lenthSpeedPacketHead[1];
                lenthSpeedData[LeastLenthSpeedPosPktLenth - 2] = lenthSpeedPacketTail[0];
                lenthSpeedData[LeastLenthSpeedPosPktLenth - 1] = lenthSpeedPacketTail[1];

                DistancePosSpeedPacket lenthSpeedDrawData = new DistancePosSpeedPacket();
                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i++)
                {
                    lenthSpeedDrawData.InDataArray[i] = (Int16)((lenthSpeedData[2 + i * 2]) | (UInt16)(lenthSpeedData[2 + i * 2 + 1] << 8));
                }
                AllInDataProcess.EnQueueLenthSpeedPkt(lenthSpeedDrawData);

                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
                {
                    testBytesBuilder.Append(lenthSpeedData[i * 2].ToString("X2") + " " + lenthSpeedData[i * 2 + 1].ToString("X2") + " ");
                }
                testBytesBuilder.Append("\r\n\r\n");

            }
            else if (justProcessSomeData == 2 || justProcessSomeData == 0)
            {

                Random lenthPosDataRandom = new Random(DateTime.Now.Millisecond + 200);
                lenthPosDataRandom.NextBytes(lenthPosData);

                lenthPosData[0] = lenthPosPacketHead[0];
                lenthPosData[1] = lenthPosPacketHead[1];

                for (int i = 2; i < LeastLenthSpeedPosPktLenth - 2; i += 4)
                {
                    lenthPosData[i] = (byte)lenthPosDataRandom.Next(255);
                    lenthPosData[i + 1] = (byte)lenthPosDataRandom.Next(255);
                    lenthPosData[i + 2] = (byte)lenthPosDataRandom.Next(255);
                    lenthPosData[i + 3] = (byte)lenthPosDataRandom.Next(255);
                }

                lenthPosData[LeastLenthSpeedPosPktLenth - 2] = lenthPosPacketTail[0];
                lenthPosData[LeastLenthSpeedPosPktLenth - 1] = lenthPosPacketTail[1];

                DistancePosSpeedPacket lenthPosDrawData = new DistancePosSpeedPacket();
                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i++)
                {
                    lenthPosDrawData.InDataArray[i] = (Int16)((lenthPosData[2 + i * 2]) | (UInt16)(lenthPosData[2 + i * 2 + 1] << 8));
                }

                AllInDataProcess.EnQueueLenthPosPkt(lenthPosDrawData);

                testBytesBuilder.Append("\r\n\r\n");
                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
                {
                    testBytesBuilder.Append(lenthPosData[i * 2].ToString("X2") + " " + lenthPosData[i * 2 + 1].ToString("X2") + " ");
                }
                testBytesBuilder.Append("\r\n\r\n");
            }
            
            if( testBytesBuilder != null)
            {
                rcvRichTextBox.AppendText(testBytesBuilder.ToString());
            }
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
#if TTTTT
            ChartXy LenthFFTdrawChartXy = LenthFFTChartControlDisplay.LenthFFTChartControl.ChartPanel.ChartContainers[0] as ChartXy;
            //ChartXy lenthPosdrawChartXy = LenthPosChartControlDisplay.LenthPosChartControl.ChartPanel.ChartContainers[0] as ChartXy;
            ChartXy lenthSpeeddrawChartXy = LenthSpeedChartControlDisplay.LenthSpeedChartControl.ChartPanel.ChartContainers[0] as ChartXy;

            TargetPosPacket outTargetPosPoint = null;
            LenthFFTPacket outLenthFFT1Pkt = null;
            LenthFFTPacket outLenthFFT2Pkt = null;
            DistancePosSpeedPacket outLenthSpeedPkt = null;

            //if (TargetPosContainer.targetPosContainer.Count == LenthFFT1Container.lenthFFT1DataContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count == LenthFFT2Container.lenthFFT2DataContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count == DistanceSpeedContainer.distanceSpeedContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count == DistancePosContainer.distancePosContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count != 0)

            //if (TargetPosContainer.targetPosContainer.Count > 0 &&
            //    LenthFFT1Container.lenthFFT1DataContainer.Count > 0 &&
            //    LenthFFT2Container.lenthFFT2DataContainer.Count > 0 &&
            //    DistanceSpeedContainer.distanceSpeedContainer.Count > 0 &&
            //    DistancePosContainer.distancePosContainer.Count > 0)
            //{
            //    outTargetPosPoint = AllInDataProcess.DeQueueTargetPosPkt();
            //    outLenthFFT1Pkt = AllInDataProcess.DeQueueLenthFFT1Pkt();
            //    outLenthFFT2Pkt = AllInDataProcess.DeQueueLenthFFT2Pkt();
            //    outLenthSpeedPkt = AllInDataProcess.DeQueueLenthSpeedPkt();
            //    outLenthPosPkt = AllInDataProcess.DeQueueLenthPosPkt();
            //}

            if (TargetPosContainer.targetPosContainer.Count > 0 &&
                LenthFFT1Container.lenthFFT1DataContainer.Count > 0 &&
                LenthFFT2Container.lenthFFT2DataContainer.Count > 0)
            {
                outTargetPosPoint = AllInDataProcess.DeQueueTargetPosPkt();
                outLenthFFT1Pkt = AllInDataProcess.DeQueueLenthFFT1Pkt();
                outLenthFFT2Pkt = AllInDataProcess.DeQueueLenthFFT2Pkt();
            }

            if (DistanceSpeedContainer.distanceSpeedContainer.Count > 0)
            {
                outLenthSpeedPkt = AllInDataProcess.DeQueueLenthSpeedPkt();
            }

            if (DistancePosContainer.distancePosContainer.Count > 0)
            {

                //Console.WriteLine("distanceAzimuth count: " + DistancePosContainer.distancePosContainer.Count);
                outLenthPosPkt = AllInDataProcess.DeQueueLenthPosPkt();
            }


            if (outLenthFFT1Pkt != null)
            {
                SeriesPoint[] lenthFFT1DrawData = new SeriesPoint[iPointNum - 1];
                SeriesPoint[] lenthFFT1AverageDrawData = new SeriesPoint[iPointNum - 1];
                SeriesPoint[] lenthFFT1LabelSeriesPoint = new SeriesPoint[TargetPosPacket.iPointCnt];
                for (int i = 0; i < iPointNum - 1; i++)
                {
                    double xPointValue = (300000 / iPointNum) * i;
                    lenthFFT1DrawData[i] = new SeriesPoint(xPointValue, Convert.ToDouble(outLenthFFT1Pkt.InDataArray[i]));
                    lenthFFT1AverageDrawData[i] = new SeriesPoint(xPointValue, Convert.ToDouble(outLenthFFT1Pkt.InDataArray[iPointNum - 1]));
                }

                for (int i = 0; i < TargetPosPacket.iPointCnt; i++)
                {
                    lenthFFT1LabelSeriesPoint[i] = new SeriesPoint((300000 / iPointNum) * outTargetPosPoint.InDataArray[i],
                        Convert.ToDouble(outLenthFFT1Pkt.InDataArray[outTargetPosPoint.InDataArray[i]]));
                }

                Console.WriteLine("lenthFFT1 draw, data[0] = " + outLenthFFT1Pkt.InDataArray[iPointNum - 1]);

                ChartSeries lenthFF1ChartSeries = LenthFFTdrawChartXy.ChartSeries["lenthFFT1"];
                ChartSeries lenthFFT1AverageChartSeries = LenthFFTdrawChartXy.ChartSeries["lenthFFT1Average"];
                this.Invoke((EventHandler)(delegate
                {
                    lenthFF1ChartSeries.SeriesPoints.Clear();
                    lenthFFT1AverageChartSeries.SeriesPoints.Clear();

                    lenthFF1ChartSeries.SeriesPoints.AddRange(lenthFFT1DrawData);
                    lenthFFT1AverageChartSeries.SeriesPoints.AddRange(lenthFFT1AverageDrawData);

                    AddSeriesPointLabel(lenthFF1ChartSeries, lenthFFT1LabelSeriesPoint, Color.Green, outLenthPosPkt, outLenthSpeedPkt);
                }));
            }

            if (outLenthFFT2Pkt != null)
            {
                SeriesPoint[] lenthFFT2DrawData = new SeriesPoint[iPointNum - 1];
                SeriesPoint[] lenthFFT2AverageDrawData = new SeriesPoint[iPointNum - 1];
                SeriesPoint[] lenthFFT2LabelSeriesPoint = new SeriesPoint[TargetPosPacket.iPointCnt];
                for (int i = 0; i < iPointNum - 1; i++)
                {
                    double xPointValue = (300000 / iPointNum) * i;
                    lenthFFT2DrawData[i] = new SeriesPoint(xPointValue, Convert.ToDouble(outLenthFFT2Pkt.InDataArray[i]));
                    lenthFFT2AverageDrawData[i] = new SeriesPoint(xPointValue, Convert.ToDouble(outLenthFFT2Pkt.InDataArray[iPointNum - 1]));
                }
                Console.WriteLine("lenthFFT2 draw, data[0] = " + outLenthFFT2Pkt.InDataArray[iPointNum - 1]);

                for (int i = 0; i < TargetPosPacket.iPointCnt; i++)
                {
                    lenthFFT2LabelSeriesPoint[i] = new SeriesPoint((300000 / iPointNum) * outTargetPosPoint.InDataArray[i],
                        Convert.ToDouble((outLenthFFT2Pkt.InDataArray[outTargetPosPoint.InDataArray[i]])));
                }

                ChartSeries lenthFF2ChartSeries = LenthFFTdrawChartXy.ChartSeries["lenthFFT2"];
                ChartSeries lenthFFT2AverageChartSeries = LenthFFTdrawChartXy.ChartSeries["lenthFFT2Average"];
                this.Invoke((EventHandler)(delegate
                {
                    lenthFF2ChartSeries.SeriesPoints.Clear();
                    lenthFFT2AverageChartSeries.SeriesPoints.Clear();

                    lenthFF2ChartSeries.SeriesPoints.AddRange(lenthFFT2DrawData);
                    lenthFFT2AverageChartSeries.SeriesPoints.AddRange(lenthFFT2AverageDrawData);
                    AddSeriesPointLabel(lenthFF2ChartSeries, lenthFFT2LabelSeriesPoint, Color.Orange, outLenthPosPkt, outLenthSpeedPkt);
                }));
            }

            bool lenthSpeedPosDataNotSorted = false;
            StringBuilder displayLenthSpeedstring = null;

            if (outLenthSpeedPkt != null)
            {
                List<SeriesPoint> lenthSpeedDrawPointList = new List<SeriesPoint>();
                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i += 2)
                {
                    lenthSpeedDrawPointList.Add(new SeriesPoint(Convert.ToDouble( outLenthSpeedPkt.InDataArray[i]) / 100, Convert.ToDouble(outLenthSpeedPkt.InDataArray[i + 1]) / 100));
                }
                Console.WriteLine("outLenthSpeedPkt is not null");

                displayLenthSpeedstring = new StringBuilder();
                displayLenthSpeedstring.Append("speed data start:");
                foreach (SeriesPoint point in lenthSpeedDrawPointList)
                {
                    displayLenthSpeedstring.Append("(" + point.ValueX + "," + point.ValueY[0] + ")");
                }
                displayLenthSpeedstring.Append("speed data end\r\n");

                IEnumerable<SeriesPoint> sortedSpeedSeriesPointList =
                        from singlePoint in lenthSpeedDrawPointList
                        orderby singlePoint.ValueX
                        select singlePoint;


                for (int i = 0; i < lenthSpeedDrawPointList.Count; i++)
                {
                    if (lenthSpeedDrawPointList.ElementAt(i).ValueX != sortedSpeedSeriesPointList.ElementAt(i).ValueX)
                    {
                        lenthSpeedPosDataNotSorted = true;
                        break;
                    }
                }

                ChartSeries lenthSpeedChartSeries = lenthSpeeddrawChartXy.ChartSeries["DistanceSpeed"];
                lenthSpeedChartSeries.PointLabelDisplayMode |= PointLabelDisplayMode.AllSeriesPoints;
                this.Invoke((EventHandler)(delegate
                {
                    lenthSpeedChartSeries.SeriesPoints.Clear();
                    lenthSpeedChartSeries.SeriesPoints.AddRange(sortedSpeedSeriesPointList);
                }));
            }

#endif
            if (justProcessSomeData == 1)
            {

            }
            else if (justProcessSomeData == 2 || justProcessSomeData == 0)
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

                                    if ( lenthPosPointY == 0 && lenthPosPointX == 0 )
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
