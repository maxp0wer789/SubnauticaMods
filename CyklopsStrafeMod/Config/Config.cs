using System.IO;
using UnityEngine;

namespace pp.SubnauticaMods.Strafe
{
    [System.Serializable]
    public class Config
    {
        public const string CFG_FILE_NAME = "config.json";

        public float StrafeSpeed            = 0.5f;

        public KeyCode StrafeModifierKey    = KeyCode.LeftShift;
        public bool UseModifier             = true;

        public KeyCode StrafeLeftKey        = KeyCode.A;
        public KeyCode StrafeRightKey       = KeyCode.D;

        public bool ThrottleLeft    => Input.GetKey(StrafeLeftKey);
        public bool ThrottleRight   => Input.GetKey(StrafeRightKey);
        public bool ModifierActive  => UseModifier && Input.GetKey(StrafeModifierKey);

        public static Config LoadConfig()
        {
            try
            {
                var targetFile = Path.Combine(CyclopsStrafeMod.ModPath, CFG_FILE_NAME);

                if (!File.Exists(targetFile))
                {
                    Util.LogW("Mod configuration could not be found. Creating...");
                    var cfg = new Config();
                    cfg.Save();
                    return cfg;
                }

                return JsonUtility.FromJson<Config>(File.ReadAllText(targetFile)) ?? throw new System.Exception("Failed to deserialize mod config");
            }
            catch (System.Exception _e)
            {
                Util.LogE("Error occurred while loading mod config: " + _e.Message);
                Util.LogE("\n" + _e.StackTrace);
                CyclopsStrafeMod.ModErrorOccurred = true;
                return null;
            }
        }

        public void Save()
        {
            try
            {
                var targetFile = Path.Combine(CyclopsStrafeMod.ModPath, CFG_FILE_NAME);
                var data = JsonUtility.ToJson(this, true);
                File.WriteAllText(targetFile, data);
            }
            catch (System.Exception _e)
            {
                Util.LogE("Error occurred while saving mod config: " + _e.Message);
                Util.LogE("\n" + _e.StackTrace);
                CyclopsStrafeMod.ModErrorOccurred = true;
            }
        }

    }
}
