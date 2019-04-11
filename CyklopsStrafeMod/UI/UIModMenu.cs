using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace pp.SubnauticaMods.Strafe
{
    /// <summary>
    /// Menu loader. Creating menu binding buttons, tabs, toggles
    /// </summary>
    public class UIModMenu : MonoBehaviour
    {
        private int m_tabIndex;
        private uGUI_OptionsPanel m_optionsPanel;

        private GameObject m_useModifierBindingOption;

        private uGUI_Binding m_modifierBindingOption;
        private Toggle m_useModifierToggle;

        private string m_currentModifierBinding;

        private Config m_config;

        public void Load(uGUI_OptionsPanel _optionsPanel)
        {
            m_optionsPanel  = _optionsPanel;
            m_config        = CyclopsStrafeMod.ModConfig;
        }

        private void OnEnable()
        {
            if (m_optionsPanel == null) return;

            AddTab();
        }

        private void OnDisable()
        {
            if(CyclopsStrafeMod.ModConfig != null)
                CyclopsStrafeMod.ModConfig.Save();
        }

        private void AddTab()
        {
            m_tabIndex = -1;

            Transform tab;
            Text tabText;

            for (int i = 0; i < m_optionsPanel.tabsContainer.childCount; ++i) //search for "Mods" tab
            {
                tab = m_optionsPanel.tabsContainer.GetChild(i);
                tabText = tab.GetComponentInChildren<Text>(true);
                if (tabText != null && tabText.text == "Mods")
                {
                    m_tabIndex = i;
                }
            }

            if (m_tabIndex == -1) //no existing mods tab found. add new
            {
                m_tabIndex = m_optionsPanel.AddTab("Mods");
            }

            //add options
            m_optionsPanel.AddHeading(m_tabIndex, "Cyclops Strafe");

            //m_optionsPanel.AddHeading(m_tabIndex, "Keyboard");
            m_useModifierToggle             = m_optionsPanel.AddToggleOption(m_tabIndex, "Use modifier key", m_config.UseModifier, OnStrafeUseModifierToggle);
            m_useModifierBindingOption      = AddBinding(m_tabIndex, "Modifier", GetKeyCodeAsInputName(m_config.StrafeModifierKey), GameInput.Device.Keyboard, OnModifierBindingChange);
            AddBinding(m_tabIndex, "Strafe left key", GetKeyCodeAsInputName(m_config.StrafeLeftKey), GameInput.Device.Keyboard, OnStrafeLeftBindingChange);
            AddBinding(m_tabIndex, "Strafe right key", GetKeyCodeAsInputName(m_config.StrafeRightKey), GameInput.Device.Keyboard, OnStrafeRightBindingChange);

            //m_optionsPanel.AddHeading(m_tabIndex, "Controller");

            //AddBinding(m_tabIndex, "Strafe left button", GetKeyCodeAsInputName(m_config.StrafeLeftButton), GameInput.Device.Controller, OnStrafeLeftButtonBindingChange);
            //AddBinding(m_tabIndex, "Strafe right button", GetKeyCodeAsInputName(m_config.StrafeRightButton), GameInput.Device.Controller, OnStrafeRightButtonBindingChange);

            //no speed editing for now
            //m_optionsPanel.AddSliderOption(m_tabIndex, "Strafe speed", m_config.StrafeSpeed, 0.01f, 2f, m_config.StrafeSpeed, OnStrafeSpeedValueChange); 
            m_currentModifierBinding    = System.Enum.GetName(typeof(KeyCode), m_config.StrafeModifierKey);
            m_modifierBindingOption     = m_useModifierBindingOption.GetComponentInChildren<uGUI_Binding>(true);

            if (!m_config.UseModifier)
            {
                m_useModifierBindingOption.SetActive(false);
            }
        }

        //from map mod
        private GameObject AddBinding(int _tabIndex, string _label, string _value, GameInput.Device _device, UnityAction<string> _action)
        {
            GameObject gameObject = m_optionsPanel.AddItem(_tabIndex, m_optionsPanel.bindingOptionPrefab);
            Text txt = gameObject.GetComponentInChildren<Text>();
            if (txt != null)
            {
                gameObject.GetComponentInChildren<TranslationLiveUpdate>().translationKey = _label;
                txt.text = Language.main.Get(_label);
            }

            uGUI_Bindings bindings = gameObject.GetComponentInChildren<uGUI_Bindings>();
            uGUI_Binding binding = bindings.bindings.First<uGUI_Binding>();
            Destroy(bindings.bindings.Last<uGUI_Binding>().gameObject);
            Destroy(bindings);

            binding.device = _device;
            binding.value = _value;
            binding.onValueChanged.RemoveAllListeners();
            binding.onValueChanged.AddListener(_action);
            return gameObject;
        }

        private KeyCode KeyNameToKeyCode(string _keyName)
        {
            try
            {
                KeyCode code = (KeyCode)System.Enum.Parse(typeof(KeyCode), _keyName);
                return code;
            }
            catch (System.Exception _e)
            {
                Util.LogW("Failed to parse " + _keyName + " to valid key-code.");
                return default(KeyCode);
            }
        }

        private void OnStrafeUseModifierToggle(bool _newState)
        {
            m_config.UseModifier = _newState;
            m_useModifierBindingOption.SetActive(_newState);
            if (_newState)
            {
                m_modifierBindingOption.value = m_currentModifierBinding;
                var modType = m_modifierBindingOption.GetType();
                modType.GetMethod("RefreshShownValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(m_modifierBindingOption, null);
            }
            m_config.Save();
        }

        private void OnModifierBindingChange(string _newBinding)
        {
            m_currentModifierBinding = _newBinding;
            m_config.StrafeModifierKey = KeyNameToKeyCode(GetInputNameAsKeyCodeName(_newBinding));
            m_config.Save();
        }

        private void OnStrafeLeftBindingChange(string _newBinding)
        {
            m_config.StrafeLeftKey = KeyNameToKeyCode(GetInputNameAsKeyCodeName(_newBinding));
            m_config.Save();
        }

        private void OnStrafeRightBindingChange(string _newBinding)
        {
            m_config.StrafeRightKey = KeyNameToKeyCode(GetInputNameAsKeyCodeName(_newBinding));
            m_config.Save();
        }

        //private void OnStrafeLeftButtonBindingChange(string _newBinding)
        //{
        //    if (IsAxisInput(_newBinding))
        //    {
        //        m_config.StrafeLeftAxis = _newBinding;
        //        m_config.StrafeLeftButton = KeyCode.None;
        //    }
        //    else
        //    {
        //        m_config.StrafeLeftButton = KeyNameToKeyCode(GetInputNameAsKeyCodeName(_newBinding));
        //        m_config.StrafeLeftAxis = "none";
        //    }
        //    m_config.Save();
        //}

        //private void OnStrafeRightButtonBindingChange(string _newBinding)
        //{
        //    if (IsAxisInput(_newBinding))
        //    {
        //        m_config.StrafeRightAxis = _newBinding;
        //        m_config.StrafeLeftButton = KeyCode.None;
        //    }
        //    else
        //    {
        //        m_config.StrafeRightButton = KeyNameToKeyCode(GetInputNameAsKeyCodeName(_newBinding));
        //        m_config.StrafeLeftAxis = "none";
        //    }
        //    m_config.Save();
        //}

        private void OnStrafeSpeedValueChange(float _newSpeed)
        {
            m_config.StrafeSpeed = _newSpeed;
        }

        //adapted from: GameInput class UWE
        private static string GetKeyCodeAsInputName(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Mouse0:
                    return "MouseButtonLeft";
                case KeyCode.Mouse1:
                    return "MouseButtonRight";
                case KeyCode.Mouse2:
                    return "MouseButtonMiddle";
                case KeyCode.Alpha0:
                    return "0";
                case KeyCode.Alpha1:
                    return "1";
                case KeyCode.Alpha2:
                    return "2";
                case KeyCode.Alpha3:
                    return "3";
                case KeyCode.Alpha4:
                    return "4";
                case KeyCode.Alpha5:
                    return "5";
                case KeyCode.Alpha6:
                    return "6";
                case KeyCode.Alpha7:
                    return "7";
                case KeyCode.Alpha8:
                    return "8";
                case KeyCode.Alpha9:
                    return "9";
                case KeyCode.JoystickButton0:
                    return "ControllerButtonA";
                case KeyCode.JoystickButton1:
                    return "ControllerButtonB";
                case KeyCode.JoystickButton2:
                    return "ControllerButtonX";
                case KeyCode.JoystickButton3:
                    return "ControllerButtonY";
                case KeyCode.JoystickButton4:
                    return "ControllerButtonLeftBumper";
                case KeyCode.JoystickButton5:
                    return "ControllerButtonRightBumper";
                case KeyCode.JoystickButton6:
                    return "ControllerButtonBack";
                case KeyCode.JoystickButton7:
                    return "ControllerButtonHome";
                case KeyCode.JoystickButton8:
                    return "ControllerButtonLeftStick";
                case KeyCode.JoystickButton9:
                    return "ControllerButtonRightStick";
                default:
                    return keyCode.ToString();
            }
        }

        private static string AxisName(string _inputName)
        {
            switch (_inputName)
            {
                case "ControllerLeftStickLeft":
                case "ControllerLeftStickRight":
                    return "ControllerLeftStickX";
                case "ControllerLeftStickDown":
                case "ControllerLeftStickUp":
                    return "ControllerLeftStickY";
                case "ControllerRightStickLeft":
                case "ControllerRightStickRight":
                    return "ControllerRightStickX";
                case "ControllerRightStickDown":
                case "ControllerRightStickUp":
                    return "ControllerRightStickY";
                case "ControllerDPadLeft":
                case "ControllerDPadRight":
                    return "ControllerDPadX";
                case "ControllerDPadDown":
                case "ControllerDPadUp":
                    return "ControllerDPadY";
                case "ControllerLeftTrigger":
                    return "ControllerLeftTrigger";
                case "ControllerRightTrigger":
                    return "ControllerRightTrigger";
            }
            return "none";
        }

        private static bool IsAxisInput(string _inputName)
        {
            switch (_inputName)
            {
                case "ControllerLeftStickLeft":
                case "ControllerLeftStickRight":
                case "ControllerLeftStickDown":
                case "ControllerLeftStickUp":
                case "ControllerRightStickLeft":
                case "ControllerRightStickRight":
                case "ControllerRightStickDown":
                case "ControllerRightStickUp":
                case "ControllerDPadLeft":
                case "ControllerDPadRight":
                case "ControllerDPadDown":
                case "ControllerDPadUp":
                case "ControllerLeftTrigger":
                case "ControllerRightTrigger":
                    return true;
            }
            return false;
        }

        private static string GetInputNameAsKeyCodeName(string _inputName)
        {
            switch (_inputName)
            {
                case "MouseButtonLeft":
                    return "Mouse0";
                case "MouseButtonRight":
                    return "Mouse1"; 
                case "MouseButtonMiddle":
                    return "Mouse2";
                case "0": 
                    return "Alpha0";
                case "1":
                    return "Alpha1";
                case "2":
                    return "Alpha2";
                case "3":
                    return "Alpha3";
                case "4":
                    return "Alpha4";
                case "5":
                    return "Alpha5";
                case "6":
                    return "Alpha6";
                case "7":
                    return "Alpha7";
                case "8":
                    return "Alpha8";
                case "9":
                    return "Alpha9";
                case "ControllerButtonA":
                    return "JoystickButton0";
                case "ControllerButtonB":
                    return "JoystickButton1";
                case "ControllerButtonX":
                    return "JoystickButton2";
                case "ControllerButtonY":
                    return "JoystickButton3";
                case "ControllerButtonLeftBumper":
                    return "JoystickButton4";
                case "ControllerButtonRightBumper":
                    return "JoystickButton5";
                case "ControllerButtonBack":
                    return "JoystickButton6";
                case "ControllerButtonHome":
                    return "JoystickButton7";
                case "ControllerButtonLeftStick":
                    return "JoystickButton8";
                case "ControllerButtonRightStick":
                    return "JoystickButton9";
                default:
                    return _inputName;
            }
        }
    }
}
