using System;
using System.Collections.Generic;
using System.Threading;

namespace CoreLib
{
    public enum ServiceType
    {
        ServiceType_UnKnow = -1,
        ServiceType_Login,
        ServiceType_DB,
        ServiceType_Mail,
        ServiceType_Scene,
        ServiceType_Chat,
        ServiceType_Team,
        ServiceType_Rank,
        ServiceType_Guild,
        ServiceType_Friend,
        ServiceType_Auction,
        ServiceType_PVP,
        ServiceType_Billing,
        ServiceType_Global,
        ServiceType_Push,
        ServiceType_Activity,
        ServiceType_OtherWorld,
        ServiceType_Max,
    }

    public enum ServiceStatus
    {
        Status_Empty = 0,

        Status_OpenUp,
        Status_OpenUp_Process,
        Status_OpenUp_OK,

        Status_Running,

        Status_ShutDown,
        Status_ShutDown_Process,
        Status_ShutDown_OK,

        Status_FinialSave,
        Status_FinialSave_Process,
        Status_FinialSave_OK,
    }

    public enum ServiceManagerStatus
    {
        Status_Empty = 0,

        Status_OpenUp,
        Status_Running,
        Status_ShutDown,
        Status_FinialSave,

    }

    public sealed class ServiceManager
    {
        Dictionary<ServiceType, AbstractService> m_DicService = new Dictionary<ServiceType, AbstractService>();
        List<AbstractService> m_lstService = new List<AbstractService>();
        ServiceManagerStatus m_eStatus = ServiceManagerStatus.Status_Empty;
        private List<Invoker> m_lstInvoker = new List<Invoker>();
        long m_lLastTikeTime = 0;
        static ServiceManager m_sInstance = new ServiceManager();



        public static ServiceManager Instance
        {
            get { return m_sInstance; }
        }

        public ServiceManagerStatus GetStatus()
        {
            return m_eStatus;
        }

        public void RegistService(AbstractService vService)
        {

            ServiceType typeService = vService.GetServiceId();
            if (typeService <= ServiceType.ServiceType_UnKnow || typeService >= ServiceType.ServiceType_Max)
            {
                return;
            }

            if (m_DicService.ContainsKey(typeService))
            {
                //LogSystem.AssertAndExit(false, "ServiceManager::RegistService  Duplicate ServiceType!!!  ServiceType = {0}", typeService.ToString());
            }
            m_DicService.Add(typeService, vService);
        }

        void Tick(long nElapse)
        {
            TickAllService(nElapse);
            TickAllInvoker(nElapse);
        }

        public void Run()
        {
            RunOpenUp();

            RunMainLogic();

            RunShutdown();

            RunFinalSave();
        }

        void RunOpenUp()
        {
            InitAllService();

            m_eStatus = ServiceManagerStatus.Status_OpenUp;
            SetAllServiceStatus(ServiceStatus.Status_OpenUp);

            long lLastTickTime = TimeManager.Instance.GetLocalMilliseconds();
            while(true)
            {
                long lCurrentTime = TimeManager.Instance.GetLocalMilliseconds();
                long lElapse = lCurrentTime - m_lLastTikeTime;
                m_lLastTikeTime = lCurrentTime;
                Tick(lElapse);

                if(IsAllServiceStatusEqual(ServiceStatus.Status_OpenUp_OK))
                {
                    break;
                }

                Thread.Sleep(10);
            }

        }

        void RunMainLogic()
        {
            m_eStatus = ServiceManagerStatus.Status_Running;
            SetAllServiceStatus(ServiceStatus.Status_Running);

            long lLastTickTime = TimeManager.Instance.GetLocalMilliseconds();
            long lShutdownCheckTime = 10000;

            while (true)
            {
                long lCurrentTime = TimeManager.Instance.GetLocalMilliseconds();
                long lElapse = lCurrentTime - lLastTickTime;
                lLastTickTime = TimeManager.Instance.GetLocalMilliseconds();
                Tick(lElapse);

                lShutdownCheckTime -= lElapse;
                if (lShutdownCheckTime <= 0)
                {
                    lShutdownCheckTime = 10000;
                    if(IsShouldShutdown())
                    {
                        break;
                    }
                }
                Thread.Sleep(10);

                if (DateTime.Now.Year > 2030)
                {
                    Thread.Sleep(5000);
                }
            } 
        }

        void RunShutdown()
        {
            m_eStatus = ServiceManagerStatus.Status_ShutDown;
            SetAllServiceStatus(ServiceStatus.Status_ShutDown);

            long lLastTickTime = TimeManager.Instance.GetLocalMilliseconds();
            while (true)
            {
                long lCurrentTime = TimeManager.Instance.GetLocalMilliseconds();
                long lElapse = lCurrentTime - lLastTickTime;
                lLastTickTime = lCurrentTime;
                Tick(lElapse);

                if(IsAllServiceStatusEqual(ServiceStatus.Status_ShutDown_OK))
                {
                    break;
                }

                Thread.Sleep(10);
            } 
        }

