using System.Collections.Generic;
using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    public class Console
    {
        private Vector2 m_consoleScroll;
        private List<ConsoleEntry> m_consoleEntries = new List<ConsoleEntry>();

        private ConsoleEntry[] m_drawEntries        = new ConsoleEntry[0];

        public void Start()
        {
            Application.logMessageReceived -= OnLogMessage;
            Application.logMessageReceived += OnLogMessage;
        }

        public void Destroy()
        {
            Application.logMessageReceived -= OnLogMessage;
        }

        public void Update()
        {
            m_drawEntries = m_consoleEntries.ToArray();
            if (DebugPanel.Get.PanelConfig.ConsoleAutoScroll)
                m_consoleScroll.y = float.MaxValue;
        }

        public void Draw(GUIStyle _consoleStyle)
        {
            GUILayout.BeginVertical(_consoleStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                m_consoleScroll = GUILayout.BeginScrollView(m_consoleScroll);
                foreach(var entry in m_drawEntries)
                {
                    _consoleStyle.normal.textColor = Color.white;
                    switch (entry.Type)
                    {
                        case LogType.Warning:
                            _consoleStyle.normal.textColor = Color.yellow;
                            break;
                        case LogType.Error:
                        case LogType.Exception:
                            _consoleStyle.normal.textColor = Color.red;
                            break;
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5f);
                    GUILayout.TextField($"{(DebugPanel.Get.PanelConfig.ConsoleShowType ? $"[{entry.Type}]" : "")}" +
                        $"{(DebugPanel.Get.PanelConfig.ConsoleShowTime ? $"[{entry.Time.ToString(DebugPanel.Get.PanelConfig.ConsoleTimeFormat)}]" : "")}" +
                        $" {entry.Message}", _consoleStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Space(5f);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public void Clear()
        {
            m_consoleEntries.Clear();
        }

        private void OnLogMessage(string _condition, string _stackTrace, LogType _type)
        {
            if (m_consoleEntries.Count >= DebugPanel.Get.PanelConfig.ConsoleMaxEntries)
            {
                m_consoleEntries.RemoveAt(0);
            }

            m_consoleEntries.Add(new ConsoleEntry(_type, _condition, _stackTrace));
        }

        private class ConsoleEntry
        {
            public LogType Type             { get; private set; }
            public string Message           { get; private set; }
            public string Stacktrace        { get; private set; }
            public bool DisplayStacktrace   { get; set; }
            public System.DateTime Time     { get; private set; }

            public ConsoleEntry(LogType _type, string _message, string _stackTrace)
            {
                Time        = System.DateTime.Now;
                Type        = _type;
                Message     = _message;
                Stacktrace  = _stackTrace;
            }
        }
    }
}