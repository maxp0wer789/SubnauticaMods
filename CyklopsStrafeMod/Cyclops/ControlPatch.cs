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

        private SubControl m_subControls;

        private System.Type m_subControlType;
        private FieldInfo m_throttleMemberInfo;
        private FieldInfo m_canAccelInfo;
        private bool m_strafing;

        private Rigidbody m_rigidBody;

        private static bool Prefix(SubControl __instance)
        {
            if (CyclopsStrafeMod.ModErrorOccurred) return false;

            if (m_instance == null)
            {
                m_instance = new ControlPatch();
            }
            try
            {
                m_instance.Load(__instance);
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

        private static void Postfix()
        {
            if (CyclopsStrafeMod.ModErrorOccurred) return;

            m_instance?.FixedUpdateCyclopsControls();
        }

        private void Load(SubControl _subControl)
        {
            if (m_subControls != null) return; //already loaded

            m_subControls           = _subControl;
            m_subControlType        = m_subControls.GetType();
            m_throttleMemberInfo    = GetPrivateSubControlMemberInfo("throttle");
            m_canAccelInfo          = GetPrivateSubControlMemberInfo("canAccel");

            m_rigidBody             = m_subControls.GetComponent<Rigidbody>();
        }

        private void FixedUpdateCyclopsControls() //injected at the end of SubControl.FixedUpdate() method
        {
            try
            {
                if (m_subControls == null) return;

                if (!m_subControls.LOD.IsFull()) return;

                if (m_subControls.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline) return;

                if (Ocean.main.GetDepthOf(m_subControls.gameObject) <= 0f) return;

                var throttle = (Vector3)m_throttleMemberInfo.GetValue(m_subControls);

                Util.Log(m_strafing.ToString());
                //m_rigidBody.freezeRotation = m_strafing;
                m_rigidBody.freezeRotation = false;

                if ((double)Mathf.Abs(throttle.x) <= 0.0001 || !m_strafing) return;

                m_rigidBody.freezeRotation = true;

                float num = m_subControls.BaseVerticalAccel;
                num += (float)m_subControls.gameObject.GetComponentsInChildren<BallastWeight>().Length * m_subControls.AccelPerBallast;
                var canAccel = (bool)m_canAccelInfo.GetValue(m_subControls);
                if (canAccel)
                {
                    m_rigidBody.AddForce(-m_subControls.transform.right * num * m_subControls.accelScale * throttle.x * CyclopsStrafeMod.ModConfig.StrafeSpeed, ForceMode.Acceleration);
                }
            }
            catch (System.Exception _e)
            {
                Util.LogE("Error occurred while updating mod: " + _e.Message);
                Util.LogE("\n" + _e.StackTrace);
                CyclopsStrafeMod.ModErrorOccurred = true;
            }
        }

        public void UpdateCyclopsControls()
        {
            m_strafing = false;

            if (m_subControls == null) return;

            if (!m_subControls.LOD.IsFull()) return;

            if (m_subControls.controlMode != SubControl.Mode.DirectInput) return;

            var canAccel = (bool)m_canAccelInfo.GetValue(m_subControls);
            var throttle = (Vector3)m_throttleMemberInfo.GetValue(m_subControls);

            if (CyclopsStrafeMod.ModConfig.UseModifier ? (CyclopsStrafeMod.ModConfig.ModifierActive && CyclopsStrafeMod.ModConfig.ThrottleLeft) : CyclopsStrafeMod.ModConfig.ThrottleLeft)
            {
                throttle.x = -1f;
                m_throttleMemberInfo.SetValue(m_subControls, throttle);
                m_strafing = true;
            }
            else if (CyclopsStrafeMod.ModConfig.UseModifier ? (CyclopsStrafeMod.ModConfig.ModifierActive && CyclopsStrafeMod.ModConfig.ThrottleRight) : CyclopsStrafeMod.ModConfig.ThrottleRight)
            {
                throttle.x = 1f;
                m_throttleMemberInfo.SetValue(m_subControls, throttle);
                m_strafing = true;
            }
        }

        private FieldInfo GetPrivateSubControlMemberInfo(string _memberName)
        {
            return m_subControlType.GetField(_memberName, BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }

    [HarmonyPatch(typeof(SubControl)), HarmonyPatch("Update")]
    public class UpdatePatch
    {
        private static void Postfix()
        {
            if (CyclopsStrafeMod.ModErrorOccurred) return;

            ControlPatch.Instance?.UpdateCyclopsControls();
        }
    }
}
