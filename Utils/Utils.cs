using Microsoft.Xna.Framework;
using System.IO;
using TAutomation.Interfaces;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TAutomation.Utils
{
	public static class MiscUtils {
		public static void LogChat(string message)
		{
			Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(150, 250, 150));
		}
	}

	public class ChestUtils: INetHandler
	{
		private Mod mod;

		public ChestUtils(Mod mod)
		{
			this.mod = mod;
			NetRouter.AddHandler(this);
		}

		public static int FindChest(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			if (tile == null || !tile.HasTile)
				return -1;

			int originX = x;
			int originY = y;

			//if (TileLoader.IsDresser(tile.TileType))
			//	originX -= tile.TileFrameX % 54 / 18;
			//else
			//	originX -= tile.TileFrameX % 36 / 18;
			//originY -= tile.TileFrameY % 36 / 18;

			if (!Chest.IsLocked(originX, originY))
				return Chest.FindChest(originX, originY);
			else
				return -1;
		}

		public bool InsertItem(Chest chest, int chestID, Item item, int itemIndex)
		{
			bool injectedPartial = false;

			if (item.maxStack > 1)
            {
                for (int i = 0; i < chest.item.Length; i++)
                {
                    Item chestItem = chest.item[i];
                    if (item.type == chestItem.type && chestItem.stack < chestItem.maxStack)
                    {
                        int spaceLeft = chestItem.maxStack - chestItem.stack;
                        if (spaceLeft >= item.stack)
                        {
							chestItem.stack += item.stack;
							item.stack = 0;
							Main.item[itemIndex] = new Item();

                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);

                            SyncPlayerChest(chestID);
                            return true;
						}
						else
						{
							item.stack -= spaceLeft;
							chestItem.stack = chestItem.maxStack;

							if (Main.netMode == NetmodeID.Server)
								NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
							SyncPlayerChest(chestID);
							injectedPartial = true;
						}
					}
                }
            }

            for (int i = 0; i < chest.item.Length; i++)
			{
				if (chest.item[i].IsAir)
				{
					chest.item[i] = item.Clone();
					item.stack = 0;

					Main.item[itemIndex] = new Item();
					SyncPlayerChest(chestID);

					if (Main.netMode == NetmodeID.Server)
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);
					return true;
				}
			}

			return injectedPartial;
		}

		private void SyncPlayerChest(int chest)
		{
			int player = GetPlayerUsingChest(chest);
			if (player != -1)
			{
				if (Main.netMode == NetmodeID.Server)
				{
					
					ModPacket packet = NetRouter.GetPacketTo(this, mod);
					packet.Send(player);
					Main.player[player].chest = -1;
				}
				else if (Main.netMode == 0)
					Recipe.FindRecipes();
			}
		}

		public static int GetPlayerUsingChest(int chestId)
		{
			for (int i = 0; i < 255; i++)
				if (Main.player[i].chest == chestId)
					return i;
			return -1;
		}

        public void HandlePacket(BinaryReader reader, int WhoAmI)
        {
			Main.LocalPlayer.chest = -1;
			Recipe.FindRecipes();
			SoundEngine.PlaySound(SoundID.MenuClose);
		}
    }


	public static class TileUtils
	{
		/// <summary>
		/// Gets the top-left tile of a multitile
		/// </summary>
		/// <param name="i">The tile X-coordinate</param>
		/// <param name="j">The tile Y-coordinate</param>
		public static Point16 GetTileOrigin(int i, int j)
		{
			//Framing.GetTileSafely ensures that the returned Tile instance is not null
			//Do note that neither this method nor Framing.GetTileSafely check if the wanted coordiates are in the world!
			Tile tile = Framing.GetTileSafely(i, j);

			Point16 coord = new Point16(i, j);
			Point16 frame = new Point16(tile.TileFrameX / 18, tile.TileFrameY / 18);

			return coord - frame;
		}

		/// <summary>
		/// Uses <seealso cref="GetTileOrigin(int, int)"/> to try to get the entity bound to the multitile at (<paramref name="i"/>, <paramref name="j"/>).
		/// </summary>
		/// <typeparam name="T">The type to get the entity as</typeparam>
		/// <param name="i">The tile X-coordinate</param>
		/// <param name="j">The tile Y-coordinate</param>
		/// <param name="entity">The found <typeparamref name="T"/> instance, if there was one.</param>
		/// <returns><see langword="true"/> if there was a <typeparamref name="T"/> instance, or <see langword="false"/> if there was no entity present OR the entity was not a <typeparamref name="T"/> instance.</returns>
		public static bool TryGetTileEntityAs<T>(int i, int j, out T entity) where T : TileEntity
		{
			Point16 origin = GetTileOrigin(i, j);


			//TileEntity.ByPosition is a Dictionary<Point16, TileEntity> which contains all placed TileEntity instances in the world
			//TryGetValue is used to both check if the dictionary has the key, origin, and get the value from that key if it's there
			if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity existing) && existing is T existingAsT)
			{
				entity = existingAsT;
				return true;
			}

			entity = null;
			return false;
		}
	}
}