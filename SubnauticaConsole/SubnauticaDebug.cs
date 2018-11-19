using System.IO;
using System.Reflection;

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
                Util.LogW("[Console] Already patched. Doing nothing.");
                return;
            }

            m_goBrowser = DebugPanel.CreateNew();
            Util.Log("Subnautica console intialized!");
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
                Util.LogW($"Failed to load dependency library({path}): {_e.Message}");
                return false;
            }
            return true;
        }
    }
}
