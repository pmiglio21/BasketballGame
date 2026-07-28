using Godot;
using Root;
using Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Online
{
    public partial class OnlineNetworkHandler: Node
    {
        private const int _port = 7000;
        private const int _maxConnections = 20;
        private const string _ipAddress = "localhost";

        private RootSceneSwapper _rootSceneSwapper;


        private ENetMultiplayerPeer _peer;

        public override void _Ready()
        {
            _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");
            _rootSceneSwapper.InitializeOnlinePlayScreen();

            _rootSceneSwapper.OnlinePlayScreen.CreateAndPlayOnlineSession += OnCreateAndPlayOnlineSession;
            _rootSceneSwapper.OnlinePlayScreen.JoinOnlineSession += OnJoinOnlineSession;
        }

        public void StartServer()
        {
            _peer = new ENetMultiplayerPeer();
            _peer.CreateServer(_port, _maxConnections);
            Multiplayer.MultiplayerPeer = _peer;
        }

        public void StartClient()
        {
            _peer = new ENetMultiplayerPeer();
            _peer.CreateClient(_ipAddress, _maxConnections);
            Multiplayer.MultiplayerPeer = _peer;
        }

        #region Signal Reception

        private void OnCreateAndPlayOnlineSession()
        {
            GD.Print("Creating and playing online session...");
            //EmitSignal(SignalName.GoToTitleScreen);
        }

        private void OnJoinOnlineSession()
        {
            GD.Print("Creating and playing online session...");
            //EmitSignal(SignalName.GoToTitleScreen);
        }

        #endregion
    }
}
