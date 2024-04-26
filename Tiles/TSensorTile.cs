using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System;
using System.Collections.Generic;
using Terraria.ID;
using TAutomation.Utils;
using Terraria.Audio;

namespace TAutomation.Tiles
{
	public class TSensorTile : ModTile
	{
		public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;

			TileID.Sets.DoesntPlaceWithTileReplacement[ModContent.TileType<Tiles.TSensorTile>()] = true;
			TileID.Sets.AllowsSaveCompressionBatching[ModContent.TileType<Tiles.TSensorTile>()] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.EmptyTile | AnchorType.SolidTile , 1, 0);
            TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TSensorTileEntity>().Hook_AfterPlacement, -1, 0, processedCoordinates: true);

			TileObjectData.addTile(Type);

			AddMapEntry(new Color(0, 254, 0));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			bool on;
			TSensorTileEntity.LogicCheckType logicCheck = TSensorTileEntity.FigureCheckType(i, j, out on);

			switch (logicCheck)
			{
				case TSensorTileEntity.LogicCheckType.Rain:
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<Items.RainSensorItem>());
					break;
				case TSensorTileEntity.LogicCheckType.BloodMoon:
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<Items.BloodMoonSensorItem>());
					break;
			}

			Point16 origin = TileUtils.GetTileOrigin(i, j);
			Point16 key = new Point16(i, j);
			if (TileEntity.ByPosition.TryGetValue(key, out var value) && value.type == ModContent.GetInstance<TSensorTileEntity>().Type)
			{ 
				((ModTileEntity)value).OnKill();
				TileEntity.ByID.Remove(value.ID);
				TileEntity.ByPosition.Remove(key);
			}
		}
	}

	public class TSensorTileEntity : ModTileEntity
	{
		public enum LogicCheckType
		{
			None,
			BloodMoon,
			Rain
		}

		private static List<Point16> tripPoints = new List<Point16>();
		public LogicCheckType logicCheck;
		public bool On;

		public override bool IsTileValidForEntity(int x, int y)
        {
			if (!Main.tile[x, y].HasTile || Main.tile[x, y].TileType != ModContent.TileType<TSensorTile>()
				|| Main.tile[x, y].TileFrameY % 18 != 0 || Main.tile[x, y].TileFrameX % 18 != 0)
				return false;

			return true;
		}

		public override void NetPlaceEntityAttempt(int x, int y)
		{
			NetPlaceEntity(x, y);
		}

		public void NetPlaceEntity(int x, int y)
		{
			int number = Place(x, y);
			TSensorTileEntity tileEntity = ((TSensorTileEntity)TileEntity.ByID[number]);
			tileEntity.FigureCheckState();
			NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, number, x, y);
		}

		public override void Update()
		{
			bool state = GetState(Position.X, Position.Y, logicCheck, this);
			switch (logicCheck)
			{
				case LogicCheckType.Rain:
				case LogicCheckType.BloodMoon:
					if (On != state)
						ChangeState(state, TripWire: true);
					break;
			}

			foreach (Point16 tripPoint in tripPoints)
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Wiring.TripWire(tripPoint.X, tripPoint.Y, 1, 1);
				}
				else
				{
					Wiring.TripWire(tripPoint.X, tripPoint.Y, 1, 1);
					ModPacket packet = NetRouter.GetPacketTo(TAutomation.Instance.getWireNetHandler(), Mod);
					packet.Write((Int16)tripPoint.X);
					packet.Write((Int16)tripPoint.Y);
					packet.Send();
				}
				SoundEngine.PlaySound(SoundID.MenuTick);
			}

			tripPoints.Clear();
		}

		public void ChangeState(bool onState, bool TripWire)
		{
			if (onState == On || SanityCheck(Position.X, Position.Y))
			{
				Main.tile[Position.X, Position.Y].TileFrameX = (short)(onState ? 18 : 0);
				On = onState;
				if (Main.netMode == NetmodeID.Server)
					NetMessage.SendTileSquare(-1, Position.X, Position.Y);

				if (TripWire && Main.netMode != NetmodeID.MultiplayerClient)
					tripPoints.Add(Position);
			}
		}

		public static LogicCheckType FigureCheckType(int x, int y, out bool on)
		{
			on = false;
			if (!WorldGen.InWorld(x, y))
				return LogicCheckType.None;

			Tile tile = Main.tile[x, y];
			if (tile == null)
				return LogicCheckType.None;

			LogicCheckType logicCheckType = LogicCheckType.None;
			switch (tile.TileFrameY / 18)
			{
				case 0:
					logicCheckType = LogicCheckType.BloodMoon;
					break;
				case 1:
					logicCheckType = LogicCheckType.Rain;
					break;
			}

			return logicCheckType;
		}

		public static bool GetState(int x, int y, LogicCheckType type, TSensorTileEntity instance = null)
		{
			switch (type)
			{
				case LogicCheckType.Rain:
					return Main.raining;
				case LogicCheckType.BloodMoon:
					return Main.bloodMoon;
				default:
					return false;
			}
		}

		public void FigureCheckState()
		{
			logicCheck = FigureCheckType(Position.X, Position.Y, out On);
			GetFrame(Position.X, Position.Y, logicCheck, On);
		}

		public static void GetFrame(int x, int y, LogicCheckType type, bool on)
		{
			Main.tile[x, y].TileFrameX = (short)(on ? 18 : 0);
			switch (type)
			{
				case LogicCheckType.BloodMoon:
					Main.tile[x, y].TileFrameY = 0;
					break;
				case LogicCheckType.Rain:
					Main.tile[x, y].TileFrameY = 18;
					break;
				default:
					Main.tile[x, y].TileFrameY = 0;
					break;
			}
		}

		public bool SanityCheck(int x, int y)
		{
			if (!Main.tile[x, y].HasTile || Main.tile[x, y].TileType != ModContent.TileType<TSensorTile>())
			{
				Kill(x, y);
				return false;
			}

			return true;
		}

		public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate)
		{
			bool on;
			LogicCheckType type2 = FigureCheckType(x, y, out on);
			GetFrame(x, y, type2, on);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, x, y, 1, 1);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, x, y, Type);
				return -1;
			}

			int id = Place(x, y);
			((TSensorTileEntity)ByID[id]).OnPlace();
			return id;
		}

        public void OnPlace()
		{
			FigureCheckState();
		}

		public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }

        public override void NetSend(BinaryWriter writer)
		{
			writer.Write((byte)logicCheck);
			writer.Write(On);
		}

		public override void NetReceive(BinaryReader reader)
		{
			logicCheck = (LogicCheckType)reader.ReadByte();
			On = reader.ReadBoolean();
		}

		public override string ToString() => Position.X + "x  " + Position.Y + "y " + logicCheck;
	}
}