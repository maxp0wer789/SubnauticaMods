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
            m_optionsPanel.AddToggleOption(m_tabIndex, "Use modifier key", m_config.UseModifier, OnStrafeUseModifierToggle);
            m_useModifierBindingOption = AddBinding(m_tabIndex, "Modifier key", System.Enum.GetName(typeof(KeyCode), m_config.StrafeModifierKey), OnModifierBindingChange);
            AddBinding(m_tabIndex, "Strafe left key", System.Enum.GetName(typeof(KeyCode), m_config.StrafeLeftKey), OnStrafeLeftBindingChange);
            AddBinding(m_tabIndex, "Strafe right key", System.Enum.GetName(typeof(KeyCode), m_config.StrafeRightKey), OnStrafeRightBindingChange);
            //no speed editing for now
            //m_optionsPanel.AddSliderOption(m_tabIndex, "Strafe speed", m_config.StrafeSpeed, 0.01f, 2f, m_config.StrafeSpeed, OnStrafeSpeedValueChange); 

            m_currentModifierBinding = System.Enum.GetName(typeof(KeyCode), m_config.StrafeModifierKey);
            m_modifierBindingOption = m_useModifierBindingOption.GetComponentInChildren<uGUI_Binding>(true);
            if (!m_config.UseModifier)
            {
                m_useModifierBindingOption.SetActive(false);
            }
        }

        //from map mod
        private GameObject AddBinding(int _tabIndex, string _label, string _value, UnityAction<string> _action)
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
            m_config.StrafeModifierKey = KeyNameToKeyCode(_newBinding);
            m_config.Save();
        }

        private void OnStrafeLeftBindingChange(string _newBinding)
        {
            m_config.StrafeLeftKey = KeyNameToKeyCode(_newBinding);
            m_config.Save();
        }

        private void OnStrafeRightBindingChange(string _newBinding)
        {
            m_config.StrafeRightKey = KeyNameToKeyCode(_newBinding);
            m_config.Save();
        }

        private void OnStrafeSpeedValueChange(float _newSpeed)
        {
            m_config.StrafeSpeed = _newSpeed;
        }
    }
}
