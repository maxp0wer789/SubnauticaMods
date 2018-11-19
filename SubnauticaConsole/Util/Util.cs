using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    public static class Util
    {
        public static string GetHierarchyPath(this GameObject _source)
        {
            string path = "";
            Transform target = _source.transform;
            for (int i = 0; i < _source.transform.hierarchyCount; ++i)
            {
                path = target.name + (i <= 0 ? "" : "/") + path;
                target = target.parent;
                if (!target)
                    break;
            }
            return path;
        }

        public static Texture2D CreateTextureFromColor(Color _color)
        {
            var tx = new Texture2D(1, 1, TextureFormat.RGB24, false);
            tx.SetPixel(0, 0, _color);
            tx.Apply();
            return tx;
        }

        public static void Log(string _message)
        {
            Debug.Log($"[SubnauticaDebug] {_message}");
        }

        public static void LogW(string _message)
        {
            Debug.LogWarning($"[SubnauticaDebug] {_message}");
        }

        public static void LogE(string _message)
        {
            Debug.LogError($"[SubnauticaDebug] {_message}");
        }
    }
}