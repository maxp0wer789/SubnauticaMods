using System.Collections.Generic;
using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    public class Console
    {
        public const int MAX_CONSOLE_ENTRIES        = 200;

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

        public void Draw(GUIStyle _consoleStyle)
        {
            GUILayout.BeginVertical(_consoleStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                m_consoleScroll = GUILayout.BeginScrollView(m_consoleScroll);
                m_consoleEntries.ForEach(_o =>
                {
                    _consoleStyle.normal.textColor = Color.white;
                    switch (_o.Type)
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
                    GUILayout.TextField($"{(DebugPanel.Get.PanelConfig.ConsoleShowType ? $"[{_o.Type}]" : "")}" +
                        $"{(DebugPanel.Get.PanelConfig.ConsoleShowTime ? $"[{_o.Time.ToString(DebugPanel.Get.PanelConfig.ConsoleTimeFormat)}]" : "")}" +
                        $" {_o.Message}", _consoleStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Space(5f);
                    GUILayout.EndHorizontal();
                });
                GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void OnLogMessage(string _condition, string _stackTrace, LogType _type)
        {
            if (m_consoleEntries.Count >= MAX_CONSOLE_ENTRIES)
            {
                m_consoleEntries.RemoveAt(0);
            }

            m_consoleEntries.Add(new ConsoleEntry(_type, _condition, _stackTrace));

            if (DebugPanel.Get.PanelConfig.ConsoleAutoScroll)
                m_consoleScroll.y = float.MaxValue;
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