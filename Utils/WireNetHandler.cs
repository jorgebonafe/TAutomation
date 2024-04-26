using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TAutomation.Interfaces;
using Terraria;
using Terraria.ModLoader;

namespace TAutomation.Utils
{
    public class WireNetHandler: INetHandler
    {
        private Mod mod;
        public WireNetHandler(Mod mod)
        {
            this.mod = mod;
            NetRouter.AddHandler(this);
        }

        public void HandlePacket(BinaryReader reader, int WhoAmI)
        {
            int i = reader.ReadInt16();
            int j = reader.ReadInt16();
            Wiring.TripWire(i, j, 1, 1);
        }
    }
}