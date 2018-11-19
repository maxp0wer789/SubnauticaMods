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

        public static Config ModConfig;

        public static bool ModErrorOccurred;

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

            //create settings page
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene _loaded, LoadSceneMode _mode)
        {
            if (ModErrorOccurred) return;

            if(_loaded.name == "XMenu" || _loaded.name == "Essentials")
            {
                LoadModMenu();
            }
        }

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
                CyclopsStrafeMod.ModErrorOccurred = true;
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
