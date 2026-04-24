using Enums;
using Godot;
using Helpers;

namespace Screens
{
    public partial class OnlinePlayScreen : Control
    {
        private Timer _inputTimer;

        #region Components

        private Button _createAndPlayOnlineButton;

        #endregion

        #region Signals

        [Signal]
        public delegate void CreateAndPlayOnlineSessionEventHandler();

        #endregion

        public override void _Ready()
        {
            _createAndPlayOnlineButton = FindChild("CreateAndPlayOnlineButton") as Button;
        }

        public override void _Process(double delta)
        {
            GetButtonPressInput();

            //GetNavigationInput();
        }

        private void GetButtonPressInput()
        {
            if (UniversalInputHelper.IsActionJustPressed(InputType.UiActionConfirm))
            {
                if (_createAndPlayOnlineButton.HasFocus())
                {
                    OnCreateAndPlayOnlineSession();
                }
            }
        }

        private void GetNavigationInput()
        {
        }

        public void GrabFocusOfTopButton()
        {
            _createAndPlayOnlineButton.GrabFocus();
        }

        private void OnCreateAndPlayOnlineSession()
        {
            EmitSignal(SignalName.CreateAndPlayOnlineSession);
        }
    }
}
