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
        public Dictionary<string, TestBasketballPlayer> Players = new Dictionary<string, TestBasketballPlayer>();

        //Might need to initalize instead
        public Lobby(int hostId)
        {
            HostId = hostId;
        }

        public TestBasketballPlayer AddPlayer(string playerId)
        {
            Players.Add(playerId, new TestBasketballPlayer());

            return Players[playerId];
        }
    }
}
