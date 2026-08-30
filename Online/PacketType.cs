using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online
{
    public enum PacketType
    {
        PeerConnected,
        PeerDisconnected,
        LobbyJoined,
        JoiningLobby,
        SyncLobbyPlayers,
        TestPacket
    }
}
