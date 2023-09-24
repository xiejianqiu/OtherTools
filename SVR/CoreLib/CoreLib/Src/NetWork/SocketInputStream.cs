
using System.Net.Sockets;
using System;
namespace CoreLib
{
    class SocketInputStream
    {
        public static int DEFAULT_SOCKET_INPUT_STREAM_SIZE = 64 * 1024;
        public static int DISCONNECT_SOCKET_OUTPUT_STREAM_SIZE = 96 * 1024;

        Socket m_Socket = null;
        int m_nBuffSize = 0;
        int m_nMaxBuffSize = 0;
        int m_nHead = 0;
        int m_nTail = 0;
        byte[] m_Buffer = null;

        public SocketInputStream(Socket vSocket,int nBuffSize,int nMaxBuffSize)
        {
            m_Socket = vSocket;
            m_nBuffSize = nBuffSize;
            m_nMaxBuffSize = nMaxBuffSize;
            m_Buffer = new byte[m_nBuffSize];

        }
        public void CleanUp()
        {
            m_nHead = 0;
            m_nTail = 0;
        }
        public void Initsize()
        {
            m_nHead = 0;
            m_nTail = 0;

            m_Buffer = new byte[DEFAULT_SOCKET_INPUT_STREAM_SIZE];
            m_nBuffSize = DEFAULT_SOCKET_INPUT_STREAM_SIZE;


        }
        public int Length()
        {
            if(m_nHead<m_nTail)
            {
                return m_nTail - m_nHead;
            }
            else if( m_nHead > m_nTail)
            {
                return m_nBuffSize - m_nHead + m_nTail;
            }
            return 0;
        }
        public bool Resize(int nSize)
        {
            nSize = Math.Max(nSize,(int)(m_nBuffSize>>1));
            int nNewSize = m_nBuffSize + nSize;
            int nLen = Length();

            if( nSize < 0 )
            {
                if(nNewSize <0 || nNewSize < nLen)
                {
                    return false;
                }
            }

            byte[] newBuff = new byte[nNewSize];

            if(m_nHead < m_nTail)
            {
                Array.Copy(m_Buffer,m_nHead,newBuff,0,m_nTail-m_nHead);
            }
            else if(m_nHead > m_nTail)
            {
                Array.Copy(m_Buffer,m_nHead,newBuff,0,m_nBuffSize-m_nHead);
                Array.Copy(m_Buffer,0,newBuff,m_nBuffSize-m_nHead,m_nTail);
            }

            m_Buffer = newBuff;
            m_nHead = 0;
            m_nTail = nLen;

            return true;
        }
        public bool ReadPacket()
        {
            return true;
        }

        public int Fill()
        {
            int nFilled = 0;
            int nReceived = 0;
            int nFree = 0;

            if( m_nHead <= m_nTail )
            {
                if(m_nHead == 0)
                {
                    nReceived = 0;
                    nFree = m_nBuffSize - m_nTail - 1;
                    if(nFree != 0)
                    {
                        nReceived = m_Socket.Receive(m_Buffer, m_nTail, nFree, 0);
                        if(0 == nReceived)
                        {
                            return -1;
                        }

                        m_nTail += nReceived;
                        nFilled += nReceived;
                    }

                    if(nReceived == nFree)
                    {
                        int nAvailable = m_Socket.Available;
                        if(nAvailable > 0)
                        {
                            if(m_nBuffSize+nAvailable+1>m_nMaxBuffSize)
                            {
                                Initsize();
                                return -2;
                            }

                            if(!Resize(nAvailable + 1))
                            {
                                return 0;
                            }

                            nReceived = m_Socket.Receive(m_Buffer,m_nTail,nAvailable,0);
                            if(nReceived == 0)
                            {
                                return -3;
                            }

                            m_nTail += nReceived;
                            nFilled += nReceived;
                        }
                    }
                }
                else
                {
                    nFree = m_nBuffSize - m_nTail;
                    nReceived = m_Socket.Receive(m_Buffer, m_nTail, nFree, 0);
                    if (0 == nReceived)
                    {
                        return -4;
                    }

                    m_nTail = (m_nTail + nReceived) % m_nBuffSize;
                    nFilled += nReceived;

                    if (nReceived == nFree)
                    {
                        nReceived = 0;
                        nFree = m_nHead - 1;
                        if (nFree != 0)
                        {
                            nReceived = m_Socket.Receive(m_Buffer, 0, nFree, 0);
                            if (0 == nReceived)
                            {
                                return -5;
                            }

                            m_nTail += nReceived;
                            nFilled += nReceived;
                        }

                        if (nReceived == nFree)
                        {
                            int nAvailable = m_Socket.Available;
                            if (nAvailable > 0)
                            {
                                if (m_nBuffSize + nAvailable + 1 > m_nMaxBuffSize)
                                {
                                    Initsize();
                                    return -6;
                                }

                                if (!Resize(nAvailable + 1))
                                {
                                    return 0;
                                }

                                nReceived = m_Socket.Receive(m_Buffer, m_nTail, nAvailable, 0);
                                if (0 == nReceived)
                                {
                                    return -7;
                                }

                                m_nTail += nReceived;
                                nFilled += nReceived;
                            }
                        }
                    }
                }
            }
            else
            {
                nReceived = 0;
                nFree = m_nHead - m_nTail - 1;
                if( nFree !=0 )
                {
                    nReceived = m_Socket.Receive(m_Buffer,m_nTail,nFree,0);
                    if(nReceived == 0)
                    {
                        return -8;
                    }

                    m_nTail += nReceived;
                    nFilled += nReceived;
                }

                if(nReceived == nFree)
                {
                    int nAvailable = m_Socket.Available;
                    if(nAvailable > 0)
                    {
                        if(m_nBuffSize+nAvailable+1>m_nMaxBuffSize)
                        {
                            Initsize();
                            return -9;
                        }

                        if(!Resize(nAvailable+1))
                        {
                            return 0;
                        }

                        nReceived = m_Socket.Receive(m_Buffer,m_nTail,nAvailable,0);
                        if(0 == nReceived)
                        {
                            return -10;
                        }

                        m_nTail += nReceived;
                        nFilled += nReceived;

                    }
                }


            }
            return nFilled;
        }



    }
}
