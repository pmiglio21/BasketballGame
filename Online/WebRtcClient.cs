using Godot;
using Godot.Collections;
using Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Online
{
    public partial class WebRtcClient : Node
    {
        WebSocketMultiplayerPeer _peer;
        WebRtcMultiplayerPeer _rtcPeer;
        int _hostId;
        string _lobbyId;

        public override void _Ready()
        {
            _peer = new();
            _rtcPeer = new();

            Multiplayer.ConnectedToServer += OnRtcServerConnected;
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }

        private void OnRtcServerConnected()
        {
            GD.Print("RTC Server Connected");
        }

        private void OnPeerConnected(long playerId)
        {
            GD.Print($"RTC Peer Connected: {playerId}");
        }

        private void OnPeerDisconnected(long playerId)
        {
            GD.Print($"RTC Peer Disconnected: {playerId}");
        }

        public override void _Process(double delta)
        {
            PollAndHandlePackets();
        }

        private void PollAndHandlePackets()
        {
            try
            {
                _peer.Poll();

                if (_peer.GetAvailablePacketCount() > 0)
                {
                    var packet = _peer.GetPacket();

                    if (packet != null)
                    {
                        var dataString = packet.GetStringFromUtf8();

                        //GD.Print($"Packet data: {dataString}");

                        PacketData packetData = Newtonsoft.Json.JsonConvert.DeserializeObject<PacketData>(dataString);

                        //GD.Print($"My id is {packetData.PlayerId}");

                        if (packetData.PacketType == PacketType.PeerConnected)
                        {
                            CreateMeshOnRtcPeer(Int32.Parse(packetData.PlayerId));
                        }
                        else if (packetData.PacketType == PacketType.LobbyJoined)
                        {
                            //GD.Print("Entered CreatePeerConnection()");

                            _hostId = Int32.Parse(packetData.HostId);
                            _lobbyId = packetData.LobbyId;

                            CreatePeerConnection(packetData.PlayerId);
                        }
                        else if (packetData.PacketType == PacketType.SyncLobbyPlayers)
                        {
                            _hostId = Int32.Parse(packetData.HostId);
                            _lobbyId = packetData.LobbyId;

                            GameManager.TestPlayers = packetData.Players;

                            string message = "Current list of players in lobby, sent to client: ";

                            foreach (var player in GameManager.TestPlayers)
                            {
                                message += $"{player.PlayerId}, ";
                            }

                            GD.Print($"{message}");
                        }
                        else if (packetData.PacketType == PacketType.IceCandidateCreated)
                        {
                            if (_rtcPeer.HasPeer(Int32.Parse(packetData.OriginalPeerId)))
                            {
                                GD.Print($"Got Candidate: {packetData.OriginalPeerId}. My id is {_peer.GetUniqueId()}");

                                //This cast probably ain't gonna work but I don't know what it wants
                                WebRtcPeerConnection webRtcConnection = (WebRtcPeerConnection)_rtcPeer.GetPeer(Int32.Parse(packetData.OriginalPeerId))["connection"];
                                webRtcConnection.AddIceCandidate(packetData.IceMedia, (int)packetData.IceIndex, packetData.IceName);
                            }                                                                   
                        }
                        else if (packetData.PacketType == PacketType.SendingOffer)
                        {
                            if (_rtcPeer.HasPeer(Int32.Parse(packetData.OriginalPeerId)))
                            {
                                //This cast probably ain't gonna work but I don't know what it wants
                                WebRtcPeerConnection webRtcConnection = (WebRtcPeerConnection)_rtcPeer.GetPeer(Int32.Parse(packetData.OriginalPeerId))["connection"];
                                webRtcConnection.SetRemoteDescription("offer", packetData.OfferData);
                            }
                        }
                        else if (packetData.PacketType == PacketType.SendingAnswer)
                        {
                            if (_rtcPeer.HasPeer(Int32.Parse(packetData.OriginalPeerId)))
                            {
                                //This cast probably ain't gonna work but I don't know what it wants
                                WebRtcPeerConnection webRtcConnection = (WebRtcPeerConnection)_rtcPeer.GetPeer(Int32.Parse(packetData.OriginalPeerId))["connection"];
                                webRtcConnection.SetRemoteDescription("answer", packetData.OfferData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PushError($"Error in PollAndHandlePackets: {ex.Message}");
            }
        }

        private void CreateMeshOnRtcPeer(int playerId)
        {
            //Make all players connect to each other through _rtcPeer 
            _rtcPeer.CreateMesh(playerId);
            Multiplayer.MultiplayerPeer = _rtcPeer;
        }

        private void CreatePeerConnection(string playerId)
        {
            //
            //Godot debugger has a problem in here
            //
            if (Int32.Parse(playerId) != _peer.GetUniqueId())
            {
                WebRtcPeerConnection peerConnection = new();

                var configuration = new Dictionary
                {
                    { "iceServers", new Godot.Collections.Array
                        {
                            new Dictionary
                            {
                                { "urls", new Godot.Collections.Array { "stun:stun.l.google.com:19302" } }
                            }
                        }
                    }
                };

                //Dictionary<string, object[]> webRtcConfig = new()
                //{
                //    //Test STUN server
                //    { "iceServers", new object[] { new Dictionary<string, object> { { "urls", new string[] { "stun:stun.l.google.com:19302" } } } } }
                //};

                //peer.Initialize({
                //    "iceServers" : [{"urls" : ["stun:stun.l.google.com:19302"]}]
                //});

                peerConnection.Initialize(configuration);

                GD.Print($"Binding id {playerId}. My id is {_peer.GetUniqueId()}");

                peerConnection.SessionDescriptionCreated += (type, sdp) => OfferCreated(type, sdp, playerId);
                peerConnection.IceCandidateCreated += (media, index, name) => IceCandidateCreated(media, index, name, playerId);

                _rtcPeer.AddPeer(peerConnection, Int32.Parse(playerId));

                int peerId = _peer.GetUniqueId();

                if (_hostId != Int32.Parse(playerId))
                {
                    peerConnection.CreateOffer();
                }
            }
        }

        private void OfferCreated(string type, string sdp, string playerId)
        {
            if (!_rtcPeer.HasPeer(Int32.Parse(playerId)))
            {
                return;
            }

            //This cast probably ain't gonna work but I don't know what it wants
            WebRtcPeerConnection webRtcConnection = (WebRtcPeerConnection)_rtcPeer.GetPeer(Int32.Parse(playerId))["connection"];
            webRtcConnection.SetLocalDescription(type, sdp);
            

            if (type == "offer")
            {
                SendOffer(playerId, sdp);
            }
            else
            {
                SendAnswer(playerId, sdp);
            }
        }

        private void SendOffer(string playerId, string offerData)
        {
            PacketData packetData = new()
            {
                PacketType = PacketType.SendingOffer,
                PeerId = playerId,
                OriginalPeerId = _peer.GetUniqueId().ToString(),
                OfferData = offerData,
                LobbyId = _lobbyId
            };

            SendPacketData(packetData);
        }

        private void SendAnswer(string playerId, string answerData)
        {
            PacketData packetData = new()
            {
                PacketType = PacketType.SendingAnswer,
                PeerId = playerId,
                OriginalPeerId = _peer.GetUniqueId().ToString(),
                OfferData = answerData,
                LobbyId = _lobbyId
            };

            SendPacketData(packetData);
        }

        private void IceCandidateCreated(string media, long index, string name, string playerId)
        {
            PacketData packetData = new()
            {
                PacketType = PacketType.IceCandidateCreated,
                PeerId = playerId,
                OriginalPeerId = _peer.GetUniqueId().ToString(),
                IceMedia = media,
                IceIndex = index,
                IceName = name,
                LobbyId = _lobbyId
            };

            SendPacketData(packetData);
        }

        private void ConnectToServer(string ipAddress)
        {
            _peer.CreateClient("ws://127.0.0.1:8915");
            GD.Print("Started client...");
        }

        private void OnJoinServerButtonPressed()
        {
            ConnectToServer("");
        }

        private void OnSendPacketButtonPressed()
        {
            PacketData packetData = new()
            {
                PacketType  = PacketType.TestPacket,
                Message = $"Client {_peer.GetUniqueId()} sending packet to server",
                PlayerId = _peer.GetUniqueId().ToString(),
            };

            SendPacketData(packetData);
        }
       
        private void OnJoinLobbyButtonPressed()
        {
            LineEdit lineEdit = GetParent().GetNode<LineEdit>("LineEdit");

            PacketData packetData = new()
            {
                Message = $"Client {_peer.GetUniqueId()} joining lobby",
                PacketType = PacketType.JoiningLobby,
                PlayerId = _peer.GetUniqueId().ToString(),
                LobbyId = lineEdit.Text
            };

            SendPacketData(packetData);
        }

        private void SendPacketData(PacketData packetData)
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(packetData);

            _peer.PutPacket(jsonData.ToUtf8Buffer());
        }
    }
}
