using System;
using UnityEngine;

namespace Module.Log
{
	internal class LogModule
	{
		private enum LOG_TYPE
		{
			DEGUG_LOG,
			WARNING_LOG,
			ERROR_LOG
		}

		public delegate void OnOutputLog(string _msg);

		public static LogModule.OnOutputLog onOutputLog;

		private static void WriteLog(string msg, LogModule.LOG_TYPE type, bool _showInConsole = false)
		{
			switch (type)
			{
			case LogModule.LOG_TYPE.DEGUG_LOG:
				Debug.Log(msg);
				break;
			case LogModule.LOG_TYPE.WARNING_LOG:
				Debug.LogWarning(msg);
				break;
			case LogModule.LOG_TYPE.ERROR_LOG:
				Debug.LogError(msg);
				break;
			}
		}

		public static void ErrorLog(string fort, params object[] areges)
		{
			if (areges.Length > 0)
			{
				string msg = string.Format(fort, areges);
				LogModule.WriteLog(msg, LogModule.LOG_TYPE.ERROR_LOG, true);
			}
			else
			{
				LogModule.WriteLog(fort, LogModule.LOG_TYPE.ERROR_LOG, true);
			}
		}

		public static void WarningLog(string fort, params object[] areges)
		{
			if (areges.Length > 0)
			{
				string msg = string.Format(fort, areges);
				LogModule.WriteLog(msg, LogModule.LOG_TYPE.WARNING_LOG, true);
			}
			else
			{
				LogModule.WriteLog(fort, LogModule.LOG_TYPE.WARNING_LOG, true);
			}
		}

		public static void DebugLog(string fort, params object[] areges)
		{
			if (areges.Length > 0)
			{
				string msg = string.Format(fort, areges);
				LogModule.WriteLog(msg, LogModule.LOG_TYPE.DEGUG_LOG, true);
			}
			else
			{
				LogModule.WriteLog(fort, LogModule.LOG_TYPE.DEGUG_LOG, true);
			}
		}

		private static void ErrorLog(string msg)
		{
			LogModule.WriteLog(msg, LogModule.LOG_TYPE.ERROR_LOG, false);
		}

		private static void WarningLog(string msg)
		{
			LogModule.WriteLog(msg, LogModule.LOG_TYPE.WARNING_LOG, false);
		}

		public static void DebugLog(string msg)
		{
			LogModule.WriteLog(msg, LogModule.LOG_TYPE.DEGUG_LOG, false);
		}

		public static void Log(string logString, string stackTrace, LogType type)
		{
			switch (type)
			{
			case LogType.Error:
				LogModule.ErrorLog(logString);
				break;
			case LogType.Warning:
				LogModule.WarningLog(logString);
				break;
			case LogType.Log:
				LogModule.DebugLog(logString);
				break;
			}
		}

		public static string ByteToString(byte[] byteData, int nStartIndex, int nCount)
		{
			string text = string.Empty;
			if (nStartIndex < 0 || nStartIndex >= byteData.Length)
			{
				return text;
			}
			int num = nStartIndex;
			while (num < nCount && num < byteData.Length)
			{
				text += Convert.ToString(byteData[num]);
				num++;
			}
			return text;
		}
	}
}
