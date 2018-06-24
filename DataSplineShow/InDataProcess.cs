using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataSplineShow
{
    class InDataProcess
    {
        readonly static object _locker = new object();

        public InDataProcess()
        {

        }

        public void InDataProcessWork()
        {
            //生产者消费者模式
            //开启生产
            Action<InData> produce = Product;
            InData firstData = new InData();
            produce.BeginInvoke(firstData, null, null);

            //开启消费
            Action<InData> consume = Consume;
            consume.BeginInvoke(null, null, null);
        }

        /// <summary>
        /// 生产
        /// </summary>
        /// <param name="name"></param>
        static void Product(InData lenthFFTData)
        {
            lock (_locker)
            {
                if (Container.lenthFFTDataContainer.Count == 0)
                {
                    new Producer(Container.lenthFFTDataContainer).Produce(lenthFFTData);
                }
            }
        }
        /// <summary>
        /// 消费
        /// </summary>
        /// <param name="name"></param>
        static void Consume(InData lenthFFTData)
        {
            lock (_locker)
            {
                if (Container.lenthFFTDataContainer.Count > 0)
                {
                    new Consumer(Container.lenthFFTDataContainer).Consume();
                }
            }
        }
    }

    public class InData
    {
        public static Int32 iPointNum = 257;
        public Int32[] InDataArray = new Int32[iPointNum];
    }

    public static class Container
    {
        public static Queue<InData> lenthFFTDataContainer = new Queue<InData>();
    }

    /// <summary>
    /// 生产者
    /// </summary>
    public class Producer
    {
        Queue<InData> ProducerContainer = new Queue<InData>();
        public Producer(Queue<InData> container)
        {
            this.ProducerContainer = container;
        }
        public void Produce(InData lenthFFTData)
        {
            ProducerContainer.Enqueue(lenthFFTData);
        }
    }

    /// <summary>
    /// 消费者
    /// </summary>
    public class Consumer
    {
        Queue<InData> ConsumerContainer = new Queue<InData>();
        public Consumer(Queue<InData> container)
        {
            this.ConsumerContainer = container;
        }
        public void Consume()
        {
            var DataValue = ConsumerContainer.Dequeue();
            foreach (Int32 drawPoint in DataValue.InDataArray)
            {
                Console.Write(drawPoint + " ");
            }
            Console.WriteLine();
        }
    }
}
