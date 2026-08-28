using Constants;
using Entities;
using Godot;
using Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Online
{
    public partial class WebRtcServer : Node
    {
        WebSocketMultiplayerPeer _peer;
        List<string> _users = new List<string>();
        Dictionary<string, Lobby> _lobbies = new Dictionary<string, Lobby>();
        string _charactersForLobbyIdGeneration = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        public override void _Ready()
        {
            _peer = new();

            _peer.PeerConnected += OnPeerConnected;
            _peer.PeerDisconnected += OnPeerDisconnected;

            //Multiplayer.PeerConnected += OnPeerConnected;
            //Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }

        public override void _Process(double delta)
        {
            _peer.Poll();

            if (_peer.GetAvailablePacketCount() > 0) 
            {
                var packet = _peer.GetPacket();

                if (packet != null)
                {
                    var dataString = packet.GetStringFromUtf8();

                    PacketData packetData = Newtonsoft.Json.JsonConvert.DeserializeObject<PacketData>(dataString);

                    //GD.Print($"Packet data: {packetData.Message}");

                    //need something better than this - probably an enum
                    if (packetData.Message.Contains("lobby"))
                    {
                        JoinLobby(Int32.Parse(packetData.PlayerId), packetData.LobbyId);
                    }
                }
            }
        }

        private void JoinLobby(int userId, string lobbyId)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
            {
                lobbyId = GenerateRandomLobbyId();
                _lobbies[lobbyId] = new Lobby(userId);
            }

            var player = _lobbies[lobbyId].AddPlayer(userId.ToString());

            PacketData packetData = new PacketData
            {
                Message = $"User {userId} connected to lobby {lobbyId}",
                PlayerId = userId.ToString(),
                HostId = _lobbies[lobbyId].HostId.ToString(),
                Player = _lobbies[lobbyId].Players[userId.ToString()]
            };

            SendPacketData(packetData, userId);
        }

        private string GenerateRandomLobbyId()
        {
            var result = "";

            RandomNumberGenerator rng = new RandomNumberGenerator();

            for (int i = 0; i < 32; i++)
            {
                var index = rng.RandiRange(0, _charactersForLobbyIdGeneration.Length - 1);

                result += _charactersForLobbyIdGeneration[index];
            }

            return result;
        }

        private void StartServer()
        {
            _peer.CreateServer(8915);
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

            PacketData packetData = new PacketData
            {
                Message = "Peer connected",
                PlayerId = _users.Last().ToString()
            };

            SendPacketData(packetData, id);
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

        private void SendPacketData(PacketData packetData, long playerId)
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(packetData);

            _peer.GetPeer((int)playerId).PutPacket(jsonData.ToUtf8Buffer());
        }
    }
}
