using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Online
{
    public partial class WebRtcClient : Node
    {
        WebSocketMultiplayerPeer _peer;

        public override void _Ready()
        {
            _peer = new();
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

                    //GD.Print($"Packet data: {dataString}");

                    PacketData packetData = Newtonsoft.Json.JsonConvert.DeserializeObject<PacketData>(dataString);

                    GD.Print($"My id is {packetData.PlayerId}");
                }
            }
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
