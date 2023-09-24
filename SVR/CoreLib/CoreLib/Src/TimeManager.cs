using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    public sealed class TimeManager
    {
        private long m_nStartTime = 0;

        public static readonly TimeManager Instance = new TimeManager();

        public TimeManager()
        {
            m_nStartTime = GetLocalMilliseconds();
        }

        public long GetLocalMilliseconds()
        {
            return DateTime.Now.Ticks / 10000;
        }

        public long GetRunTime()
        {
            return GetLocalMilliseconds() - m_nStartTime;
        }

    }
}
