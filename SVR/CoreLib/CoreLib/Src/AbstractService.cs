using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib
{
    public enum ServiceRunStatus
    {
        RunStatus_Normal,
        RunStatus_Slowly,  //是需要这个服务调用的频率慢一些
        RunStatus_Pause,
    }

    public abstract class AbstractService
    {
        private ConcurrentStack<Invoker> m_lstInvoker = new ConcurrentStack<Invoker>();
        protected ServiceStatus m_eStatus;
        private long m_nCurTime = 0;
        private long m_nElapsedTime = 0;

        public abstract void Tick(long nElapnTseTime);
        public abstract ServiceType GetServiceId();
        public abstract void ReceiveTask(object vTask);
        public virtual void Init() { m_nCurTime = TimeManager.Instance.GetLocalMilliseconds(); m_nElapsedTime = 0; }

        public ServiceStatus GetServiceStatus() { return m_eStatus; }
        public void SetServiceStatus(ServiceStatus emStatus) { m_eStatus = emStatus; }
        public void TickTime()
        {
            m_nElapsedTime = m_nCurTime;
            m_nCurTime = TimeManager.Instance.GetLocalMilliseconds();
            m_nElapsedTime = m_nCurTime - m_nElapsedTime;
        }
        public long CurTime
        {
            get { return m_nCurTime; }
        }
        public long ElapsedTime
        {
            get { return m_nElapsedTime; }
        }
        public virtual ServiceRunStatus GetRunStatus()
        {
            return GetRunStatus_General();
        }

        private ServiceRunStatus GetRunStatus_General()
        {
            ServiceRunStatus eRet = ServiceRunStatus.RunStatus_Pause;
            switch (m_eStatus)
            {
                case ServiceStatus.Status_Empty:
                    {
                        eRet = ServiceRunStatus.RunStatus_Pause;
                    }
                    break;
                case ServiceStatus.Status_OpenUp:
                case ServiceStatus.Status_OpenUp_Process:
                case ServiceStatus.Status_Running:
                case ServiceStatus.Status_ShutDown:
                case ServiceStatus.Status_ShutDown_Process:
                case ServiceStatus.Status_FinialSave:
                case ServiceStatus.Status_FinialSave_Process:
                    {
                        eRet = ServiceRunStatus.RunStatus_Normal;
                    }
                    break;
                case ServiceStatus.Status_OpenUp_OK:
                case ServiceStatus.Status_ShutDown_OK:
                case ServiceStatus.Status_FinialSave_OK:
                    {
                        eRet = ServiceRunStatus.RunStatus_Pause;
                    }
                    break;
                default:
                    break;
            }
            return eRet;
        }

        public ServiceRunStatus GetRunStatus_Base()
        {
            ServiceRunStatus eRet = ServiceRunStatus.RunStatus_Pause;
            switch (m_eStatus)
            {
                case ServiceStatus.Status_Empty:
                    {
                        eRet = ServiceRunStatus.RunStatus_Pause;
                    }
                    break;
                case ServiceStatus.Status_OpenUp:
                case ServiceStatus.Status_OpenUp_Process:
                case ServiceStatus.Status_Running:
                case ServiceStatus.Status_ShutDown:
                case ServiceStatus.Status_ShutDown_Process:
                case ServiceStatus.Status_FinialSave:
                case ServiceStatus.Status_FinialSave_Process:
                case ServiceStatus.Status_OpenUp_OK:
                case ServiceStatus.Status_ShutDown_OK:
                case ServiceStatus.Status_FinialSave_OK:
                    {
                        eRet = ServiceRunStatus.RunStatus_Normal;
                    }
                    break;
                default:
                    break;
            }
            return eRet;
        }

        public Invoker PopInvoker()
        {
            Invoker vRet = null;
            if(m_lstInvoker.Count >0)
            {
                m_lstInvoker.TryPop(out vRet);
            }
            return vRet;
        }

        public void AddInvoker(Invoker vInvoker)
        {
            m_lstInvoker.Push(vInvoker);
        }

    }
}
