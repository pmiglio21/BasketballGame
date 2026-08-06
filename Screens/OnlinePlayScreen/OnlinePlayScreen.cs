using Enums;
using Godot;
using Helpers;
using Root;
using System.Net;

namespace Screens
{
    public partial class OnlinePlayScreen : Control
    {
        private Timer _inputTimer;

        #region Components

        private Button _createServerAndJoinButton;
        private Button _joinServerButton;

        #endregion

        #region Signals

        [Signal]
        public delegate void CreateAndPlayOnlineSessionEventHandler();

        [Signal]
        public delegate void JoinOnlineSessionEventHandler();

        #endregion

        public override void _Ready()
        {
            _inputTimer = FindChild("InputTimer") as Timer;
            _createServerAndJoinButton = FindChild("CreateServerAndJoinButton") as Button;
            _joinServerButton = FindChild("JoinServerButton") as Button;

            _createServerAndJoinButton.Pressed += OnCreateServerAndJoin;
            _joinServerButton.Pressed += OnJoinServer;
        }

        public override void _Process(double delta)
        {
            GetButtonPressInput();

            GetNavigationInput();
        }

        private void GetButtonPressInput()
        {
            if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionConfirm))
            {
                if (_createServerAndJoinButton.HasFocus())
                {
                    OnCreateServerAndJoin();
                }
                else if (_joinServerButton.HasFocus())
                {
                    OnJoinServer();
                }
            }
        }

        private void GetNavigationInput()
        {
            if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.NavigateSouth)))
            {
                if (_createServerAndJoinButton.HasFocus())
                {
                    _joinServerButton.GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.NavigateNorth)))
            {
                if (_joinServerButton.HasFocus())
                {
                    _createServerAndJoinButton.GrabFocus();
                }

                _inputTimer.Start();
            }
        }

        public void GrabFocusOfTopButton()
        {
            _createServerAndJoinButton.GrabFocus();
        }

        private void OnCreateServerAndJoin()
        {
            //OnlineNetworkHandler.Instance.StartServer();



            //EmitSignal(SignalName.CreateAndPlayOnlineSession);




            //MultiplayerApi multiplayerApi = GetTree().GetMultiplayer(); // Get the default MultiplayerAPI object.

            //// Create client.
            //var peer = new ENetMultiplayerPeer();
            //peer.CreateClient(IPAddress, 12345);
            //Multiplayer.MultiplayerPeer = peer;

            //// Create server.
            //var peer = new ENetMultiplayerPeer();
            //peer.CreateServer(12345, 2);
            //Multiplayer.MultiplayerPeer = peer;

            //Multiplayer.GetUniqueId();

            //Lobby.Instance = new Lobby();

            //Lobby.Instance.CreateGame();
        }

        private void OnJoinServer()
        {
            //OnlineNetworkHandler.Instance.StartClient();



            //EmitSignal(SignalName.JoinOnlineSession);




            //MultiplayerApi multiplayerApi = GetTree().GetMultiplayer(); // Get the default MultiplayerAPI object.

            //// Create client.
            //var peer = new ENetMultiplayerPeer();
            //peer.CreateClient(IPAddress, 12345);
            //Multiplayer.MultiplayerPeer = peer;

            //// Create server.
            //var peer = new ENetMultiplayerPeer();
            //peer.CreateServer(12345, 2);
            //Multiplayer.MultiplayerPeer = peer;

            //Multiplayer.GetUniqueId();

            //Lobby.Instance = new Lobby();

            //Lobby.Instance.CreateGame();
        }
    }
}
