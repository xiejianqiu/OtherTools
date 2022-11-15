using System;
//using System.Diagnostics;

namespace Shark
{
    /**
     * @brief 日志类型
     */
    public enum Log_Type
    {
        LT_Debug,
        LT_Info,
        LT_Warn,
        LT_Error,
        LT_Assert,
    }
    public delegate void LogSystemOutputDelegation(string msg);
    /**
     * @brief 日志系统
     */
    internal class LogSystem
    {
        public static LogSystemOutputDelegation OnOutput;
        public static void Debug(string format, params object[] args)
        {
            string str = string.Format("[Debug]:" + format, args);
            Output(Log_Type.LT_Debug, str);
        }
        public static void Info(string format, params object[] args)
        {
            string str = string.Format("[Info]:" + format, args);
            Output(Log_Type.LT_Info, str);
        }
        public static void Warn(string format, params object[] args)
        {
            string str = string.Format("[Warn]:" + format, args);
            Output(Log_Type.LT_Warn, str);
        }
        public static void Error(string format, params object[] args)
        {
            string str = string.Format("[Error]:" + format, args);
            Output(Log_Type.LT_Error, str);
        }
        public static void Assert(bool check, string format, params object[] args)
        {
            if (!check)
            {
                string str = string.Format("[Assert]:" + format, args);
                Output(Log_Type.LT_Assert, str);
            }
        }

        public static void AssertAndExit(bool check, string format, params object[] args)
        {
            if (!check)
            {
                string str = string.Format("[Assert]:" + format, args);
                Output(Log_Type.LT_Assert, str);
            }
        }
        private static void Output(Log_Type type, string msg)
        {
            if (OnOutput!=null)
            {
                OnOutput(msg);
            }
        }
    }
    public class LogForCommonProj
    {
        public static void SetOutput(LogSystemOutputDelegation del)
        {
            LogSystem.OnOutput = del;
        }
    }
}
