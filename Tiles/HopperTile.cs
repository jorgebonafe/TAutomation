using Microsoft.Xna.Framework;
using System.IO;
using TAutomation.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace TAutomation.Tiles
{
    public class HopperTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = false;
            Main.tileFrameImportant[Type] = true;
            DustType = 7; // Wood

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateHeights = new[] { 24 };
            TileObjectData.newTile.DrawYOffset = 0;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 2, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[1] { 21 };
            //TileObjectData.newTile.AnchorAlternateTiles = new int[2] { 21, 88 };

            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(CanPlace, -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<HopperTileEntity>().Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(214, 143, 118));
        }

        public static int CanPlace(int i, int j, int type, int style, int direction, int alternative)
        {
            // Top Left corner of the chest tile is the chest entity.
            int chestId = Chest.FindChest(i, j + 1); // If this exists, hopper is above chest
            if (chestId == -1)
                return -1;

            return 0;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 20, ModContent.ItemType<Items.HopperItem>());
            Point16 origin = TileUtils.GetTileOrigin(i, j);

            Point16 key = new Point16(i, j);
            if (TileEntity.ByPosition.TryGetValue(key, out var value) && value.type == ModContent.GetInstance<HopperTileEntity>().Type)
            {
                ((ModTileEntity)value).OnKill();
                TileEntity.ByID.Remove(value.ID);
                TileEntity.ByPosition.Remove(key);
            }
        }
    }

    public class HopperTileEntity : ModTileEntity
    {
        internal bool pickupInCooldown = false;
        internal int pickupTimer = 0;
        internal int checkChestTimer = 0;

        public Chest getChest()
        {
            int chestId = -1;
            return getChest(ref chestId);
        }

        public Chest getChest(ref int chestId)
        {
            chestId = Utils.ChestUtils.FindChest(this.Position.X, this.Position.Y + 1);
            if (chestId == -1)
                return null;
            return Main.chest[chestId];
        }

        // Use when tile entity position not yet defined
        public Chest getChest(int i, int j)
        {
            int chestId = Utils.ChestUtils.FindChest(i, j);
            if (chestId == -1)
                return null;
            return Main.chest[chestId];
        }

        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.HasTile && tile.TileType == ModContent.TileType<HopperTile>() && getChest(i, j + 1) != null;
        }

        public override void Update()
        {
            checkChestTimer++;

            if (checkChestTimer >= 20)
            {
                checkChestTimer = 0;
                Chest chest = getChest();
                if (chest == null)
                {
                    this.Kill(this.Position.X, this.Position.Y);
                    WorldGen.KillTile(this.Position.X, this.Position.Y, false, false, false);
                    return;
                }
            }

            if (pickupInCooldown)
            {
                pickupTimer--;
                if (pickupTimer <= 0)
                    pickupInCooldown = false;
                else
                    return;
            }

            for (int i = 0; i < Main.item.Length; i++)
            {
                if (Main.item[i].active && Main.item[i].noGrabDelay == 0)
                {
                    Item item = Main.item[i];

                    if (PickupArea().Intersects(item.getRect()))
                    {
                        int chestID = -1;
                        Chest chest = getChest(ref chestID);
                        if (chest == null)
                        {
                            this.Kill(this.Position.X, this.Position.Y);
                            WorldGen.KillTile(this.Position.X, this.Position.Y, false, false, false);
                            return;
                        }

                        bool inserted = TAutomation.Instance.getChestUtils().InsertItem(chest, chestID, item, i);
                        if (inserted)
                        {
                            SoundStyle soundStyle = SoundID.MenuTick with { Volume = 0.3f };
                            SoundEngine.PlaySound(soundStyle);
                        }

                        pickupInCooldown = true;
                        pickupTimer = 60;

                        return;
                    }
                }
            }
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 1);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
                return -1;
            }

            int placedEntity = Place(i, j);
            return placedEntity;
        }

        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }

        public Rectangle PickupArea()
        {
            return new Rectangle((Position.X - 1) * 16, (Position.Y - 1) * 16, 48, 16);
        }
    }
}