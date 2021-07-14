using Archipelago.MultiClient.Net.Enums;
using Oculus.Newtonsoft.Json;

namespace Archipelago.MultiClient.Net.Packets
{
    public class PrintPacket : ArchipelagoPacketBase
    {
        public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.Print;

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
