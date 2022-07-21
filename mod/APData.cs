using System.Collections.Generic;

namespace Archipelago
{
    public class APData
    {
        public long index;
        public string host_name;
        public string slot_name;
        public string password;
        public HashSet<long> @checked = new HashSet<long>();
        public bool death_link = false;
    }
}