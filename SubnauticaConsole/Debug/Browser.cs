using plib.Util;
using pp.subnauticamods.dbg;
using pp.subnauticamods.dbg.Config;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pp.SubnauticaMods.dbg
{
    public class Browser
    {
        private const int NUM_HOR_PANELS = 4;

        private Scene[] m_activeScenes;

        private GameObject      m_selectedGameObject;
        private Component[]     m_selectedComponents;
        private Component       m_selectedComponent;
        private FieldInfo[]     m_selectedMember;
        private MethodInfo[]    m_selectedMethod;

        private Vector2 m_gameObjectScroll  = Vector2.zero;
        private Vector2 m_componentScroll   = Vector2.zero;
        private Vector2 m_memberScroll      = Vector2.zero;
        private Vector2 m_methodScroll      = Vector2.zero;

        private Dictionary<System.Array, bool> m_arrayFoldableStates = new Dictionary<System.Array, bool>();
        private Dictionary<System.Type, System.Func<string, object, object>> m_typeDrawers;
        private Dictionary<GameObject, bool> m_objectFoldState = new Dictionary<GameObject, bool>();
        private GameObject[] m_rootObjects;

        private object m_invokeResult;
        private EUIState m_consoleUIState = EUIState.ROOTS;

        #region ENGINE_CALLBACKS
        public void Start()
        {
            SceneManager.sceneLoaded -= OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnActiveSceneChanged;

            InitializeTypeDrawers();
        }

        public void Update()
        {
            ReloadScenes();
        }

        public void Draw(GUIStyle _browserStyle)
        {
            GUILayout.BeginVertical(_browserStyle, GUILayout.MinHeight(300f));

            GUILayout.Label($"### Hierarchy ({m_activeScenes?.Length ?? 0} scene(s) loaded) ###", GUILayout.ExpandWidth(true));
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
            m_consoleUIState        = EUIState.ROOTS;

            m_objectFoldState.Clear();
            m_rootObjects           = new GameObject[0];
            m_selectedGameObject    = null;
            m_selectedComponents    = null;
            m_selectedComponent     = null;
            m_selectedMember        = null;
            m_selectedMethod        = null;
        }

        private void InitializeTypeDrawers()
        {
            m_arrayFoldableStates = new Dictionary<System.Array, bool>();

            var types   = Assembly.GetExecutingAssembly().GetExportedTypes();
            var methods = types.Select(_o => _o.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
            var vAtt = methods
                .SelectMany(_methodNfo => _methodNfo
                    .Select(_nfo => new KeyValuePair<MethodInfo, object[]>(_nfo, _nfo.GetCustomAttributes(true).Select(_att => _att is TypeDrawer ? _att : null).Where(_att => _att != null)
                        .ToArray()))
                    .Where(_o => _o.Value != null && _o.Value.Length > 0))
                    .ToDictionary(_o => _o.Key, _o => _o.Value);

        }

        private void DrawObjectTree()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.MinWidth(DebugPanel.Get.Size.x * 0.6f));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Refresh"))
                {
                    ReloadScenes();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                m_gameObjectScroll = GUILayout.BeginScrollView(m_gameObjectScroll, GUILayout.ExpandWidth(true));
                foreach(var obj in m_rootObjects)
                {
                    DrawObjectTreeItem(obj);
                }
                GUILayout.EndScrollView();
            GUILayout.EndVertical();


            if (m_selectedGameObject != null)
            {
                GUILayout.BeginVertical(GUILayout.MinWidth(DebugPanel.Get.Size.x * 0.4f));
                    m_componentScroll = GUILayout.BeginScrollView(m_componentScroll, GUILayout.ExpandWidth(true));
                    DrawObjectInspector(m_selectedGameObject);
                    GUILayout.EndScrollView();
                GUILayout.EndVertical();
            } else
            {
                GUILayout.Label("<< No GameObject selected. >>");
            }

            GUILayout.EndHorizontal();
        }

        private void DrawObjectTreeItem(GameObject _object)
        {
            if (!_object) return;

            if (!m_objectFoldState.ContainsKey(_object))
                m_objectFoldState.Add(_object, false);

            GUILayout.BeginHorizontal();
                GUI.color = (_object.activeInHierarchy ? Color.white : Color.gray);
                m_objectFoldState[_object] = GUILayout.Toggle(m_objectFoldState[_object], _object.name);
                GUI.color = Color.white;
                if(GUILayout.Button(_object.activeSelf ? "Disable" : "Enable"))
                {
                    _object.SetActive(!_object.activeSelf);
                }
                if (GUILayout.Button(m_selectedGameObject == _object ? "Deselect" : "Select"))
                {
                    m_selectedGameObject = m_selectedGameObject == _object ? null : _object;
                }
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (m_objectFoldState[_object])
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10f);
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
                _object.tag = GUILayout.TextField(_object.tag ?? "", GUILayout.Width(150f));
                GUILayout.FlexibleSpace();
                _object.isStatic = GUILayout.Toggle(_object.isStatic, "Static");
            GUILayout.EndHorizontal();

            GUILayout.Box("Transform", GUILayout.ExpandWidth(true), GUILayout.Height(20f));
            GUI.color = Color.white;

            GUILayout.BeginVertical();
            _object.transform.position      = (Vector3)m_typeDrawers[typeof(Vector3)]("Position", _object.transform.position);
            _object.transform.rotation      = (Quaternion)m_typeDrawers[typeof(Quaternion)]("Rotation", _object.transform.rotation);
            _object.transform.localScale    = (Vector3)m_typeDrawers[typeof(Vector3)]("Scale", _object.transform.localScale);
            GUILayout.EndVertical();

            var cmps = _object.GetComponents<Component>();
            foreach(var cmp in cmps)
            {
                if (cmp is Behaviour)
                {
                    GUILayout.BeginHorizontal();
                    GUI.color = ((cmp as Behaviour).enabled ? Color.white : Color.gray);
                    GUILayout.Box(cmp.GetType().Name, GUILayout.ExpandWidth(true), GUILayout.Height(20f));
                    GUI.color = Color.white;
                    if (GUILayout.Button((cmp as Behaviour).enabled ? "Disable" : "Enable", GUILayout.Width(100f))) 
                    {
                        (cmp as Behaviour).enabled = !(cmp as Behaviour).enabled;
                    }

                    GUILayout.EndHorizontal();

                    DrawComponent(cmp);
                }
            }
        }

        private void DrawModeGameObject()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Back"))
            {
                if (m_selectedGameObject.transform.parent == null)
                {
                    m_consoleUIState = EUIState.ROOTS;
                    m_gameObjectScroll = Vector2.zero;
                }
                else
                {
                    m_selectedGameObject = m_selectedGameObject.transform.parent.gameObject;
                }
            }
            GUILayout.Label($"GameObject: {m_selectedGameObject.name}");
            GUILayout.Label($"Component: {(m_selectedComponent ? m_selectedComponent.GetType().Name : "None selected")}");
            GUILayout.Label($"Path: {m_selectedGameObject.GetHierarchyPath()}");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            //CHILD LIST
            GUILayout.BeginVertical(GUILayout.Width((DebugPanel.Get.Size.x - (DebugPanel.PANEL_PADDING_LEFT + 5f)) / NUM_HOR_PANELS));
            GUILayout.Label("### Children ###");
            if (m_selectedGameObject.transform.childCount <= 0)
            {
                GUILayout.Label("No children!");
            }
            else
            {
                m_gameObjectScroll = GUILayout.BeginScrollView(m_gameObjectScroll);
                for (int i = 0; i < m_selectedGameObject.transform.childCount; ++i)
                    DrawGameObject(m_selectedGameObject.transform.GetChild(i).gameObject);
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            //COMPONENT LIST
            GUILayout.BeginVertical(GUILayout.Width((DebugPanel.Get.Size.x - (DebugPanel.PANEL_PADDING_LEFT + 5f)) / NUM_HOR_PANELS));
            GUILayout.Label("### Components ###");
            if (m_selectedComponents == null || m_selectedComponents.Length <= 0)
            {
                GUILayout.Label("No components!");
            }
            else
            {
                m_componentScroll = GUILayout.BeginScrollView(m_componentScroll);
                foreach (var cmp in m_selectedComponents)
                {
                    DrawComponent(cmp);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            //MEMBER LIST
            GUILayout.BeginVertical(GUILayout.Width((DebugPanel.Get.Size.x - (DebugPanel.PANEL_PADDING_LEFT + 5f)) / NUM_HOR_PANELS));
            GUILayout.Label("### Members ### ");
            if (m_selectedComponent == null || m_selectedMember == null || m_selectedMember.Length <= 0)
            {
                GUILayout.Label("No members!");
            }
            else
            {
                m_memberScroll = GUILayout.BeginScrollView(m_memberScroll);
                foreach (var member in m_selectedMember)
                {
                    DrawComponentMember(m_selectedComponent, member);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            //METHODS
            GUILayout.BeginVertical(GUILayout.Width((DebugPanel.Get.Size.x - (DebugPanel.PANEL_PADDING_LEFT + 5f)) / NUM_HOR_PANELS));
            GUILayout.Label("### Methods ### ");
            if (m_invokeResult != null)
                GUILayout.Label($"Last invoke result:\t\n{m_invokeResult}");
            if (m_selectedComponent == null || m_selectedMethod == null || m_selectedMethod.Length <= 0)
            {
                GUILayout.Label("No methods found!");
            }
            else
            {
                m_methodScroll = GUILayout.BeginScrollView(m_methodScroll);
                foreach (var method in m_selectedMethod)
                {
                    DrawComponentMethod(method);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawGameObject(GameObject _go)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select"))
            {
                m_selectedGameObject = _go;
                m_selectedComponents = m_selectedGameObject.GetComponents<Component>();
                m_consoleUIState = EUIState.GAME_OBJECT;
                m_gameObjectScroll = Vector2.zero;
                m_selectedComponent = null;
            }
            GUILayout.Label($"- {_go.name}");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawComponent(Component _component)
        {
            GUILayout.BeginVertical();
            var member = _component
                                            .GetType()
                                            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                            //.Where(_o => (m_showPublic && _o.IsPublic) || (m_showPrivate && _o.IsPrivate))
                                            .ToArray();
            member.ForEach(_o => DrawField(_o, _component));
            GUILayout.EndVertical();
        }

        private void DrawComponentMember(Component _cmp, FieldInfo _member)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{_member.Name} ({_member.GetValue(_member.IsStatic ? null : _cmp)}) : {(_member.IsStatic ? "static " : "")} {_member.FieldType}", GUILayout.ExpandWidth(true));
            DrawField(_member, m_selectedComponent);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawComponentMethod(MethodInfo _method)
        {
            GUILayout.BeginHorizontal();
            if (_method.GetParameters().Length <= 0 && GUILayout.Button("Invoke"))
            {
                m_invokeResult = _method.Invoke(m_selectedComponent, null);
            }
            GUILayout.Label($"{(_method.IsStatic ? "static " : "")}{_method.ReturnType} - {_method.Name} ({string.Join(", ", _method.GetParameters().Select(_o => $"{_o.Name} : {_o.ParameterType}").ToArray())})");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private object DrawArray(string _label, object _object)
        {
            var arr = _object as System.Array;
            if (arr == null)
                return null;
            if (!m_arrayFoldableStates.ContainsKey(arr))
                m_arrayFoldableStates.Add(arr, false);
            m_arrayFoldableStates[arr] = GUILayout.Toggle(m_arrayFoldableStates[arr], _label);
            if (m_arrayFoldableStates[arr])
            {
                GUILayout.BeginHorizontal();
                GUILayout.Width(15f);
                var length = int.Parse(GUILayout.TextField("Elements", arr.Length));
                var elementType = arr.GetType().GetElementType();
                if (length != arr.Length)
                {
                    System.Array newArray = System.Array.CreateInstance(elementType, length);
                    System.Array.Copy(arr, newArray, Mathf.Min(arr.Length, newArray.Length));
                    for (var i = Mathf.Max(0, arr.Length - 1); i < newArray.Length; ++i)
                    {
                        newArray.SetValue(elementType.Assembly.CreateInstance(elementType.FullName), i);
                    }
                    m_arrayFoldableStates.Remove(arr);
                    m_arrayFoldableStates.Add(newArray, true);
                    arr = newArray;
                    _object = arr;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Width(15f);
                GUILayout.BeginVertical();
                for (var i = 0; i < arr.Length; ++i)
                {
                    arr.SetValue(DrawField("Element " + i, elementType, arr.GetValue(i)), i);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            return arr;
        }

        private void DrawField(FieldInfo _nfo, object _source)
        {
            var type = _nfo.FieldType;
            if (!m_typeDrawers.ContainsKey(type))
            {
                var kvP = m_typeDrawers.FirstOrDefault(_o => type.IsSubclassOf(_o.Key));
                if (kvP.Key == null || kvP.Value == null)
                    return;
                type = kvP.Key;
            }
            _nfo.SetValue(m_selectedComponent, m_typeDrawers[type](_nfo.Name, _nfo.GetValue(_source)));
        }

        private object DrawField(string _label, System.Type _type, object _value)
        {
            if (!m_typeDrawers.ContainsKey(_type))
            {
                var kvP = m_typeDrawers.FirstOrDefault(_o => _type.IsSubclassOf(_o.Key));
                if (kvP.Key != null && kvP.Value != null)
                    _type = kvP.Key;
            }
            return m_typeDrawers[_type](_label, _value);
        }

        private void RefreshMember(Component _cmp)
        {
            m_selectedMember = _cmp
                                            .GetType()
                                            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                            //.Where(_o => (m_showPublic && _o.IsPublic) || (m_showPrivate && _o.IsPrivate))
                                            .ToArray();

            m_selectedMethod = _cmp
                                            .GetType()
                                            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                            //.Where(_o => (m_showMethPublic && _o.IsPublic) || (m_showMethPrivate && _o.IsPrivate))
                                            .ToArray();
        }

        private enum EUIState
        {
            ROOTS,
            GAME_OBJECT
        }
    }
}
