using System.Collections.Generic;

namespace Archipelago
{
    public class APData : APLastConnectInfo
    {
        public long index;
        public HashSet<long> @checked = new HashSet<long>();
        public bool death_link = false;

        public void FillFromLastConnect(APLastConnectInfo lastConnectInfo)
        {
            host_name = lastConnectInfo.host_name;
            slot_name = lastConnectInfo.slot_name;
            password = lastConnectInfo.password;
        }
        public APLastConnectInfo GetAsLastConnect()
        {
            var lastConnectInfo = new APLastConnectInfo();
            lastConnectInfo.host_name = host_name;
            lastConnectInfo.slot_name = slot_name;
            lastConnectInfo.password = password;
            return lastConnectInfo;
        }
    }
}