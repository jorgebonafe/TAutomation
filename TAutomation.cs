using System.IO;
using TAutomation.Utils;
using Terraria.ModLoader;

namespace TAutomation
{
	public class TAutomation : Mod
	{
		public static TAutomation Instance => ModContent.GetInstance<TAutomation>();

		private ChestUtils chestUtils;
		private WireNetHandler wireNetHandler;

		public override void Load()
		{
			chestUtils = new ChestUtils(this);
			wireNetHandler = new WireNetHandler(this);
			NetRouter.Init(0);
		}

		public ChestUtils getChestUtils()
        {
			return chestUtils;
        }

		public WireNetHandler getWireNetHandler()
		{
			return wireNetHandler;
		}

		public override void Unload()
		{
			NetRouter.Unload();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetRouter.RouteMessage(reader, whoAmI);
		}
	}
}