using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib
{
    public sealed class SerialGenerater
    {
        private long m_nNextSerial = 0;
        private long m_nMaxSerial = 0;
        private bool m_bInit = false;
        private object m_lock = new object();
        private bool m_bMaxLimit = true;

        public SerialGenerater(bool bMaxLimit)
        {
            m_bMaxLimit = bMaxLimit;
        }

        public void InitWithServerId(int nServerId)
        {
            long nHigh = (long)nServerId / 256;
            long nLow = (long)nServerId % 256;
            m_nNextSerial = (nHigh << 56) + (nLow << 48);
            m_nMaxSerial = m_nNextSerial + 0xDFFFFFFFFFFF;
            m_bInit = true;
        }

        public void InitWithValue(long nValue)
        {
            m_nNextSerial = nValue;
            m_nMaxSerial = m_nNextSerial >> 48;
            m_nMaxSerial = m_nMaxSerial << 48;
            m_nMaxSerial += 0xDFFFFFFFFFFF;
            m_bInit = true;
        }

        public long GenSerial()
        {
            long uRet = m_nNextSerial;
            lock(m_lock)
            {
                uRet = m_nNextSerial++;
            }
            if(m_bMaxLimit && uRet > m_nMaxSerial)
            {

            }
            return uRet;
        }
    }
}
