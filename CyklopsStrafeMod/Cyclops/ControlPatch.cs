using Harmony;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace pp.SubnauticaMods.Strafe
{
    /// <summary>
    /// Patch for <see cref="SubControl.FixedUpdate"/> and main class of the control overrides.
    /// Update patch is created in <see cref="UpdatePatch"/>.
    /// </summary>
    [HarmonyPatch(typeof(SubControl)), HarmonyPatch("FixedUpdate")]
    public class ControlPatch
    {
        public static string ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static ControlPatch Instance => m_instance;
        private static ControlPatch m_instance = null;

        private FieldInfo m_throttleMemberInfo;
        private FieldInfo m_canAccelInfo;
        private bool m_strafing;

        private static bool Prefix(SubControl __instance)
        {
            if (CyclopsStrafeMod.ModErrorOccurred) return false;

            if (m_instance != null) return true;

            try
            {
                m_instance = new ControlPatch();
                m_instance.Load();
            }
            catch (System.Exception _e)
            {
                Util.LogW($"Error occurred while initializing cyclops strafe mod({_e.GetType().Name}): {_e.Message}");
                Util.LogW("\n" + _e.StackTrace);
                CyclopsStrafeMod.ModErrorOccurred = true;
                return false;
            }
            return true;
        }

        private static void Postfix(SubControl __instance)
        {
            if (CyclopsStrafeMod.ModErrorOccurred) return;

            m_instance?.FixedUpdateCyclopsControls(__instance);
        }

        private void Load()
        {
            m_throttleMemberInfo    = GetPrivateSubControlMemberInfo("throttle");
            m_canAccelInfo          = GetPrivateSubControlMemberInfo("canAccel");
        }

        private void FixedUpdateCyclopsControls(SubControl _subControl) //injected at the end of SubControl.FixedUpdate() method
        {
            try
            {
                if (_subControl == null) return;

                if (!_subControl.LOD.IsFull()) return;

                if (_subControl.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline) return;

                if (Ocean.main.GetDepthOf(_subControl.gameObject) <= 0f) return;

                var throttle = (Vector3)m_throttleMemberInfo.GetValue(_subControl);

                var rb = _subControl.GetComponent<Rigidbody>();
                rb.freezeRotation = false;

                if ((double)Mathf.Abs(throttle.x) <= 0.0001 || !m_strafing) return;

                rb.freezeRotation = true;

                float num = _subControl.BaseVerticalAccel;
                num += (float)_subControl.gameObject.GetComponentsInChildren<BallastWeight>().Length * _subControl.AccelPerBallast;
                var canAccel = (bool)m_canAccelInfo.GetValue(_subControl);
                if (canAccel)
                {
                    rb.AddForce(-_subControl.transform.right * num * _subControl.accelScale * throttle.x * CyclopsStrafeMod.ModConfig.StrafeSpeed, ForceMode.Acceleration);
                }
            }
            catch (System.Exception _e)
            {
                Util.LogE("Error occurred while updating mod: " + _e.Message);
                Util.LogE("\n" + _e.StackTrace);
                CyclopsStrafeMod.ModErrorOccurred = true;
            }
        }

        public void UpdateCyclopsControls(SubControl _subControl)
        {
            if (_subControl == null) return;

            if (!_subControl.LOD.IsFull()) return;

            if (_subControl.controlMode != SubControl.Mode.DirectInput) return;

            m_strafing = false;

            var canAccel = (bool)m_canAccelInfo.GetValue(_subControl);
            var throttle = (Vector3)m_throttleMemberInfo.GetValue(_subControl);

            if ((CyclopsStrafeMod.ModConfig.UseModifier ? (CyclopsStrafeMod.ModConfig.ModifierActive && CyclopsStrafeMod.ModConfig.ThrottleLeft) : CyclopsStrafeMod.ModConfig.ThrottleLeft))
            {
                throttle.x = -1f;
                m_throttleMemberInfo.SetValue(_subControl, throttle);
                m_strafing = true;
            }
            else if (CyclopsStrafeMod.ModConfig.UseModifier ? (CyclopsStrafeMod.ModConfig.ModifierActive && CyclopsStrafeMod.ModConfig.ThrottleRight) : CyclopsStrafeMod.ModConfig.ThrottleRight)
            {
                throttle.x = 1f;
                m_throttleMemberInfo.SetValue(_subControl, throttle);
                m_strafing = true;
            }
        }

        private FieldInfo GetPrivateSubControlMemberInfo(string _memberName)
        {
            return typeof(SubControl).GetField(_memberName, BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }

    [HarmonyPatch(typeof(SubControl)), HarmonyPatch("Update")]
    public class UpdatePatch
    {
        private static void Postfix(SubControl __instance)
        {
            if (CyclopsStrafeMod.ModErrorOccurred) return;

            ControlPatch.Instance?.UpdateCyclopsControls(__instance);
        }
    }
}
