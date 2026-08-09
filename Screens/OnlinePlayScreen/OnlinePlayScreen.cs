using Constants;
using Enums;
using Godot;
using Helpers;
using Levels;
using Root;
using System.Net;

namespace Screens
{
    public partial class OnlinePlayScreen : Control
    {
        #region Online Properties

        private ENetMultiplayerPeer _peer;

        [Export]
        private int _serverPort = 7777;

        [Export]
        private string _ipAddress = "127.0.0.1"; //"localhost"

        private const int _maxNumberOfPlayers = 2;

        #endregion

        #region Components

        private Timer _inputTimer;
        private Button _createServerButton;
        private Button _joinServerButton;
        private Button _startGameButton;

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
            _createServerButton = FindChild("CreateServerButton") as Button;
            _joinServerButton = FindChild("JoinServerButton") as Button;
            _startGameButton = FindChild("StartGameButton") as Button;

            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ConnectionFailed += OnConnectionFailed;

            _createServerButton.Pressed += OnCreateServer;
            _joinServerButton.Pressed += OnJoinServer;
            _startGameButton.Pressed += OnStartGame;
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
                if (_createServerButton.HasFocus())
                {
                    OnCreateServer();
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
                if (_createServerButton.HasFocus())
                {
                    _joinServerButton.GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.NavigateNorth)))
            {
                if (_joinServerButton.HasFocus())
                {
                    _createServerButton.GrabFocus();
                }

                _inputTimer.Start();
            }
        }

        public void GrabFocusOfTopButton()
        {
            _createServerButton.GrabFocus();
        }

        #region Server Events

        /// <summary>
        /// Runs on all peers, including server
        /// </summary>
        /// <param name="id"></param>
        private void OnPeerConnected(long id)
        {
            GD.Print($"Peer connected: {id}");
        }

        /// <summary>
        /// Runs on all peers, including server
        /// </summary>
        /// <param name="id"></param>
        private void OnPeerDisconnected(long id)
        {
            GD.Print($"Peer disconnected: {id}");
        }

        /// <summary>
        /// Runs only on the clients
        /// </summary>
        private void OnConnectedToServer()
        {
            GD.Print($"Connected to server");
        }

        /// <summary>
        /// Runs only on the clients
        /// </summary>
        private void OnConnectionFailed()
        {
            GD.Print($"Connection failed");
        }

        #endregion

        #region Button Pressed Events

        private void OnCreateServer()
        {
            _peer = new ENetMultiplayerPeer();
            Error error = _peer.CreateServer(_serverPort, _maxNumberOfPlayers);

            if (error != Error.Ok)
            {
                GD.Print($"Failed to create server: {error}");
                return;
            }

            _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

            Multiplayer.MultiplayerPeer = _peer;

            GD.Print("Waiting for players...");
        }

        private void OnJoinServer()
        {
            _peer = new ENetMultiplayerPeer();
            Error error = _peer.CreateClient(_ipAddress, _serverPort);

            if (error != Error.Ok)
            {
                GD.Print($"Failed to create client: {error}");
                return;
            }
            _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

            Multiplayer.MultiplayerPeer = _peer;

            GD.Print("Joining game...");
        }

        private void OnStartGame()
        {
            Rpc(nameof(StartGame));
            StartGame();
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void StartGame()
        {
            BasketballCourtLevel basketballCourtLevel = ResourceLoader.Load<PackedScene>(ScreenFilePaths.BasketballCourtLevelScreenPath).Instantiate<BasketballCourtLevel>();
            GetTree().Root.AddChild(basketballCourtLevel);
            this.Hide();
        }

        #endregion
    }
}
