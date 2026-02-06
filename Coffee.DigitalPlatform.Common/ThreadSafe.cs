using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public class CounterInterlocked
    {
        private int _count = 0;

        public CounterInterlocked(int initSeed = 0)
        {
            if (initSeed < 0)
                throw new ArgumentException("初始值必须大于等于0");
            _count = initSeed;
        }

        // 线程安全递增
        public int Increment()
        {
            return Interlocked.Increment(ref _count);
        }

        // 递增并返回递增前的值
        public int GetAndIncrement()
        {
            return Interlocked.Increment(ref _count) - 1;
        }

        // 递增指定值
        public int Add(int value)
        {
            return Interlocked.Add(ref _count, value);
        }

        // 获取当前值（注意：这里需要额外的同步才能保证获取的即时性）
        public int GetCount()
        {
            return Volatile.Read(ref _count);
        }
    }
}
