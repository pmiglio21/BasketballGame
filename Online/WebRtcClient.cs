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
        WebSocketMultiplayerPeer peer;

        public override void _Ready()
        {
            peer = new();
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

                    GD.Print($"Packet data: {dataString}");

                    if (dataString.ToString().Contains("_ID"))
                    {
                        GD.Print($"My id is {dataString.Replace("_ID","")}");
                    }
                }
            }
        }

        private void ConnectToServer(string ipAddress)
        {
            peer.CreateClient("ws://127.0.0.1:8915");
            GD.Print("Started client...");
        }

        private void OnJoinServerButtonPressed()
        {
            ConnectToServer("");
        }

        private void OnSendPacketButtonPressed()
        {
            var packetData = new
            {
                message = "Joining server",
                data = "test"
            };

            byte[] packet = JsonSerializer.SerializeToUtf8Bytes(packetData);

            peer.PutPacket(packet);
        }
    }
}
