using Harmony;
using System.Reflection;
using UnityEngine;

namespace pp.SubnauticaMods.Strafe
{
    [HarmonyPatch(typeof(SubControl)), HarmonyPatch("FixedUpdate")]
    public class ControlPatch
    {
        private static ControlPatch m_instance = null;
        private static bool m_failedToLoad;

        private SubControl m_subControls;

        private System.Type m_subControlType;
        private FieldInfo m_throttleMemberInfo;
        private FieldInfo m_canAccelInfo;

        private Rigidbody m_rigidBody;

        private static bool Prefix(SubControl __instance)
        {
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
                Debug.LogWarning("[CyclopsStrafe] Error occurred while initializing cyclops strafe mod(" + _e.GetType().Name + "): " + _e.Message);
                Debug.LogWarning("[CyclopsStrafe]\n" + _e.StackTrace);
                m_failedToLoad = true;
                return false;
            }
            return true;
        }

        private static void Postfix()
        {
            if (m_failedToLoad) return;
            m_instance?.UpdateCyklopsControls();
        }

        private void Load(SubControl _subControl)
        {
            if (m_subControls != null) return; //already loaded

            m_subControls           = _subControl;
            m_subControlType        = m_subControls.GetType();
            m_throttleMemberInfo    = GetPrivateSubControlMemberInfo("throttle");
            m_canAccelInfo          = GetPrivateSubControlMemberInfo("canAccel");

            m_rigidBody             = m_subControls.GetComponent<Rigidbody>();

            Debug.Log("[CyclopsStrafe] Initialized Cyclops strafe mod!");
        }

        private void UpdateCyklopsControls() //injected at the end of SubControl.FixedUpdate() method
        {
            if (m_subControls == null) return;

            if (!m_subControls.LOD.IsFull()) return;

            if (m_subControls.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline) return;

            if (Ocean.main.GetDepthOf(m_subControls.gameObject) <= 0f) return;

            var throttle = (Vector3)m_throttleMemberInfo.GetValue(m_subControls);

            m_rigidBody.freezeRotation = false;

            if ((double)Mathf.Abs(throttle.x) <= 0.0001 || !Input.GetKey(KeyCode.LeftShift)) return;

            m_rigidBody.freezeRotation = true; //prevent the cyclops from rotating.

            float num = m_subControls.BaseVerticalAccel;
            num += (float)m_subControls.gameObject.GetComponentsInChildren<BallastWeight>().Length * m_subControls.AccelPerBallast;
            var canAccel = (bool)m_canAccelInfo.GetValue(m_subControls);
            if (canAccel)
            {
                m_rigidBody.AddForce(-m_subControls.transform.right * num * m_subControls.accelScale * throttle.x, ForceMode.Acceleration);
            }
        }

        private FieldInfo GetPrivateSubControlMemberInfo(string _memberName)
        {
            return m_subControlType.GetField(_memberName, BindingFlags.Instance | BindingFlags.NonPublic); ;
        }
    }
}
