using Harmony;
using System.Reflection;

namespace pp.SubnauticaMods.Strafe
{
    public class CyclopsStrafeMod
    {
        public static void Initialize()
        {
            var harmony = HarmonyInstance.Create("de.petesplace.subnauticamods.cstrafe");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
