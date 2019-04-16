using UnityEngine;

namespace pp.SubnauticaMods.Strafe
{
    public static class Util
    {
        public const string LOG_SOURCE = "[CyclopsStrafe]";

        public static void Log(object _message)
        {
            Debug.Log($"{LOG_SOURCE} {_message.ToString()}");
        }

        public static void LogW(object _message)
        {
            Debug.LogWarning($"{LOG_SOURCE} {_message.ToString()}");
        }

        public static void LogE(object _message)
        {
            Debug.LogError($"{LOG_SOURCE} {_message.ToString()}");
        }
    }
}