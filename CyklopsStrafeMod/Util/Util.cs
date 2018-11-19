using UnityEngine;

namespace pp.SubnauticaMods.Strafe
{
    public static class Util
    {
        public const string LOG_SOURCE = "[CyclopsStrafe]";

        public static void Log(string _message)
        {
            Debug.Log($"{LOG_SOURCE} {_message}");
        }

        public static void LogW(string _message)
        {
            Debug.LogWarning($"{LOG_SOURCE} {_message}");
        }

        public static void LogE(string _message)
        {
            Debug.LogError($"{LOG_SOURCE} {_message}");
        }
    }
}