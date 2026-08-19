using Constants;
using Entities;
using Godot;
using Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Online
{
    public partial class WebRtcServer : Node
    {
        WebSocketMultiplayerPeer peer;
        List<string> _users = new List<string>();

        public override void _Ready()
        {
            peer = new();

            peer.PeerConnected += OnPeerConnected;
            peer.PeerDisconnected += OnPeerDisconnected;

            //Multiplayer.PeerConnected += OnPeerConnected;
            //Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }

        public override void _Process(double delta)
        {
            peer.Poll();

            if (peer.GetAvailablePacketCount() > 0) 
            {
                var packet = peer.GetPacket();

                if (packet != null)
                {
                    var dataString = packet.GetStringFromUtf8();
                    var data = Json.ParseString(dataString);

                    GD.Print($"Packet data: {data}");
                }
            }
        }

        private void StartServer()
        {
            peer.CreateServer(8915);
            GD.Print("Started server...");
        }

        private void OnCreateServerButtonPressed()
        {
            StartServer();
        }

        /// <summary>
        /// Runs on all peers, including server
        /// </summary>
        /// <param name="id"></param>
        private void OnPeerConnected(long id)
        {
            GD.Print($"Peer connected: {id}");

            _users.Add(id.ToString());

            peer.GetPeer((int)id).PutPacket((Json.Stringify(_users.Last()) + "_ID").ToUtf8Buffer());
        }

        /// <summary>
        /// Runs on all peers, including server
        /// </summary>
        /// <param name="id"></param>
        private void OnPeerDisconnected(long id)
        {
            GD.Print($"Peer disconnected: {id}");

            //GameManager.Players.RemoveAll(player => player.OnlinePeerId == id);

            //var players = GetTree().GetNodesInGroup(GroupTags.BasketballPlayer);

            //foreach (var player in players)
            //{
            //    if (player is TestBasketballPlayer)
            //    {
            //        TestBasketballPlayer testBasketballPlayer = player as TestBasketballPlayer;

            //        if (testBasketballPlayer != null && testBasketballPlayer.OnlinePeerId == id)
            //        {
            //            player.QueueFree();
            //        }
            //    }
            //}
        }
    }
}
