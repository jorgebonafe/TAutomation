using System.IO;
using Terraria;
using Terraria.DataStructures;

namespace TAutomation.Interfaces
{
    public interface INetHandler
    {
        void HandlePacket(BinaryReader reader, int WhoAmI);
    }
}