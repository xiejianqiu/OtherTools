using UnityEngine;
#if !UNITY_EDITOR
    public class Debug
    {
        public static bool developerConsoleVisible;
        public static bool isDebugBuild;
        public static ILogger logger;
        public static void Assert(bool condition) { }
        public static void Assert(bool condition, string message) { }
        public static void Assert(bool condition, object message) { }
        public static void Assert(bool condition, Object context) { }
        public static void Assert(bool condition, string message, Object context) { }
        public static void Assert(bool condition, object message, Object context) { }
        public static void Assert(bool condition, string format, params object[] args) { }
        public static void AssertFormat(bool condition, string format, params object[] args) { }
        public static void AssertFormat(bool condition, Object context, string format, params object[] args) { }
        public static void Break() { }
        public static void ClearDeveloperConsole() { }
        public static void DebugBreak() { }
        public static void DrawLine(Vector3 start, Vector3 end) { }
        public static void DrawLine(Vector3 start, Vector3 end, Color color) { }
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration) { }
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest) { }
        public static void DrawRay(Vector3 start, Vector3 dir) { }
        public static void DrawRay(Vector3 start, Vector3 dir, Color color) { }
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration) { }
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration, bool depthTest) { }
        public static void Log(object message) { }
        public static void Log(object message, Object context) { }
        public static void LogAssertion(object message) { }
        public static void LogAssertion(object message, Object context) { }
        public static void LogAssertionFormat(string format, params object[] args) { }
        public static void LogAssertionFormat(Object context, string format, params object[] args) { }
        public static void LogError(object message) { }
        public static void LogError(object message, Object context) { }
        public static void LogErrorFormat(string format, params object[] args) { }
        public static void LogErrorFormat(Object context, string format, params object[] args) { }
        public static void LogException(System.Exception exception) { }
        public static void LogException(System.Exception exception, Object context) { }
        public static void LogFormat(string format, params object[] args) { }
        public static void LogFormat(Object context, string format, params object[] args) { }
        public static void LogWarning(object message) { }
        public static void LogWarning(object message, Object context) { }
        public static void LogWarningFormat(string format, params object[] args) { }
        public static void LogWarningFormat(Object context, string format, params object[] args) { }
    }
#endif