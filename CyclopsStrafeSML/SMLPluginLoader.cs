using SMLHelper.V2.Handlers;
using UnityEngine;

namespace pp.SubnauticaMods.Strafe.SML
{
    public class SMLPluginLoader
    {
        private StrafeUpgrade m_strafeUpgrade = new StrafeUpgrade();

        private void Load()
        {
            try
            {
                Debug.Log("Patching sml mod additions...");
                OptionsPanelHandler.RegisterModOptions(new SMLCyclopsStrafeOptions());
                m_strafeUpgrade.Patch();
            }
            catch (System.Exception _e)
            {
                Debug.LogError("Error occurred in sml plugin patcher (CyclopsStrafe): " + _e.Message);
                Debug.LogError(_e.StackTrace);
            }
        }

        public static void LoadSMLPlugin()
        {
            var loader = new SMLPluginLoader();
            loader.Load();
        }
    }
}
