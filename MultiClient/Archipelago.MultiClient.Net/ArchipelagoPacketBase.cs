using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using System;

namespace Archipelago.MultiClient.Net
{
    [Serializable]
    public class ArchipelagoPacketBase
    {
        [JsonProperty("cmd")]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ArchipelagoPacketType PacketType { get; set; }
    }
}
