using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TAutomation.Items
{
    public class BloodMoonSensorItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Sensor detects if a Blood Moon is happening");
            DisplayName.SetDefault("Blood Moon Sensor");
        }

        public override void SetDefaults()
        {
            Item.createTile = ModContent.TileType <Tiles.TSensorTile>();
            Item.width = 16;
            Item.height = 16;
            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.placeStyle = 0; // 0 for the BloodMoonSensor sub tile
            Item.mech = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BloodMoonStarter, 1)
                .AddIngredient(ItemID.IronBar, 1)
                .AddIngredient(ItemID.Wire, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
