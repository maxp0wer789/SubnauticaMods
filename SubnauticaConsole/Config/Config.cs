using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    [System.Serializable]
    public class Config
    {
        public const string CONFIG_FILE_NAME = "config.xml";
        public static string ConfigFilePath => Path.Combine(SubnauticaDebug.ModPath, CONFIG_FILE_NAME);

        public SerializableVector Position  = new SerializableVector(50f, 50f);
        public SerializableVector Size      = new SerializableVector(Screen.width * 0.5f, Screen.height * 0.5f);

        public bool ConsoleVisible      = false;
        public bool ConsoleShowType     = true;
        public bool ConsoleShowTime     = true;
        public string ConsoleTimeFormat = "T";
        public int ConsoleFontSize      = 12;
        public bool ConsoleAutoScroll   = true;
        public int ConsoleMaxEntries    = 300;

        public bool BrowserShowValueChangeButtons = true;
        public float BrowserValueButtonChangeStep = 0.01f;

        public bool BrowserInspectorDebug = false;

        public void Save()
        {
            try
            {
                using (var stream = File.Open(ConfigFilePath, FileMode.Create))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(Config));
                    xSer.Serialize(stream, this);
                }
            }
            catch (System.Exception _e)
            {
                Util.LogE($"Failed to save debug panel config: {_e.Message}");
            }
        }

        public static Config Load()
        {
            if(!File.Exists(ConfigFilePath))
            {
                Util.LogW("Config file does not exist. Creating new config.");
                return new Config();
            }

            try
            {
                using (var stream = File.Open(ConfigFilePath, FileMode.Open))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(Config));
                    return xSer.Deserialize(stream) as Config ?? throw new System.Exception("Failed to deserialize config data.");
                }
            }
            catch (System.Exception _e)
            {
                Util.LogE($"Failed to load debug panel config: {_e.Message}");
                return new Config();
            }
        }
    }
}
