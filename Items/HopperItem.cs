using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TAutomation.Items
{
    public class HopperItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Collects items into the chest bellow");
            DisplayName.SetDefault("Hopper");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = 100;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.consumable = true;
            Item.reuseDelay = 1;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.White;
            Item.createTile = ModContent.TileType<Tiles.HopperTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 10)
                .AddIngredient(ItemID.Cog, 4)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
