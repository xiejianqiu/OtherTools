using System.Net.Sockets;
using System;
namespace CoreLib
{
    class SocketOutputStream
    {
        public static int DEFAULT_SOCKET_OUTPUT_BUFFER_SIZE = 8192;
        public static int DISCONNECT_SOCKET_OUTPUT_SIZE = 102400;

        Socket m_Socket = null;
        int m_nBuffSize = 0;
        int m_nMaxBuffSize = 0;
        int m_nHead = 0;
        int m_nTail = 0;

        byte[] m_Buff = null;

        public SocketOutputStream(Socket vSocket,int nBuffLen,int nMaxBuffLen)
        {
            m_Socket = vSocket;
            m_nBuffSize = nBuffLen;
            m_nMaxBuffSize = nMaxBuffLen;

            m_nHead = 0;
            m_nTail = 0;

            m_Buff = new byte[nBuffLen];
        }
        void CleanUp()
        {
            m_nHead = 0;
            m_nTail = 0;
        }

        public int Length()
        {
            if (m_nHead < m_nTail)
                return m_nTail - m_nHead;

            else if (m_nHead > m_nTail)
                return m_nBuffSize - m_nHead + m_nTail;

            return 0;
        }

        public void InitSize()
        {
            m_nHead = 0;
            m_nTail = 0;

            m_Buff = new byte[DEFAULT_SOCKET_OUTPUT_BUFFER_SIZE];
            m_nBuffSize = DEFAULT_SOCKET_OUTPUT_BUFFER_SIZE;
        }

        public int Flush()
        {
            int nFlushed = 0;
            int nSent = 0;
            int nLeft;

            if(m_nBuffSize > m_nMaxBuffSize)
            {
                InitSize();
                return -1;
            }

            try
            {
                if (m_nHead < m_nTail)
                {
                    nLeft = m_nTail - m_nHead;

                    while (nLeft > 0)
                    {
                        nSent = m_Socket.Send(m_Buff, m_nHead, nLeft, 0);
                        if (nSent == 0)
                        {
                            return -2;
                        }

                        nFlushed += nSent;
                        nLeft -= nSent;
                        m_nHead += nSent;
                    }
                }
                else if (m_nHead > m_nTail)
                {
                    nLeft = m_nBuffSize - m_nHead;

                    while (nLeft > 0)
                    {
                        nSent = m_Socket.Send(m_Buff, m_nHead, nLeft, 0);
                        if (nSent == 0)
                        {
                            return 0;
                        }

                        nFlushed += nSent;
                        nLeft -= nSent;
                        m_nHead += nSent;
                    }

                    m_nHead = 0;

                    nLeft = m_nTail;
                    while (nLeft > 0)
                    {
                        nSent = m_Socket.Send(m_Buff, m_nHead, nLeft, 0);
                        if (0 == nSent)
                        {
                            return 0;
                        }

                        nFlushed += nSent;
                        nLeft -= nSent;
                        m_nHead += nSent;
                    }


                }

                if (m_nHead != m_nTail)
                {

                }
            }
            catch
            {
                if(nSent >0)
                {
                    m_nHead += nSent;
                }
            }

            m_nHead = m_nTail = 0;
            return nFlushed;

        }

        bool Resize(int nSize)
        {
            int orgSize = nSize;

            nSize = System.Math.Max(nSize,m_nBuffSize>>1);
            int newBuffSize = m_nBuffSize + nSize;
            int nLen = Length();

            if(nSize < 0)
            {
                if(newBuffSize<0||newBuffSize<nLen)
                {
                    return false;
                }
            }

            byte[] newBuff = new byte[newBuffSize];
            if(null == newBuff)
            {
                return false;
            }

            if(m_nHead < m_nTail)
            {
                Array.Copy(m_Buff,m_nHead,newBuff,0,m_nTail-m_nHead);
            }
            else if(m_nHead > m_nTail)
            {
                Array.Copy(m_Buff,m_nHead,newBuff,0,m_nBuffSize-m_nHead);
                Array.Copy(m_Buff,0,newBuff,m_nBuffSize-m_nHead, m_nTail);
            }

            m_Buff = newBuff;
            m_nBuffSize = newBuffSize;
            m_nHead = 0;
            m_nTail = nLen;
            return false;
        }
    }
}
