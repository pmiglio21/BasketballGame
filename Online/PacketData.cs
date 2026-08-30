using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online
{
    public class PacketData
    {
        public string Message;
        public PacketType PacketType;
        public string PlayerId;
        public string HostId;
        public string LobbyId;
        public TestOnlinePlayer Player;
        public List<TestOnlinePlayer> Players;
    }
}
