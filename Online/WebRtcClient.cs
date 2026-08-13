using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online
{
    public partial class WebRtcClient : Node
    {
        WebSocketMultiplayerPeer peer;

        public override void _Ready()
        {
            peer = new();
        }

        private void ConnectToServer(string ipAddress)
        {
            peer.CreateClient("ws://127.0.0.1:8915");
            GD.Print("Started client...");
        }
    }
}
