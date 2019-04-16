using SMLHelper.V2.Options;

namespace pp.SubnauticaMods.Strafe.SML
{
    /// <summary>
    /// Options using SMLHelper, if SMLHelper is available
    /// </summary>
    public class SMLCyclopsStrafeOptions : ModOptions
    {
        private const string OPT_IDENT_TGL_MODIF        = "tgl_modifier";
        private const string OPT_IDENT_KB_MODIF_KEY     = "kb_modifierKey";
        private const string OPT_IDENT_KB_STRAFE_LEFT   = "kb_strafeLeft";
        private const string OPT_IDENT_KB_STRAFE_RIGHT  = "kb_strafeRight";

        public SMLCyclopsStrafeOptions() : base("Cyclops Strafe")
        {
            KeybindChanged  += OnKeybindChanged;
            ToggleChanged   += OnToggleChanged;
        }

        public override void BuildModOptions()
        {
            AddToggleOption(OPT_IDENT_TGL_MODIF,         "Use modifier key", CyclopsStrafeMod.ModConfig.UseModifier);
            AddKeybindOption(OPT_IDENT_KB_MODIF_KEY,     "Modifier key",     GameInput.Device.Keyboard, CyclopsStrafeMod.ModConfig.StrafeModifierKey);
            AddKeybindOption(OPT_IDENT_KB_STRAFE_LEFT,   "Strafe left key",  GameInput.Device.Keyboard, CyclopsStrafeMod.ModConfig.StrafeLeftKey);
            AddKeybindOption(OPT_IDENT_KB_STRAFE_RIGHT,  "Strafe right key", GameInput.Device.Keyboard, CyclopsStrafeMod.ModConfig.StrafeRightKey);
        }

        private void OnToggleChanged(object _sender, ToggleChangedEventArgs _args)
        {
            switch (_args.Id)
            {
                case OPT_IDENT_TGL_MODIF:
                    CyclopsStrafeMod.ModConfig.UseModifier = _args.Value;
                    break;
                default: throw new System.ArgumentException("Invalid id argument " + _args.Id + " for keybind supplied by smlhelper.");
            }
            CyclopsStrafeMod.ModConfig.Save();
        }

        private void OnKeybindChanged(object _sender, KeybindChangedEventArgs _args)
        {
            switch(_args.Id)
            {
                case OPT_IDENT_KB_MODIF_KEY:
                    CyclopsStrafeMod.ModConfig.StrafeModifierKey = _args.Key;
                    break;
                case OPT_IDENT_KB_STRAFE_LEFT:
                    CyclopsStrafeMod.ModConfig.StrafeLeftKey = _args.Key;
                    break;
                case OPT_IDENT_KB_STRAFE_RIGHT:
                    CyclopsStrafeMod.ModConfig.StrafeRightKey = _args.Key;
                    break;
                default: throw new System.ArgumentException("Invalid id argument " + _args.Id + " for keybind supplied by smlhelper.");
            }
            CyclopsStrafeMod.ModConfig.Save();
        }
    }
}
