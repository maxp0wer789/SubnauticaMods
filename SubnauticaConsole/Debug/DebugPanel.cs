using UnityEngine;

namespace pp.SubnauticaMods.dbg
{
    public class DebugPanel : MonoBehaviour
    {
        internal static DebugPanel CreateNew() => new GameObject("_debug_").AddComponent<DebugPanel>();
        public static DebugPanel Get = null;

        public const float PANEL_PADDING_TOP    = 5f;
        public const float PANEL_PADDING_LEFT   = 5f;
        public const float OPT_BUTTON_HEIGHT    = 25f;
        public const float OPT_BUTTON_WIDTH     = 25f;

        public const float DEFAULT_TXT_FIELD_WIDTH = 35f;
        public const float SETTINGS_HEIGHT = 25f;
        public const float SETTINGS_OFFSET_BOTTOM = 5f;

        public const float PANEL_MIN_WIDTH = 900f;
        public const float PANEL_MIN_HEIGHT = 50f;

        public Config PanelConfig => m_config;

        public Console ConsoleDrawer => m_consoleDrawer;
        public Browser BrowserDrawer => m_browserDrawer;

        internal Vector2 Position
        {
            get
            {
                return m_panelPosition;
            }
            set
            {
                m_panelPosition = new Vector2(
                    Mathf.Round(Mathf.Clamp(value.x, 0, Screen.width)),
                    Mathf.Round(Mathf.Clamp(value.y, 0, Screen.height)));
            }
        }

        internal Vector2 Size
        {
            get
            {
                return m_panelSize;
            }
            set
            {
                m_panelSize = new Vector2(
                    Mathf.Round(Mathf.Max(value.x, PANEL_MIN_WIDTH)),
                    Mathf.Round(Mathf.Max(value.y, PANEL_MIN_HEIGHT)));
            }
        }

        private bool m_showConsole          = true;
        private bool m_showGOBrowser        = false;
        private bool m_showSettings         = false;

        private Vector2 m_panelPosition = Vector2.one * 55f;
        private Vector2 m_panelSize     = new Vector2(Screen.width * 0.5f, Screen.height * 0.75f);

        private Console m_consoleDrawer = new Console();
        private Browser m_browserDrawer = new Browser();

        private GUIStyle m_consoleStyle     = new GUIStyle(); 
        private GUIStyle m_panelStyle       = new GUIStyle();
        private GUIStyle m_browserStyle     = new GUIStyle();
        private GUIStyle m_settingsStyle    = new GUIStyle();

        private Texture2D m_consoleBackgroundColor;
        private Texture2D m_browserBackgroundColor;
        private Texture2D m_panelBackgroundTexture;

        private Vector2 m_oldPanelPos;
        private Vector2 m_oldPanelSize;

        private bool m_changePanelPosition;
        private bool m_changePanelSize;

        private Config m_config;

        private bool m_errorOccurred;

        public void ToggleVisibility()
        {
            m_config.ConsoleVisible = !m_config.ConsoleVisible;

            if(!m_config.ConsoleVisible)
            {
                m_changePanelSize       = false;
                m_changePanelPosition   = false;
            }
        }

        #region ENGINE_CALLBACKS
        private void Awake()
        {
            try
            {
                if (Get != null)
                {
                    Util.LogW("There is already another active instance of GOBrowser.");
                    Destroy(gameObject);
                    return;
                }

                Get = this;
                DontDestroyOnLoad(this);

                m_config = Config.Load();

                Position = m_config.Position;
                Size = m_config.Size;

                m_panelBackgroundTexture = Util.CreateTextureFromColor(Color.gray * 1.2f);
                m_consoleBackgroundColor = Util.CreateTextureFromColor(Color.black);
                m_browserBackgroundColor = Util.CreateTextureFromColor(Color.gray);

                m_panelStyle.normal.background      = m_panelBackgroundTexture;
                m_consoleStyle.normal.background    = m_consoleBackgroundColor;
                m_settingsStyle.normal.background   = m_browserBackgroundColor;
                m_browserStyle.normal.background    = m_browserBackgroundColor;

                m_consoleStyle.fontSize = m_config.ConsoleFontSize;

                m_panelStyle.normal.textColor = Color.black;

                m_settingsStyle.fontSize = 16;
                m_settingsStyle.normal.textColor = Color.black;
                m_settingsStyle.alignment = TextAnchor.MiddleLeft;

                m_consoleDrawer.Start();
                m_browserDrawer.Start();
            }
            catch (System.Exception _e)
            {
                Util.LogE("Startup error occurred: " + _e.Message);
                Util.LogE(_e.StackTrace);
                m_errorOccurred = true;
            }
        }

