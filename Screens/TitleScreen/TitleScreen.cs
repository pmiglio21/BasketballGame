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
        private Button _playButton;
        private Button _quitGameButton;

        #region Signals

        [Signal]
        public delegate void GoToPlayScreenEventHandler();

        [Signal]
        public delegate void QuitGameEventHandler();

        #endregion

        public override void _Ready()
        {
            _rootSceneSwapper = GetTree().Root.GetNode<RootSceneSwapper>("RootSceneSwapper");

            _inputTimer = FindChild("InputTimer") as Timer;
            _playButton = FindChild("PlayButton") as Button;
            _playButton.Pressed += OnGoToPlayScreen;
            _quitGameButton = FindChild("QuitGameButton") as Button;
            _quitGameButton.Pressed += OnQuitGame;

            _playButton.GrabFocus();
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
                if (_playButton.HasFocus())
                {
                    OnGoToPlayScreen();
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
                if (_playButton.HasFocus())
                {
                    //_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

                    _quitGameButton.GrabFocus();
                }

                _inputTimer.Start();
            }
            else if (_inputTimer.IsStopped() && (UniversalInputHelper.IsActionPressed(InputType.MoveNorth) || UniversalInputHelper.IsActionPressed_GamePadOnly(InputType.DPadNorth)))
            {
                if (_quitGameButton.HasFocus())
                {
                    //_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiMoveSoundPath);

                    _playButton.GrabFocus();
                }

                _inputTimer.Start();
            }
        }

        public void GrabFocusOfTopButton()
        {
            _playButton.GrabFocus();
        }

        private void OnGoToPlayScreen()
        {
            //_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

            EmitSignal(SignalName.GoToPlayScreen);
        }

        private void OnQuitGame()
        {
            //_rootSceneSwapper.PlayUiSoundEffect(SoundFilePaths.UiButtonSelectSoundPath);

            EmitSignal(SignalName.QuitGame);
        }
    }
}
