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
        public Dictionary<string, BasketballPlayer> Players = new Dictionary<string, BasketballPlayer>();

        //Might need to initalize instead
        public Lobby(int hostId)
        {
            HostId = hostId;
        }

        public BasketballPlayer AddPlayer(string playerId)
        {
            Players.Add(playerId, new BasketballPlayer());

            return Players[playerId];
        }
    }
}
