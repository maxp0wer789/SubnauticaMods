using plib.Util;
using pp.subnauticamods.dbg.Util;
using pp.SubnauticaMods.dbg;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace pp.subnauticamods.dbg.Config
{
    [System.Serializable]
    public class Config
    {
        public const string CONFIG_FILE_NAME = "config.xml";
        public static string ConfigFilePath => Path.Combine(SubnauticaDebug.ModPath, CONFIG_FILE_NAME);

        public SerializableVector Position  = new SerializableVector(50f, 50f);
        public SerializableVector Size      = new SerializableVector(Screen.width * 0.5f, Screen.height * 0.5f);

        public bool ConsoleShowType     = true;
        public bool ConsoleShowTime     = true;
        public string ConsoleTimeFormat = "T";
        public int ConsoleFontSize      = 12;
        public bool ConsoleAutoScroll   = true;

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
                L.LogE("Failed to save debug panel config: " + _e.Message);
            }
        }

        public static Config Load()
        {
            if(!File.Exists(ConfigFilePath))
            {
                L.LogW("Config file does not exist. Creating new config.", "Subnautica Debug");
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
                L.LogE("Failed to load debug panel config: " + _e.Message);
                return new Config();
            }
        }
    }
}
