using Constants;
using Entities;
using Enums;
using Godot;
using Helpers;
using Levels;
using Online;
using Root;
using System.Linq;
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

            //Multiplayer.PeerConnected += OnPeerConnected;
            //Multiplayer.PeerDisconnected += OnPeerDisconnected;
            //Multiplayer.ConnectedToServer += OnConnectedToServer;
            //Multiplayer.ConnectionFailed += OnConnectionFailed;

            //_createServerButton.Pressed += OnCreateServer;
            //_joinServerButton.Pressed += OnJoinServer;
            //_startGameButton.Pressed += OnStartGame;

            if (OS.GetCmdlineArgs().Contains("--server"))
            {
                CreateServer();
            }
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

            GameManager.Players.RemoveAll(player => player.OnlinePeerId == id);

            var players = GetTree().GetNodesInGroup(GroupTags.BasketballPlayer);

            foreach (var player in players)
            {
                if (player is TestBasketballPlayer)
                {
                    TestBasketballPlayer testBasketballPlayer = player as TestBasketballPlayer;

                    if (testBasketballPlayer != null && testBasketballPlayer.OnlinePeerId == id)
                    {
                        player.QueueFree();
                    }
                }
            }
        }

        /// <summary>
        /// Runs only on the clients
        /// </summary>
        private void OnConnectedToServer()
        {
            GD.Print($"Connected to server");
            RpcId(1, nameof(SendPlayerInformationToServer), GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
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
            CreateServer();

            SendPlayerInformationToServer(GetNode<LineEdit>("LineEdit").Text, 1);
        }

        private void CreateServer()
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
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void StartGame()
        {
            foreach (TestBasketballPlayer player in GameManager.Players)
            {
                GD.Print($"{player.OnlineName} : {player.OnlinePeerId} is playing");
            }


            TestBasketballCourtLevel basketballCourtLevel = ResourceLoader.Load<PackedScene>(ScreenFilePaths.TestBasketballCourtLevelScreenPath).Instantiate<TestBasketballCourtLevel>();
            GetTree().Root.AddChild(basketballCourtLevel);
            this.Hide();
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
        private void SendPlayerInformationToServer(string onlineName, int onlinePeerId)
        {
            TestBasketballPlayer basketballPlayer = new TestBasketballPlayer()
            {
                OnlineName = onlineName,
                OnlinePeerId = onlinePeerId
            };

            if (!GameManager.Players.Contains(basketballPlayer))
            {
                GameManager.Players.Add(basketballPlayer);
            }

            if (Multiplayer.IsServer())
            {
                foreach (TestBasketballPlayer player in GameManager.Players)
                {
                    Rpc(nameof(SendPlayerInformationToServer), player.Name, player.OnlinePeerId);
                }
            }
        }

        #endregion
    }
}
