using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TAutomation.Items
{
    public class RainSensorItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Sensor detects if it's raining");
            DisplayName.SetDefault("Rain Sensor");
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
            Item.placeStyle = 1; // 1 for the RainSensor sub tile
            Item.mech = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MagicWaterDropper, 1)
                .AddIngredient(ItemID.IronBar, 1)
                .AddIngredient(ItemID.Wire, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
