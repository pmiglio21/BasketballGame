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

                    if (packetData.PacketType == PacketType.JoiningLobby)
                    {
                        JoinLobby(Int32.Parse(packetData.PlayerId), packetData.LobbyId);
                    }

                    if (packetData.PacketType == PacketType.SendingOffer || packetData.PacketType == PacketType.SendingAnswer || packetData.PacketType == PacketType.IceCandidateCreated)
                    {
                        //OfferData is probably not a good name to use
                        GD.Print($"Source Id is {packetData.OriginalPeerId}. Message Data {packetData.OfferData}");

                        SendPacketData(packetData, long.Parse(packetData.PeerId));
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

                GD.Print($"Created new lobby: {lobbyId}");
            }

            TestOnlinePlayer newPlayer = _lobbies[lobbyId].AddPlayer(userId.ToString());

            GD.Print($"Client {userId} joining lobby {lobbyId}");

            GD.Print($"Current list of players in lobby: {string.Join(", ", _lobbies[lobbyId].Players.Keys)}");

            foreach (string playerId in _lobbies[lobbyId].Players.Keys)
            {
                PacketData lobbyJoinedPacketData = new PacketData
                {
                    PacketType = PacketType.LobbyJoined,
                    PlayerId = userId.ToString(),
                    HostId = _lobbies[lobbyId].HostId.ToString(),
                    LobbyId = lobbyId,
                };

                SendPacketData(lobbyJoinedPacketData, long.Parse(playerId));


                PacketData lobbyJoinedPacketData2 = new PacketData
                {
                    PacketType = PacketType.LobbyJoined,
                    PlayerId = playerId,
                    HostId = _lobbies[lobbyId].HostId.ToString(),
                    LobbyId = lobbyId,
                };

                SendPacketData(lobbyJoinedPacketData2, userId);

                PacketData syncLobbyPacketData = new PacketData
                {
                    PacketType = PacketType.SyncLobbyPlayers,
                    //Message = $"User {userId} connected to lobby {lobbyId}",
                    PlayerId = userId.ToString(),
                    HostId = _lobbies[lobbyId].HostId.ToString(),
                    LobbyId = lobbyId,
                    Players = _lobbies[lobbyId].Players.Values.ToList(),
                };

                SendPacketData(syncLobbyPacketData, long.Parse(playerId));
            }

            PacketData lobbyJoinedPacketData3 = new PacketData
            {
                PacketType = PacketType.LobbyJoined,
                Message = $"User {userId} connected to lobby {lobbyId}",
                PlayerId = userId.ToString(),
                HostId = _lobbies[lobbyId].HostId.ToString(),
                LobbyId = lobbyId,
                //Player = _lobbies[lobbyId].Players[userId.ToString()]
            };

            SendPacketData(lobbyJoinedPacketData3, userId);

            LineEdit lineEdit = GetParent().GetNode<LineEdit>("LineEdit");
            lineEdit.Text = lobbyId;

            //GD.Print("User connected to lobby!");
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

            GD.Print($"Lobby Key: {result}");

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
                PacketType = PacketType.PeerConnected,
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
