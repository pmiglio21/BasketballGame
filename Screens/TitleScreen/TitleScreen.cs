using Enums;
using Constants;
using Godot;
using Helpers;
using Root;

namespace Screens
{
    public partial class TitleScreen : Control
    {
        private RootSceneSwapper _rootSceneSwapper;

        private Timer _inputTimer;
        private Button _localPlayButton;
        private Button _onlinePlayButton;
        private Button _quitGameButton;

        #region Signals

        [Signal]
        public delegate void GoToLocalPlayScreenEventHandler();

        [Signal]
        public delegate void GoToOnlinePlayScreenEventHandler();

        [Signal]
        public delegate void QuitGameEventHandler();

        #endregion

        public override void _Ready()
        {
            _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _inputTimer = FindChild("InputTimer") as Timer;
            _localPlayButton = FindChild("LocalPlayButton") as Button;
            _localPlayButton.Pressed += OnGoToLocalPlayScreen;
            _onlinePlayButton = FindChild("OnlinePlayButton") as Button;
            _onlinePlayButton.Pressed += OnGoToOnlinePlayScreen;
            _quitGameButton = FindChild("QuitGameButton") as Button;
            _quitGameButton.Pressed += OnQuitGame;

            _localPlayButton.GrabFocus();
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
                if (_localPlayButton.HasFocus())
                {
                    OnGoToLocalPlayScreen();
                }
                else if (_onlinePlayButton.HasFocus())
                {
                    OnGoToOnlinePlayScreen();
                }
                else if (_quitGameButton.HasFocus())
                {
                    OnQuitGame();
                }
            }
        }

        private void GetNavigationInput()
        {
            if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveSouth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadSouth)))
            {
                if (_localPlayButton.HasFocus())
                {
                    _onlinePlayButton.GrabFocus();
                }
                else if (_onlinePlayButton.HasFocus())
                {
                    _quitGameButton.GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
            {
                if (_quitGameButton.HasFocus())
                {
                    _onlinePlayButton.GrabFocus();
                }
                else if (_onlinePlayButton.HasFocus())
                {
                    _localPlayButton.GrabFocus();
                }

                _inputTimer.Start();
            }
        }

        public void GrabFocusOfTopButton()
        {
            _localPlayButton.GrabFocus();
        }

        private void OnGoToLocalPlayScreen()
        {
            EmitSignal(SignalName.GoToLocalPlayScreen);
        }

        private void OnGoToOnlinePlayScreen()
        {
            EmitSignal(SignalName.GoToOnlinePlayScreen);
        }

        private void OnQuitGame()
        {
            EmitSignal(SignalName.QuitGame);
        }
    }
}
