using Harmony;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pp.SubnauticaMods.Strafe
{
    /// <summary>
    /// Main mod module. Loading the menu. Patching assemblies.
    /// </summary>
    public class CyclopsStrafeMod
    {
        public static string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public readonly static CyclopsStrafeMod Get = new CyclopsStrafeMod();

        private string SMLPluginPath => Path.Combine(Path.Combine(ModPath, "SML"), "cyclops_strafe_sml_plugin.dll");

        public static Config ModConfig;

        public static bool ModErrorOccurred;

        private Assembly m_smlPlugin;

        public static void Initialize()
        {
            var harmony = HarmonyInstance.Create("de.petesplace.subnauticamods.cstrafe");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Get.Hook();
        }

        public void Hook()
        {
            if (ModErrorOccurred) return;

            ModConfig = Config.LoadConfig();
            if (ModConfig == null) return;

            SceneManager.sceneLoaded -= OnSceneLoaded; //make sure the method is only added once to the event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene _loaded, LoadSceneMode _mode)
        {
            //plugin error occurred? sml plugin already loaded? incorrect scene loaded? do nothing.
            if (ModErrorOccurred || m_smlPlugin != null || (_loaded.name != "XMenu" && _loaded.name != "Essentials")) return;

            //try to find the smlhelper assembly in the current app domain
            var ass = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(_o => _o.FullName.StartsWith("SMLHelper"));
            //if smlhelper found
            if (ass != null)
            {
                //create settings page with smlhelper and register cyclops upgrades
                LoadSMLPlugin();
                return;
            }
            //otherwise create settings page the legacy way
            LoadModMenu();
        }

        //Only called if sml is available. Loads the sml plugin for cyclops upgrade item and helper options usage
        private void LoadSMLPlugin()
        {
            try
            {
                if (!File.Exists(SMLPluginPath)) throw new System.Exception("Could not find sml plugin dll. Please reinstall the mod.");
                m_smlPlugin = Assembly.LoadFrom(SMLPluginPath) ?? throw new System.Exception("Failed to load sml plugin assembly!");
                var smlPluginEntryType      = m_smlPlugin.GetExportedTypes().FirstOrDefault(_o => _o.FullName == "pp.SubnauticaMods.Strafe.SML.SMLPluginLoader") ?? throw new System.Exception("Invalid sml plugin layout. Reinstall the mod.");
                var smlPluginEntryMethod    = smlPluginEntryType.GetMethod("LoadSMLPlugin") ?? throw new System.Exception("Invalid sml plugin layout. Reinstall the mod.");
                smlPluginEntryMethod.Invoke(null, null);
            }
            catch (System.Exception _e)
            {
                Util.LogE("Error occurred while loading sml plugin: " + _e.Message);
                Util.LogE("\n" + _e.StackTrace);
                ModErrorOccurred = true;
            }
}

        //legacy mod options loader
        private void LoadModMenu()
        {
            try
            {
                uGUI_OptionsPanel optPanel = FindAllOfType<uGUI_OptionsPanel>().FirstOrDefault() ?? throw new System.Exception("Failed to retrieve options panel."); //get subnautica options panel
                if (!optPanel)
                {
                    return;
                }
                var menu = new GameObject("cyclops_strafe_menu_control").AddComponent<UIModMenu>();
                menu.transform.SetParent(optPanel.transform);
                menu.Load(optPanel);
            }
            catch (System.Exception _e)
            {
                Util.LogE("Error occurred while loading mod UI: " + _e.Message);
                Util.LogE("\n" + _e.StackTrace);
                ModErrorOccurred = true;
            }
        }

        public static T[] FindAllOfType<T>() where T : Object //need this method to find inactive ui components. FindObjectOfType does not return inactive components.
        {
            var scenes = new Scene[SceneManager.sceneCount];
            for(int i = 0; i < SceneManager.sceneCount; ++i)
            {
                scenes[i] = SceneManager.GetSceneAt(i);
            }
            var objects = scenes.Where(_o => _o.isLoaded).SelectMany(_o => _o.GetRootGameObjects()).ToList();
            return objects.SelectMany(_o => _o.GetComponentsInChildren<T>(true)).ToArray();
        }
    }
}
