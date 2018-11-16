using plib.Util;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    public class SubnauticaDebug
    {
        public static string ModPath    = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static bool Loaded       = false;

        public static void Initialize()
        {
            //need early entry point. QModManager injects into GameInput.Awake
            var patch = new SubnauticaDebug();
            patch.Load();
        }

        private static DebugPanel m_goBrowser;

        private void Load()
        {
            if (Loaded)
            {
                Debug.LogWarning("[Console] Already patched. Doing nothing.");
                return;
            }

            if (!LoadAdditionalAssembly("plib_util.dll")) return;

            L.CREATE_LOG_FILES = false;
            L.APP_NAME = "subnautica_debug";
            L.SetLogMethods(Debug.LogError, new System.Collections.Generic.Dictionary<ELogType, System.Action<string>>()
            {
                { ELogType.INFO,    Debug.Log           },
                { ELogType.DEBUG,   Debug.Log           },
                { ELogType.WARNING, Debug.LogWarning    },
                { ELogType.ERROR,   Debug.LogError      }
            });

            m_goBrowser = DebugPanel.CreateNew();
            L.Log("Subnautica console intialized!");
            Loaded = true;
        }

        private bool LoadAdditionalAssembly(string _name)
        {
            var path = Path.Combine(ModPath, _name);
            try
            {
                var util = Assembly.LoadFile(path) ?? throw new System.Exception("Failed to load assembly."); //load plib util dependency from mod directory
            }
            catch (System.Exception _e)
            {
                Debug.LogWarning($"[Console] Failed to load dependency library({path}): {_e.Message}");
                return false;
            }
            return true;
        }
    }
}
