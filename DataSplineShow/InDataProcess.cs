using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataSplineShow
{
    class InDataProcess
    {
        static Int16 DATATYPE_LENTH_FFT1 = 1;
        static Int16 DATATYPE_LENTH_FFT2 = 2;
        static Int16 DATATYPE_LENTH_POS = 3;
        static Int16 DATATYPE_LENTH_SPEED = 4;

        readonly static object lenthFFT1Locker = new object();
        readonly static object lenthFFT2Locker = new object();
        readonly static object lenthPosLocker = new object();
        readonly static object lenthSpeedLocker = new object();

        // 入队数据
        public void EnQueueLenthFFT1Pkt(LenthFFTPacket rcvData)
        {
            LenthFFTPktToQueue(rcvData, DATATYPE_LENTH_FFT1);
        }

        public void EnQueueLenthFFT2Pkt(LenthFFTPacket rcvData)
        {
            LenthFFTPktToQueue(rcvData, DATATYPE_LENTH_FFT2);
        }

        public void EnQueueLenthPosPkt(DistancePosSpeedPacket rcvData)
        {
            LenthPosSpeedPktToQueue(rcvData, DATATYPE_LENTH_POS);
        }

        public void EnQueueLenthSpeedPkt(LenthFFTPacket rcvData)
        {
            LenthFFTPktToQueue(rcvData, DATATYPE_LENTH_SPEED);
        }

        // 取出数据
        public LenthFFTPacket DeQueueLenthFFT1Pkt()
        {
            return OutLenthFFTPktFromQueue(DATATYPE_LENTH_FFT1);
        }

        public LenthFFTPacket DeQueueLenthFFT2Pkt()
        {
            return OutLenthFFTPktFromQueue(DATATYPE_LENTH_FFT2);
        }

        public DistancePosSpeedPacket DeQueueLenthPosPkt()
        {
            return OutLenthPosSpeedPktFromQueue(DATATYPE_LENTH_POS);
        }

        public DistancePosSpeedPacket DeQueueLenthSpeedPkt()
        {
            return OutLenthPosSpeedPktFromQueue(DATATYPE_LENTH_SPEED);
        }

        public void LenthFFTPktToQueue(LenthFFTPacket rcvData, Int16 type)
        {
            Action< LenthFFTPacket, Int16> lenthFFTPktProduce = LenthFFTProduct;
            lenthFFTPktProduce.Invoke(rcvData, type);
        }

        public void LenthPosSpeedPktToQueue(DistancePosSpeedPacket rcvData, Int16 type)
        {
            Action<DistancePosSpeedPacket, Int16> lenthPosSpeedPktProduce = LenthPosSpeedProduct;
            lenthPosSpeedPktProduce.Invoke(rcvData, type);
        }

        public LenthFFTPacket OutLenthFFTPktFromQueue(Int16 type)
        {
            Func<Int16, LenthFFTPacket> LenthFFTPktConsume = LenthFFTPktDeQueue;
            return LenthFFTPktConsume(type);
        }

        public DistancePosSpeedPacket OutLenthPosSpeedPktFromQueue(Int16 type)
        {
            Func<Int16, DistancePosSpeedPacket> LenthPosSpeedPktConsume = LenthPosSpeedPktDeQueue;
            return LenthPosSpeedPktConsume(type);
        }

        /// <summary>
        /// 生产
        /// </summary>
        /// <param name="name"></param>
        void LenthFFTProduct(LenthFFTPacket lenthFFTData, Int16 type)
        {
            if (type == DATATYPE_LENTH_FFT1)
            {
                lock (lenthFFT1Locker)
                {
                    new RcvDataProducer(LenthFFT1Container.lenthFFT1DataContainer).Produce(lenthFFTData);
                }
            }
            else if(type == DATATYPE_LENTH_FFT2)
            {
                lock (lenthFFT2Locker)
                {
                    new RcvDataProducer(LenthFFT2Container.lenthFFT2DataContainer).Produce(lenthFFTData);
                }
            }
        }

        /// <summary>
        /// 消费
        /// </summary>
        /// <param name="name"></param>
        public LenthFFTPacket LenthFFTPktDeQueue(Int16 type)
        {
            if (type == DATATYPE_LENTH_FFT1)
            {
                lock (lenthFFT1Locker)
                {
                    if (LenthFFT1Container.lenthFFT1DataContainer.Count > 0)
                    {
                        return new RcvDataConsume(LenthFFT1Container.lenthFFT1DataContainer).ConsumeMethod();
                    }
                }
            }
            else if (type == DATATYPE_LENTH_FFT2)
            {
                lock (lenthFFT2Locker)
                {
                    if (LenthFFT2Container.lenthFFT2DataContainer.Count > 0)
                    {
                        return new RcvDataConsume(LenthFFT2Container.lenthFFT2DataContainer).ConsumeMethod();
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// 生产
        /// </summary>
        /// <param name="name"></param>
        void LenthPosSpeedProduct(DistancePosSpeedPacket pktData, Int16 type)
        {
            if (type == DATATYPE_LENTH_POS)
            {
                lock (lenthPosLocker)
                {
                    new RcvDataProducer(DistancePosContainer.distancePosContainer).Produce(pktData);
                }
            }
            else if(type == DATATYPE_LENTH_SPEED)
            {
                lock (lenthSpeedLocker)
                {
                    new RcvDataProducer(DistanceSpeedContainer.distanceSpeedContainer).Produce(pktData);
                }
            }
        }

        /// <summary>
        /// 消费
        /// </summary>
        /// <param name="name"></param>
        public DistancePosSpeedPacket LenthPosSpeedPktDeQueue(Int16 type)
        {
            if (type == DATATYPE_LENTH_POS)
            {
                lock (lenthPosLocker)
                {
                    if (DistancePosContainer.distancePosContainer.Count > 0)
                    {
                        return new RcvDataConsume(DistancePosContainer.distancePosContainer).ConsumePosPtkMethod();
                    }
                }
            }
            else if (type == DATATYPE_LENTH_SPEED)
            {
                lock (lenthSpeedLocker)
                {
                    if (DistanceSpeedContainer.distanceSpeedContainer.Count > 0)
                    {
                        return new RcvDataConsume(DistanceSpeedContainer.distanceSpeedContainer).ConsumePosPtkMethod();
                    }
                }
            }

            return null;
        }

    }

    public class LenthFFTPacket
    {
        public static Int32 iPointNum = 257;
        public Int32[] InDataArray = new Int32[iPointNum];
    }

    public class DistancePosSpeedPacket
    {
        public static Int32 iPointCnt = 12;
        public Int32[] InDataArray = new Int32[iPointCnt];
    }

    public static class LenthFFT1Container
    {
        public static Queue<LenthFFTPacket> lenthFFT1DataContainer = new Queue<LenthFFTPacket>();
    }

    public static class LenthFFT2Container
    {
        public static Queue<LenthFFTPacket> lenthFFT2DataContainer = new Queue<LenthFFTPacket>();
    }

    public static class DistancePosContainer
    {
        public static Queue<DistancePosSpeedPacket> distancePosContainer = new Queue<DistancePosSpeedPacket>();
    }

    public static class DistanceSpeedContainer
    {
        public static Queue<DistancePosSpeedPacket> distanceSpeedContainer = new Queue<DistancePosSpeedPacket>();
    }

    /// <summary>
    /// 生产者
    /// </summary>
    public class RcvDataProducer
    {
        Queue<LenthFFTPacket> LenthFFTProducerContainer;
        Queue<DistancePosSpeedPacket> DistancePosSpeedProducerContainer;

        public RcvDataProducer(Queue<LenthFFTPacket> container)
        {
            this.LenthFFTProducerContainer = container;
        }

        public RcvDataProducer(Queue<DistancePosSpeedPacket> container)
        {
            this.DistancePosSpeedProducerContainer = container;
        }

        public void Produce(LenthFFTPacket lenthFFTData)
        {
            LenthFFTProducerContainer.Enqueue(lenthFFTData);
        }

        public void Produce(DistancePosSpeedPacket DistancePosSpeedData)
        {
            DistancePosSpeedProducerContainer.Enqueue(DistancePosSpeedData);
        }
    }

    /// <summary>
    /// 消费者
    /// </summary>
    public class RcvDataConsume
    {
        Queue<LenthFFTPacket> LenthFFTConsumerContainer;
        Queue<DistancePosSpeedPacket> DistancePosSpeedConsumerContainer;

        public RcvDataConsume(Queue<LenthFFTPacket> container)
        {
            this.LenthFFTConsumerContainer = container;
        }

        public RcvDataConsume(Queue<DistancePosSpeedPacket> container)
        {
            this.DistancePosSpeedConsumerContainer = container;
        }

        public LenthFFTPacket ConsumeMethod()
        {
            return LenthFFTConsumerContainer.Dequeue();
        }

        public DistancePosSpeedPacket ConsumePosPtkMethod()
        {
            return DistancePosSpeedConsumerContainer.Dequeue();
        }
    }
}
