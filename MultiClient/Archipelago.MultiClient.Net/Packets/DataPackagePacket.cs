using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Oculus.Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class DataPackagePacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.DataPackage;

        [JsonProperty("data")]
        public DataPackage DataPackage { get; set; }
    }
}
