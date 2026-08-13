using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online
{
    public partial class WebRtcServer : Node
    {
        WebSocketMultiplayerPeer peer;

        public override void _Ready()
        {
            peer = new();
        }

        private void StartServer(string ipAddress)
        {
            peer.CreateServer(8915);
            GD.Print("Started server...");
        }
    }
}
