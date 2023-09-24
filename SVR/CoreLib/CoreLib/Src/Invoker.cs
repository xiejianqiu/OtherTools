using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace CoreLib
{
    public class Invoker
    {
        public enum InvokerState
        {
            STATE_READY = 0,
            STATE_SCHEDULING,
            STATE_RUNNING,
        }

        WaitCallback m_Callback = null;
        long m_nNormalTickInterval = 0;
        long m_nSlowTickInterval = 0;
        long m_nDoingInterval = 0;
        long m_nScheduleTime = 0;
        long m_nRunTime = 0;
        AbstractService m_Service = null;
        private Thread _thread = null;
        InvokerState m_emState = InvokerState.STATE_READY;

        public Invoker(AbstractService vService,int nNormalTickInterval, int nSlowTickInterval)
        {
            m_Callback = new WaitCallback(Do);
            m_nNormalTickInterval = nNormalTickInterval;
            m_nSlowTickInterval = nSlowTickInterval;
            m_nDoingInterval = 0;
            m_Service = vService;
            SetState(InvokerState.STATE_READY);
            m_nRunTime = 0;
            m_nScheduleTime = 0;
        }

        void Do(object o)
        {
            _thread = Thread.CurrentThread;

            SetState(InvokerState.STATE_RUNNING);

            m_Service.TickTime();
            m_Service.Tick(m_Service.CurTime);

            m_nDoingInterval = 0;

            SetState(InvokerState.STATE_READY);

            _thread = null;
        }

        public WaitCallback GetWaitCallback() { return m_Callback; }
        public InvokerState GetState() { return m_emState; }
        public void SetState(InvokerState emState)
        {
            m_emState = emState;
            if(InvokerState.STATE_SCHEDULING == m_emState)
            {
                m_nScheduleTime = 0;
            }
            else if(InvokerState.STATE_RUNNING == m_emState)
            {
                m_nRunTime = 0;
            }
        }

        public ServiceRunStatus GetRunStatus() { return m_Service.GetRunStatus(); }

        public void UpdateDoingInterval(long nElapse) { m_nDoingInterval += nElapse; }
        public bool IsOverNormalDoingInterval() { return (m_nDoingInterval >= m_nNormalTickInterval); }
        public bool IsOverSlowDoingInterval() { return (m_nDoingInterval >= m_nSlowTickInterval); }

        public void CheckScheduleState(long nElapse)
        {
            m_nScheduleTime += nElapse;
            if(m_nScheduleTime > 600000)
            {
                m_nScheduleTime -= 600000;
                string msg = string.Format("wait too long for scheduling! In service: {0}，Time:{1}", m_Service.GetType(), DateTime.Now.ToString());
                File.AppendAllText("corelib.log",msg + Environment.NewLine);
            }
        }

        public void CheckRunState(long nElapse)
        {
            m_nRunTime += nElapse;
            if(m_nRunTime > 600000)
            {
                m_nRunTime -= 600000;
                string msg = string.Format("running too long!! In service: {0}，Time:{1}", m_Service.GetType(),DateTime.Now.ToString());
                File.AppendAllText("corelib.log", msg + Environment.NewLine);
                string info = "";
                try
                {
                    if (_thread != null)
                    {
                        _thread.Suspend();
                        var st = new StackTrace(_thread, true);
                        info = st.ToString();
                    }
                    else
                    {
                        info = "_thread is null!";
                    }
                }
                catch (Exception e)
                {
                    info = e.ToString();
                }
                finally
                {
                    if (_thread!=null && (_thread.ThreadState & ThreadState.Suspended) != 0)
                    {
                        _thread.Resume();
                    }
                    File.AppendAllText("corelib.log", info);
                }
                
            }
        }
    }
}
