using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace pp.SubnauticaMods.Strafe.SML
{
    /// <summary>
    /// Created and registered from main plugin
    /// </summary>
    public class StrafeUpgrade : Craftable
    {
        public StrafeUpgrade() : base("cs_cyclops_manuv_upgrade", "Side Thrusters", "Add additional thrusters to the sides of the cyclops gaining improved maneuverability.")
        { }

        public override CraftTree.Type FabricatorType   => CraftTree.Type.CyclopsFabricator;
        public override TechGroup GroupForPDA           => TechGroup.Cyclops;
        public override TechCategory CategoryForPDA     => TechCategory.CyclopsUpgrades;
        public override string AssetsFolder             => Path.Combine("CyclopsStrafe", "Assets");
        public override TechType RequiredForUnlock      => TechType.CyclopsFabricator;

        public override GameObject GetGameObject()
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube); //;)
            item.name = "cyclops_manuv_upgrade";
            item.GetComponentInChildren<MeshRenderer>().material.color = Color.cyan;
            item.transform.localScale *= 0.3f;
            return item;
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Titanium, 50)
                },
            };
        }
    }
}
