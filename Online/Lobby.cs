using Entities;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online
{
    public partial class Lobby : Node
    {
        public int HostId;
        public Dictionary<string, TestOnlinePlayer> Players = new Dictionary<string, TestOnlinePlayer>();

        //Might need to initalize instead
        public Lobby(int hostId)
        {
            HostId = hostId;
        }

        public TestOnlinePlayer AddPlayer(string playerId)
        {
            TestOnlinePlayer newPlayer = new TestOnlinePlayer()
            {
                PlayerId = playerId
            };

            Players.Add(playerId, newPlayer);

            return Players[playerId];
        }
    }
}
