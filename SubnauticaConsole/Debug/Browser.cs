using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pp.SubnauticaMods.dbg
{
    public class Browser
    {
        private static Dictionary<System.Type, ITypeDrawer> s_typeDrawers = new Dictionary<System.Type, ITypeDrawer>();

        private Scene[] m_activeScenes;

        private GameObject m_selectedGameObject;

        private Vector2 m_gameObjectScroll  = Vector2.zero;
        private Vector2 m_componentScroll   = Vector2.zero;

        private Dictionary<GameObject, bool> m_objectFoldState      = new Dictionary<GameObject, bool>();
        private Dictionary<Component, bool> m_componentFoldStates   = new Dictionary<Component, bool>();

        private GameObject[] m_rootObjects;

        public static void RegisterDrawer(System.Type _type, ITypeDrawer _drawer)
        {
            if(s_typeDrawers.ContainsKey(_type))
            {
                Util.LogW("Failed to register type drawer. Type \"" + _type.Name + "\" is already registered.");
                return;
            }
            s_typeDrawers.Add(_type, _drawer);
        }

        #region ENGINE_CALLBACKS
        public void Start()
        {
            SceneManager.sceneLoaded -= OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnActiveSceneChanged;

            RegisterDrawer(typeof(bool),        new BoolDrawer());
            RegisterDrawer(typeof(short),       new ShortDrawer());
            RegisterDrawer(typeof(double),      new DoubleDrawer());
            RegisterDrawer(typeof(float),       new FloatDrawer());
            RegisterDrawer(typeof(long),        new LongDrawer());
            RegisterDrawer(typeof(int),         new IntDrawer());
            RegisterDrawer(typeof(string),      new StringDrawer());
            RegisterDrawer(typeof(Vector3),     new Vector3Drawer());
            RegisterDrawer(typeof(Vector2),     new Vector2Drawer());
            RegisterDrawer(typeof(Quaternion),  new QuaternionDrawer());
            RegisterDrawer(typeof(GameObject),  new GameObjectDrawer());
            RegisterDrawer(typeof(Color),       new ColorDrawer());
            RegisterDrawer(typeof(Object),      new ObjectDrawer());
        }

        public void Draw(GUIStyle _browserStyle)
        {
            GUILayout.BeginVertical(_browserStyle, GUILayout.MinHeight(DebugPanel.Get.Size.y * 0.5f));

            if (m_rootObjects != null && m_rootObjects.Length > 0)
            {
                DrawObjectTree();
            }
            else GUILayout.Label("No GameObjects in scene.");

            GUILayout.EndVertical();
        }

        private void OnActiveSceneChanged(Scene _scene, LoadSceneMode _mode)
        {
            ReloadScenes();
        }
        #endregion

        private void ReloadScenes()
        {
            ResetSelection();

            var scenes = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                scenes.Add(SceneManager.GetSceneAt(i));
            }

            m_activeScenes  = scenes.ToArray();
            m_rootObjects   = m_activeScenes.SelectMany(_o => _o.GetRootGameObjects()).ToArray();
        }

        private void ResetSelection()
        {
            m_objectFoldState.Clear();
            m_rootObjects           = new GameObject[0];
            m_selectedGameObject    = null;
        }

        private void DrawObjectTree()
        {
            GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(GUILayout.MinWidth((DebugPanel.Get.Size.x - DebugPanel.PANEL_PADDING_LEFT * 2f) * 0.6f));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"### Hierarchy ({m_activeScenes?.Length ?? 0} scene(s) loaded) ###", GUILayout.ExpandWidth(true));
                    GUILayout.Label(string.Join(", ", m_activeScenes.Select(_o => _o.name).ToArray()));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Refresh"))
                    {
                        ReloadScenes();
                    }
                    GUILayout.EndHorizontal();

                    m_gameObjectScroll = GUILayout.BeginScrollView(m_gameObjectScroll, GUILayout.ExpandWidth(true));
                    foreach(var obj in m_rootObjects)
                    {
                        DrawObjectTreeItem(obj);
                    }
                    GUILayout.EndScrollView();
                GUILayout.EndVertical();


               GUILayout.BeginVertical(GUILayout.MinWidth((DebugPanel.Get.Size.x - DebugPanel.PANEL_PADDING_LEFT * 2f) * 0.4f));

                GUILayout.BeginHorizontal();
                GUILayout.Label($"### Inspector ###", GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                DebugPanel.Get.PanelConfig.BrowserInspectorDebug = GUILayout.Toggle(DebugPanel.Get.PanelConfig.BrowserInspectorDebug, "Debug");
                GUILayout.EndHorizontal();

                if (m_selectedGameObject != null)
                {
                    m_componentScroll = GUILayout.BeginScrollView(m_componentScroll, GUILayout.ExpandWidth(true));
                        DrawObjectInspector(m_selectedGameObject);
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("<< No GameObject selected. >>");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
               GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawObjectTreeItem(GameObject _object)
        {
            if (!_object) return;

            if (!m_objectFoldState.ContainsKey(_object))
            {
                m_objectFoldState.Add(_object, false);
            }

            GUILayout.BeginHorizontal();
                GUI.color = (   _object == m_selectedGameObject && _object.activeInHierarchy ? Color.green : 
                                _object == m_selectedGameObject && !_object.activeInHierarchy ? Color.green * 0.6f :
                                _object.activeInHierarchy ? Color.white : Color.gray);

                if (_object.transform.childCount <= 0)
                {
                    GUILayout.Space(20f);
                    GUILayout.Label(_object.name);
                }
                else
                {
                    m_objectFoldState[_object] = GUILayout.Toggle(m_objectFoldState[_object], _object.name);
                }
                GUI.color = Color.white;
                if(GUILayout.Button(_object.activeSelf ? "Disable" : "Enable"))
                {
                    _object.SetActive(!_object.activeSelf);
                }
                if (GUILayout.Button(m_selectedGameObject == _object ? "Deselect" : "Select"))
                {
                    m_selectedGameObject = (m_selectedGameObject == _object ? null : _object);
                }
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (m_objectFoldState[_object])
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                GUILayout.BeginVertical();
                if (_object.transform.childCount <= 0)
                {
                    GUILayout.Label("No children!");
                }
                else
                {
                    for (int i = 0; i < _object.transform.childCount; ++i)
                    {
                        DrawObjectTreeItem(_object.transform.GetChild(i).gameObject);
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        private void DrawObjectInspector(GameObject _object)
        {
            GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(50f));
                _object.name = GUILayout.TextField(_object.name ?? "", GUILayout.MinWidth(150f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.Label("Layer: "   + LayerMask.LayerToName(_object.layer));
                GUILayout.FlexibleSpace();
                GUILayout.Label("Tag");
                _object.tag = GUILayout.TextField(_object.tag ?? "", GUILayout.Width(120f));
            GUILayout.EndHorizontal();

            GUILayout.Box("Transform", GUILayout.ExpandWidth(true), GUILayout.Height(20f));
            GUI.color = Color.white;

            GUILayout.BeginVertical();
            _object.transform.position      = DrawType("Position",  _object.transform.position);
            _object.transform.rotation      = DrawType("Rotation",  _object.transform.rotation);
            _object.transform.localScale    = DrawType("Scale",     _object.transform.localScale);
            GUILayout.EndVertical();

            var cmps = _object.GetComponents<Component>();
            foreach(var cmp in cmps)
            {
                DrawComponent(cmp);
            }
        }

        private void DrawComponent(Component _component)
        {
            if (!(_component is Behaviour)) return;

            var behaviour = _component as Behaviour;
            try
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(behaviour.enabled ? "Disable" : "Enable", GUILayout.Width(60f)))
                {
                    behaviour.enabled = !behaviour.enabled;
                }
                GUI.color = behaviour.enabled ? Color.white : Color.gray;
                GUILayout.Box(behaviour.GetType().Name, GUILayout.ExpandWidth(true), GUILayout.Height(20f));
                GUI.color = Color.white;
                if (GUILayout.Button("x", GUILayout.Width(25f)))
                {
                    Component.Destroy(_component);
                    return;
                }
            }
            finally
            {
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginVertical();
            var bFlags = BindingFlags.Public | BindingFlags.Instance;
            if (DebugPanel.Get.PanelConfig.BrowserInspectorDebug)
                bFlags |= BindingFlags.NonPublic | BindingFlags.Static;

            var properties  = _component.GetType().GetProperties(bFlags).ToArray();
            var fields      = _component.GetType().GetFields(bFlags).ToArray();

            foreach(var prop in properties)
            {
                if (!prop.CanRead) continue;
                if(!prop.CanWrite)
                {
                    DrawType(prop.Name, prop.PropertyType, prop.GetValue(_component, null));
                    continue;
                }
                prop.SetValue(_component, DrawType(prop.Name, prop.PropertyType, prop.GetValue(_component, null)), null);
            }

            foreach (var field in fields)
            {
                if(field.IsLiteral)
                {
                    DrawType(field.Name, field.FieldType, field.GetValue(_component));
                    continue;
                }
                field.SetValue(_component, DrawType(field.Name, field.FieldType, field.GetValue(_component)));
            }

            if(DebugPanel.Get.PanelConfig.BrowserInspectorDebug)
            {
                var methods = _component.GetType().GetMethods(bFlags);
                foreach(var meth in methods)
                {
                    DrawComponentMethod(meth);
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawComponentMethod(MethodInfo _method)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{(_method.IsStatic ? "static " : "")}{_method.ReturnType} - {_method.Name} ({string.Join(", ", _method.GetParameters().Select(_o => $"{_o.Name} : {_o.ParameterType}").ToArray())})");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private object DrawType(string _label, System.Type _type, object _value)
        {
            if (!s_typeDrawers.ContainsKey(_type))
            {
                var kvP = s_typeDrawers.FirstOrDefault(_o => _type.IsSubclassOf(_o.Key));
                if (kvP.Key != null && kvP.Value != null)
                {
                    _type = kvP.Key;
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(_label);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_type.Name);
                    GUILayout.EndHorizontal();
                    return _value;
                }
            }
            return s_typeDrawers[_type].Draw(_label, _value);
        }

        private T DrawType<T>(string _label, T _value)
        {
            var type = typeof(T);
            if (!s_typeDrawers.ContainsKey(type))
            {
                var kvP = s_typeDrawers.FirstOrDefault(_o => type.IsSubclassOf(_o.Key));
                if (kvP.Key != null && kvP.Value != null)
                {
                    type = kvP.Key;
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(_label);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(typeof(T).Name);
                    GUILayout.EndHorizontal();
                    return _value;
                }
            }
            return (T)s_typeDrawers[type].Draw(_label, _value);
        }

        private enum EUIState
        {
            ROOTS,
            GAME_OBJECT
        }
    }
}