        private void OnDestroy()
        {
            try
            {
                m_config.Position = Position;
                m_config.Size = Size;
                m_config?.Save();

                if (m_panelBackgroundTexture != null)
                    Destroy(m_panelBackgroundTexture);
                m_consoleDrawer.Destroy();
                Get = null;
            }
            catch (System.Exception _e)
            {
                Util.LogE("Shutdown error occurred: " + _e.Message);
                Util.LogE(_e.StackTrace);
                m_errorOccurred = true;
            }
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F4))
            {
                ToggleVisibility();
            }

            if (m_errorOccurred || !m_config.ConsoleVisible) return;

            m_consoleDrawer?.Update();
        }

        private void OnGUI()
        {
            if (!m_config.ConsoleVisible) return;

            GUILayout.Label("Size " + Size);
            GUILayout.Label("Position " + Position);

            if (m_errorOccurred)
            {
                m_panelStyle.normal.textColor = Color.red;
                GUI.Box(new Rect(70f, 50f, 275f, 50f), "The console encountered an unexpected error!\nCheck the log files for further information.\nPress F6 to hide this message.", m_panelStyle);
                return;
            }

            try
            {
                GUI.Box(new Rect(m_panelPosition.x, m_panelPosition.y, m_panelSize.x, m_panelSize.y), "", m_panelStyle);
                GUILayout.BeginArea(new Rect(m_panelPosition.x, m_panelPosition.y, m_panelSize.x, m_panelSize.y));
                    GUILayout.BeginHorizontal(GUILayout.Width(m_panelSize.x));
                    GUILayout.Space(PANEL_PADDING_LEFT);
                        GUILayout.BeginVertical(GUILayout.Height(m_panelSize.y));
                            GUILayout.Space(PANEL_PADDING_TOP);

                            GUILayout.BeginHorizontal();
                                m_changePanelPosition = GUILayout.Toggle(m_changePanelPosition, new GUIContent("", "Change panel position"), GUILayout.Width(OPT_BUTTON_WIDTH), GUILayout.Width(OPT_BUTTON_HEIGHT));
                                if (m_changePanelPosition)
                                {
                                    Reposition();
                                }

                                var pnlSize = GUILayout.Toggle(m_changePanelSize, new GUIContent("", "Change panel size"), GUILayout.Width(OPT_BUTTON_WIDTH), GUILayout.Width(OPT_BUTTON_HEIGHT));
                                if (pnlSize != m_changePanelSize)
                                {
                                    m_oldPanelSize = Size;
                                    m_oldPanelPos = Position;
                                    m_changePanelSize = pnlSize;
                                }

                                if (m_changePanelSize)
                                {
                                    Resize();
                                }
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button($"{(m_showSettings ? "Hide" : "Show")} settings"))
                                {
                                    m_showSettings = !m_showSettings;
                                }
                            GUILayout.EndHorizontal();

                            if (m_showSettings)
                            {
                                DrawSettings();
                                GUILayout.Space(SETTINGS_OFFSET_BOTTOM);
                            }
                            if (m_showConsole)
                            {
                                m_consoleDrawer.Draw(m_consoleStyle);
                            }

                            GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button($"{(m_showGOBrowser ? "Hide" : "Show")} browser"))
                                {
                                    m_showGOBrowser = !m_showGOBrowser;
                                }
                            GUILayout.EndHorizontal();

                            if (m_showGOBrowser)
                            {
                                m_browserDrawer.Draw(m_browserStyle);
                            }

                            GUILayout.Space(PANEL_PADDING_TOP);
                        GUILayout.EndVertical();
                    GUILayout.Space(PANEL_PADDING_LEFT);
                    GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            catch (System.Exception _e)
            {
                Util.LogE("UI render error occurred: " + _e.Message);
                Util.LogE(_e.StackTrace);
                m_errorOccurred = true;
            }
        }
        #endregion

        private void Resize()
        {
            var actualY = Screen.height - Input.mousePosition.y;
            var pos = new Vector2(
                            Input.mousePosition.x - OPT_BUTTON_WIDTH * 1.65f - PANEL_PADDING_LEFT,
                            actualY - PANEL_PADDING_TOP - OPT_BUTTON_HEIGHT * 0.5f);
            Position = new Vector2(Mathf.Clamp(pos.x, 0f, Screen.width - Size.x), Mathf.Clamp(pos.y, 0f, Screen.height - Size.y));

            Size = m_oldPanelSize - (m_panelPosition - m_oldPanelPos);
        }

        private void Reposition()
        {
            var actualY = Screen.height - Input.mousePosition.y;
            var pos = new Vector2(
                Input.mousePosition.x - OPT_BUTTON_WIDTH * 0.5f - PANEL_PADDING_LEFT,
                actualY - PANEL_PADDING_TOP - OPT_BUTTON_HEIGHT * 0.5f);
            Position = new Vector2(Mathf.Clamp(pos.x, 0f, Screen.width - Size.x), Mathf.Clamp(pos.y, 0f, Screen.height - Size.y));
        }

        private void DrawSettings()
        {
            GUILayout.BeginHorizontal(m_settingsStyle, GUILayout.Height(SETTINGS_HEIGHT));
            try
            {
                var val = string.IsNullOrEmpty(m_config.ConsoleFontSize.ToString()) ? "0" : m_config.ConsoleFontSize.ToString();
                var fontSize = int.Parse(GUILayout.TextField(val, GUILayout.Width(DEFAULT_TXT_FIELD_WIDTH), GUILayout.ExpandHeight(true)));
                if (fontSize != m_config.ConsoleFontSize)
                {
                    m_consoleStyle.fontSize = fontSize;
                    m_config.ConsoleFontSize = fontSize;
                }
            }
            catch { }
            GUILayout.Label("Browser height");
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Clear console"))
            {
                m_consoleDrawer.Clear();
            }
            try
            {
                var val         = string.IsNullOrEmpty(m_config.ConsoleFontSize.ToString()) ? "0" : m_config.ConsoleFontSize.ToString();
                var fontSize    = int.Parse(GUILayout.TextField(val, GUILayout.Width(DEFAULT_TXT_FIELD_WIDTH), GUILayout.ExpandHeight(true)));
                if(fontSize != m_config.ConsoleFontSize)
                {
                    m_consoleStyle.fontSize = fontSize;
                    m_config.ConsoleFontSize = fontSize;
                }
            }
            catch {}
            GUILayout.Label("Font size");
            m_config.ConsoleAutoScroll  = GUILayout.Toggle(m_config.ConsoleAutoScroll, $"Auto scroll", GUILayout.ExpandHeight(true));
            GUILayout.Space(10f);
            m_config.ConsoleShowType    = GUILayout.Toggle(m_config.ConsoleShowType, $"Show type", GUILayout.ExpandHeight(true));
            m_config.ConsoleShowTime    = GUILayout.Toggle(m_config.ConsoleShowTime, $"Show time",  GUILayout.ExpandHeight(true));
            GUILayout.Space(10f);
            m_config.BrowserShowValueChangeButtons = GUILayout.Toggle(m_config.BrowserShowValueChangeButtons, $"Show value buttons", GUILayout.ExpandHeight(true));
            GUILayout.Space(10f);
            m_config.ConsoleShowType    = GUILayout.Toggle(m_config.ConsoleShowType, $"Info", GUILayout.ExpandHeight(true));
            m_config.ConsoleShowTime    = GUILayout.Toggle(m_config.ConsoleShowTime, $"Warning", GUILayout.ExpandHeight(true));
            m_config.ConsoleAutoScroll  = GUILayout.Toggle(m_config.ConsoleAutoScroll, $"Error", GUILayout.ExpandHeight(true));

            GUILayout.EndHorizontal();
        }
    }
}
