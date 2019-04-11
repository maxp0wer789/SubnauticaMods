using Harmony;
using System.Reflection;

namespace dd.pp.reapertest
{
    [HarmonyPatch(typeof(MeleeAttack)), HarmonyPatch("OnEnable")]
    public class SetReaperDamage
    {
        public const float REAPER_DAMAGE = 10f;

        public static void Patch()
        {
            var harmony = HarmonyInstance.Create("dd.pp.reapertest");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static bool Prefix(MeleeAttack __instance)
        {
            if (__instance is ReaperMeleeAttack)
            {
                __instance.biteDamage = REAPER_DAMAGE;
                return true;
            }

            return false;
        }
    }
}