        void RunFinalSave()
        {
            m_eStatus = ServiceManagerStatus.Status_FinialSave;
            SetAllServiceStatus(ServiceStatus.Status_FinialSave);

            long lLastTickTime = TimeManager.Instance.GetLocalMilliseconds();
            while(true)
            {
                long lCurrentTime = TimeManager.Instance.GetLocalMilliseconds();
                long lElapse = lCurrentTime - lLastTickTime;
                lLastTickTime = lCurrentTime;
                Tick(lElapse);

                if(IsAllServiceStatusEqual(ServiceStatus.Status_FinialSave_OK))
                {
                    break;
                }

                Thread.Sleep(10);
            }
        }

        void InitAllService()
        {
            foreach (KeyValuePair<ServiceType, AbstractService> keyVal in m_DicService)
            {
                keyVal.Value.Init();
            }
        }
     

        void TickAllService(long nElapse)
        {
            Invoker vInvoker = null;

            foreach (KeyValuePair<ServiceType, AbstractService> keyVal in m_DicService)
            {
                for(int i = 0; i < 256; i++)
                {
                    vInvoker = keyVal.Value.PopInvoker();
                    if(null != vInvoker)
                    {
                        m_lstInvoker.Add(vInvoker);
                    }
                    else
                    {
                        break;
                    }
                }                
            }
        }

        void TickAllInvoker(long nElapse)
        {
            Invoker vInvoker = null;
            ServiceRunStatus eRunstatus = ServiceRunStatus.RunStatus_Normal;
            bool bOverTime = false;

            for(int i = 0; i < m_lstInvoker.Count; i++)
            {
                vInvoker = m_lstInvoker[i];
                switch(vInvoker.GetState())
                {
                    case Invoker.InvokerState.STATE_READY:
                        {
                            eRunstatus = vInvoker.GetRunStatus();
                            switch (eRunstatus)
                            {
                                case ServiceRunStatus.RunStatus_Normal:
                                case ServiceRunStatus.RunStatus_Slowly:
                                    {
                                        vInvoker.UpdateDoingInterval(nElapse);
                                        bOverTime = ((ServiceRunStatus.RunStatus_Normal == eRunstatus) ? vInvoker.IsOverNormalDoingInterval() : vInvoker.IsOverSlowDoingInterval());
                                        if(bOverTime)
                                        {
                                            vInvoker.SetState(Invoker.InvokerState.STATE_SCHEDULING);
                                            ThreadPool.QueueUserWorkItem(vInvoker.GetWaitCallback());
                                        }
                                    }
                                    break;
                                case ServiceRunStatus.RunStatus_Pause:
                                    break;
                            }
                        }
                        break;
                    case Invoker.InvokerState.STATE_SCHEDULING:
                        {
                            vInvoker.CheckScheduleState(nElapse);
                        }
                        break;
                    case Invoker.InvokerState.STATE_RUNNING:
                        {
                            vInvoker.CheckRunState(nElapse);
                        }
                        break;
                }
            }
        }

        void SetAllServiceStatus(ServiceStatus emStatus)
        {
            foreach (KeyValuePair<ServiceType, AbstractService> keyVal in m_DicService)
            {
                keyVal.Value.SetServiceStatus(emStatus);
            }
        }

        bool IsAllServiceStatusEqual(ServiceStatus emStatus)
        {
            foreach (KeyValuePair<ServiceType, AbstractService> keyVal in m_DicService)
            {
                if (keyVal.Value.GetServiceStatus() != emStatus)
                {
                    return false;
                }
            }
            return true;
        }

        bool IsShouldShutdown()
        {
            if(System.IO.File.Exists("shutdown.cmd"))
            {
                System.IO.File.Delete("shutdown.cmd");
                return true;
            }

            return false;
        }

        public void ReceiveTask(ServiceType eType,object vTask)
        {
            if(eType > ServiceType.ServiceType_UnKnow || eType < ServiceType.ServiceType_Max)
            {
                m_DicService[eType].ReceiveTask(vTask);
            }
        }

        public void ReceiveTask(AbstractService vService, object vTask)
        {
            vService.ReceiveTask(vTask);
        }
        /*
        public void SendTask2User(Guid64 guid,BaseTask vTask)
        {
            TransportToUserTask vTmpTask = new TransportToUserTask();
            vTmpTask.m_Guid = guid;
            vTmpTask.m_Task = vTask;
            ReceiveTask(ServiceType.ServiceType_Scene,vTmpTask);
        }
        */
    }
}
