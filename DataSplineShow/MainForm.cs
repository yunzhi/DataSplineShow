﻿using DevComponents.DotNetBar;
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
            LenthPosChartControlDisplay LenthPosChartControlInstance = new LenthPosChartControlDisplay();
            LenthSpeedChartControlDisplay LenthSpeedChartControlInstance = new LenthSpeedChartControlDisplay();

            this.lenthFFTNavPanel.Controls.Add(LenthFFTChartControlDisplay.LenthFFTChartControl);
            this.lenthPosNavPanel.Controls.Add(LenthPosChartControlDisplay.LenthPosChartControl);
            this.lenthSpeedNavPanel.Controls.Add(LenthSpeedChartControlDisplay.LenthSpeedChartControl);
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
                            //DateTime begin = DateTime.Now;
                            CheckAnalysisRcvPacket(arrRecvmsg, length);
                            //DateTime end = DateTime.Now;
                            //Console.WriteLine("period secs: " + ExecDateDiff(begin, end));
                            ////Console.WriteLine("rcv: " + length);
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
            //Console.WriteLine("arrRecvmsglist.Count = " + arrRecvmsglist.Count + ", len = " + len);

            for (int i = 0; i < len; i++)
                arrRecvmsglist.Add(rcvPacket[i]);

            if( arrRecvmsglist.Count < LeastPacketLenth)
            {
                Console.WriteLine("len {0} < LeastPacketLenth {1}", len, LeastPacketLenth);
                return false;
            }

            int index, head_index, lenthFFT2HeadIndex, targetPosHeadIndex, lenthSpeedHeadIndex;
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
                            lenthFFT1DrawData.InDataArray[i] = (UInt16)((lenthFFT1Data[2 + i * 2]) | (UInt16)(lenthFFT1Data[2 + i * 2 + 1] << 8));
                        }
                        AllInDataProcess.EnQueueLenthFFT1Pkt(lenthFFT1DrawData);
                        
                        if (no_debug_output == false)
                        {
                            StringBuilder strDebugFFT1Out = new StringBuilder();
                            for (int i = 0; i < iPointNum + 2; i++)
                            {
                                strDebugFFT1Out.AppendFormat("{0:X2} {1:X2} ", lenthFFT1Data[i * 2], lenthFFT1Data[i * 2 + 1]);
                            }
                            strDebugFFT1Out.Append("\r\n\r\n");
                            AppendColorText2RichBox(strDebugFFT1Out.ToString());
                            strDebugFFT1Out.Clear();
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
                            lenthFFT2DrawData.InDataArray[i] = (UInt16)((lenthFFT2Data[2 + i * 2]) | (UInt16)(lenthFFT2Data[2 + i * 2 + 1] << 8));
                        }
                        AllInDataProcess.EnQueueLenthFFT2Pkt(lenthFFT2DrawData);

                        if (no_debug_output == false)
                        {
                            StringBuilder strDebugFFT2Out = new StringBuilder();
                            for (int i = 0; i < iPointNum + 2; i++)
                            {
                                strDebugFFT2Out.AppendFormat("{0:X2} {1:X2} ", lenthFFT2Data[i * 2], lenthFFT2Data[i * 2 + 1]);
                            }
                            strDebugFFT2Out.Append("\r\n\r\n");
                            AppendColorText2RichBox(strDebugFFT2Out.ToString());
                            strDebugFFT2Out.Clear();
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
                if( targetPosHeadIndex + LeastTargetPosPktLenth > arrRecvmsglist.Count )
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
                            StringBuilder strDebugTargetPosOut = new StringBuilder();
                            foreach (byte element in targetPosData)
                            {
                                strDebugTargetPosOut.AppendFormat("{0:X2} ", element);
                            }
                            strDebugTargetPosOut.Append("\r\n\r\n");
                            AppendColorText2RichBox(strDebugTargetPosOut.ToString());
                            strDebugTargetPosOut.Clear();
                        }

                        arrRecvmsglist.RemoveRange(0, targetPosHeadIndex + LeastTargetPosPktLenth);
                    }
                    else
                    {
                        if (targetPosHeadIndex  == 0)
                            arrRecvmsglist.RemoveAt(0);
                        else
                            arrRecvmsglist.RemoveRange(0, targetPosHeadIndex);
                    }
                }

                lenthSpeedHeadIndex = arrRecvmsglist.IndexOf(lenthSpeedPacketHead[0]);
                if (lenthSpeedHeadIndex + LeastLenthSpeedPosPktLenth > arrRecvmsglist.Count)
                {
                    Console.WriteLine("too short, lenthSpeedHeadIndex = " + lenthSpeedHeadIndex);
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
                            StringBuilder strDebugSpeedOut = new StringBuilder();
                            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
                            {
                                strDebugSpeedOut.AppendFormat("{0:X2} {1:X2} ", lenthSpeedData[i * 2], lenthSpeedData[i * 2 + 1]);
                            }
                            strDebugSpeedOut.AppendFormat("\r\n\r\n");
                            AppendColorText2RichBox(strDebugSpeedOut.ToString());
                            strDebugSpeedOut.Clear();
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

                int lenthPosHeadIndex = arrRecvmsglist.IndexOf(lenthPosPacketHead[0]);
                if (lenthPosHeadIndex + LeastLenthSpeedPosPktLenth > arrRecvmsglist.Count)
                {
                    Console.WriteLine("too short, lenthSpeedHeadIndex = " + lenthPosHeadIndex);
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
                            DistancePosDrawData.InDataArray[i] = (Int16)((lenthPosData[2 + i * 2]) | (UInt16)(lenthPosData[2 + i * 2 + 1] << 8));

                        }
                        AllInDataProcess.EnQueueLenthPosPkt(DistancePosDrawData);

                        if (no_debug_output == false)
                        {
                            StringBuilder strDebugPosOut = new StringBuilder();
                            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
                            {
                                strDebugPosOut.AppendFormat("{0:X2} {1:X2} ", lenthPosData[i * 2], lenthPosData[i * 2 + 1]);
                            }
                            strDebugPosOut.AppendFormat("\r\n\r\n");
                            AppendColorText2RichBox(strDebugPosOut.ToString());
                            strDebugPosOut.Clear();
                        }

                        arrRecvmsglist.RemoveRange(0, lenthPosHeadIndex + LeastLenthSpeedPosPktLenth);
                    }
                    else
                    {
                        if (lenthPosHeadIndex == 0)
                            arrRecvmsglist.RemoveAt(0);
                        else
                            arrRecvmsglist.RemoveRange(0, lenthPosHeadIndex);
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

            Random targetPosDataRandom = new Random(DateTime.Now.Millisecond + 100);
            targetPosDataRandom.NextBytes(targetPosData);
            targetPosData[0] = targetPosPacketHead[0];
            targetPosData[1] = targetPosPacketHead[1];
            targetPosData[LeastTargetPosPktLenth - 2] = targetPosPacketTail[0];
            targetPosData[LeastTargetPosPktLenth - 1] = targetPosPacketTail[1];

            Random lenthSpeedDataRandom = new Random(DateTime.Now.Millisecond + 100);
            lenthSpeedDataRandom.NextBytes(lenthSpeedData);
            lenthSpeedData[0] = lenthSpeedPacketHead[0];
            lenthSpeedData[1] = lenthSpeedPacketHead[1];
            lenthSpeedData[LeastLenthSpeedPosPktLenth - 2] = lenthSpeedPacketTail[0];
            lenthSpeedData[LeastLenthSpeedPosPktLenth - 1] = lenthSpeedPacketTail[1];

            Random lenthPosDataRandom = new Random(DateTime.Now.Millisecond + 200);
            lenthPosDataRandom.NextBytes(lenthPosData);
            lenthPosData[0] = lenthPosPacketHead[0];
            lenthPosData[1] = lenthPosPacketHead[1];
            lenthPosData[LeastLenthSpeedPosPktLenth - 2] = lenthPosPacketTail[0];
            lenthPosData[LeastLenthSpeedPosPktLenth - 1] = lenthPosPacketTail[1];

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

            DistancePosSpeedPacket lenthSpeedDrawData = new DistancePosSpeedPacket();
            DistancePosSpeedPacket lenthPosDrawData = new DistancePosSpeedPacket();
            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i++)
            {
                lenthSpeedDrawData.InDataArray[i] = (Int16)((lenthSpeedData[2 + i * 2]) | (UInt16)(lenthSpeedData[2 + i * 2 + 1] << 8));
                lenthPosDrawData.InDataArray[i] = (Int16)((lenthPosData[2 + i * 2]) | (UInt16)(lenthPosData[2 + i * 2 + 1] << 8));
            }

            StringBuilder strDebugFFT1Out = new StringBuilder();
            StringBuilder strDebugFFT2Out = new StringBuilder();
            for (int i = 0; i < iPointNum + 2; i++)
            {
                strDebugFFT1Out.AppendFormat("{0:X2} {1:X2} ", lenthFFT1Data[i * 2], lenthFFT1Data[i * 2 + 1]);
                strDebugFFT2Out.AppendFormat("{0:X2} {1:X2} ", lenthFFT2Data[i * 2], lenthFFT2Data[i * 2 + 1]);
            }
            strDebugFFT1Out.Append("\r\n\r\n");
            strDebugFFT2Out.Append("\r\n\r\n");
            AppendColorText2RichBox(strDebugFFT1Out.ToString());
            strDebugFFT1Out.Clear();
            AppendColorText2RichBox(strDebugFFT2Out.ToString());
            strDebugFFT2Out.Clear();

            StringBuilder strDebugTargetPosOut = new StringBuilder();
            foreach( byte element in targetPosData)
            {
                strDebugTargetPosOut.AppendFormat("{0:X2} ", element);
            }
            strDebugTargetPosOut.Append("\r\n\r\n");
            AppendColorText2RichBox(strDebugTargetPosOut.ToString());
            
            StringBuilder strDebugSpeedOut = new StringBuilder();
            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
            {
                strDebugSpeedOut.AppendFormat("{0:X2} {1:X2} ", lenthSpeedData[i * 2], lenthSpeedData[i * 2 + 1]);
            }
            strDebugSpeedOut.AppendFormat("\r\n\r\n");
            AppendColorText2RichBox(strDebugSpeedOut.ToString());

            StringBuilder strDebugPosOut = new StringBuilder();
            for (int i = 0; i < DistancePosSpeedPacket.iPointCnt + 2; i++)
            {
                strDebugPosOut.AppendFormat("{0:X2} {1:X2} ", lenthPosData[i * 2], lenthPosData[i * 2 + 1]);
            }
            strDebugPosOut.AppendFormat("\r\n\r\n");
            AppendColorText2RichBox(strDebugPosOut.ToString());


            AllInDataProcess.EnQueueLenthFFT1Pkt(lenthFFT1DrawData);
            AllInDataProcess.EnQueueLenthFFT2Pkt(lenthFFT2DrawData);
            AllInDataProcess.EnQueueTargetPosPkt(targetPosDrawData);
            AllInDataProcess.EnQueueLenthSpeedPkt(lenthSpeedDrawData);
            AllInDataProcess.EnQueueLenthPosPkt(lenthPosDrawData);
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
            ChartXy LenthFFTdrawChartXy = LenthFFTChartControlDisplay.LenthFFTChartControl.ChartPanel.ChartContainers[0] as ChartXy;
            ChartXy lenthPosdrawChartXy = LenthPosChartControlDisplay.LenthPosChartControl.ChartPanel.ChartContainers[0] as ChartXy;
            ChartXy lenthSpeeddrawChartXy = LenthSpeedChartControlDisplay.LenthSpeedChartControl.ChartPanel.ChartContainers[0] as ChartXy;

            TargetPosPacket outTargetPosPoint = null;
            LenthFFTPacket outLenthFFT1Pkt = null;
            LenthFFTPacket outLenthFFT2Pkt = null;
            DistancePosSpeedPacket outLenthSpeedPkt = null;
            DistancePosSpeedPacket outLenthPosPkt = null;

            //if (TargetPosContainer.targetPosContainer.Count == LenthFFT1Container.lenthFFT1DataContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count == LenthFFT2Container.lenthFFT2DataContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count == DistanceSpeedContainer.distanceSpeedContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count == DistancePosContainer.distancePosContainer.Count &&
            //    TargetPosContainer.targetPosContainer.Count != 0)

            if (TargetPosContainer.targetPosContainer.Count > 0 &&
                LenthFFT1Container.lenthFFT1DataContainer.Count > 0 &&
                LenthFFT2Container.lenthFFT2DataContainer.Count > 0 &&
                DistanceSpeedContainer.distanceSpeedContainer.Count > 0 &&
                DistancePosContainer.distancePosContainer.Count > 0)
            {
                outTargetPosPoint = AllInDataProcess.DeQueueTargetPosPkt();
                outLenthFFT1Pkt = AllInDataProcess.DeQueueLenthFFT1Pkt();
                outLenthFFT2Pkt = AllInDataProcess.DeQueueLenthFFT2Pkt();
                outLenthSpeedPkt = AllInDataProcess.DeQueueLenthSpeedPkt();
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
                    lenthSpeedDrawPointList.Add(new SeriesPoint(Convert.ToDouble( outLenthSpeedPkt.InDataArray[i])/100, Convert.ToDouble(outLenthSpeedPkt.InDataArray[i + 1])/100));
                }

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

            if (outLenthPosPkt != null)
            {
                List<SeriesPoint> lenthPosDrawPointList = new List<SeriesPoint>();
                for (int i = 0; i < DistancePosSpeedPacket.iPointCnt; i += 2)
                {
                    lenthPosDrawPointList.Add(new SeriesPoint(Convert.ToDouble(outLenthPosPkt.InDataArray[i])/100, Convert.ToDouble(outLenthPosPkt.InDataArray[i + 1])/100));
                }

                if (displayLenthSpeedstring == null)
                {
                    displayLenthSpeedstring = new StringBuilder();
                }
                displayLenthSpeedstring.Append("DistanceAzimuth data start:");
                foreach (SeriesPoint point in lenthPosDrawPointList)
                {
                    displayLenthSpeedstring.Append("(" + point.ValueX + "," + point.ValueY[0] + ")");
                }
                displayLenthSpeedstring.Append("DistanceAzimuth data end\r\n");

                IEnumerable<SeriesPoint> sortedPosSeriesPointList =
                    from singlePoint in lenthPosDrawPointList
                    orderby singlePoint.ValueX
                    select singlePoint;

                for (int i = 0; i < lenthPosDrawPointList.Count; i++)
                {
                    if (lenthPosDrawPointList.ElementAt(i).ValueX != sortedPosSeriesPointList.ElementAt(i).ValueX)
                    {
                        lenthSpeedPosDataNotSorted = true;
                        break;
                    }
                }

                ChartSeries lenthPosChartSeries = lenthPosdrawChartXy.ChartSeries["DistanceAzimuth"];
                lenthPosChartSeries.PointLabelDisplayMode |= PointLabelDisplayMode.AllSeriesPoints;
                if (lenthPosChartSeries != null)
                {
                    this.Invoke((EventHandler)(delegate
                    {
                        lenthPosChartSeries.SeriesPoints.Clear();
                        lenthPosChartSeries.SeriesPoints.AddRange(sortedPosSeriesPointList);
                    }));
                }
            }

            if ( no_debug_output == false )
            {
                if( displayLenthSpeedstring != null )
                    AppendColorText2RichBox(displayLenthSpeedstring.ToString());
                if(lenthSpeedPosDataNotSorted == true)
                    MessageBox.Show("收到的方位或速度数据不是按照距离升序的","提示");
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
    }
}
