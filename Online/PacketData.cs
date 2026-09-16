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

        //Sending offers/answers
        public string PeerId;
        public string OriginalPeerId;
        public string OfferData;

        //Creating ice candidates
        public string IceMedia;
        public long IceIndex;
        public string IceName;
    }
}
