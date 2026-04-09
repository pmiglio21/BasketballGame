using Constants;
using Enums;
using Godot;
using System.Collections.Generic;

namespace Helpers
{
    public static class UniversalInputHelper
    {
        public static bool IsActionJustPressed(InputType inputType)
        {
            return Input.IsActionJustPressed($"{inputType}_0") || Input.IsActionJustPressed($"{inputType}_1") ||
                    Input.IsActionJustPressed($"{inputType}_Keyboard");
        }

        public static bool IsActionJustPressed_GamePadOnly(InputType inputType)
        {
            return Input.IsActionJustPressed($"{inputType}_0") || Input.IsActionJustPressed($"{inputType}_1");
        }

        public static bool IsActionPressed(InputType inputType)
        {
            return Input.IsActionPressed($"{inputType}_0") || Input.IsActionPressed($"{inputType}_1") ||
                    Input.IsActionPressed($"{inputType}_Keyboard");
        }

        public static bool IsActionPressed_GamePadOnly(InputType inputType)
        {
            return Input.IsActionPressed($"{inputType}_0") || Input.IsActionPressed($"{inputType}_1");
        }

        public static bool IsActionJustReleased(InputType inputType)
        {
            return Input.IsActionJustReleased($"{inputType}_0") || Input.IsActionJustReleased($"{inputType}_1") ||
                    Input.IsActionJustReleased($"{inputType}_Keyboard");
        }

        public static List<string> GetPlayersWhoJustPressedButton(InputType inputType)
        {
            List<string> playersWhoJustPressedButton = new List<string>();

            if (Input.IsActionJustPressed($"{inputType}_0"))
            {
                playersWhoJustPressedButton.Add("0");
            }

            if (Input.IsActionJustPressed($"{inputType}_1"))
            {
                playersWhoJustPressedButton.Add("1");
            }

            if (Input.IsActionJustPressed($"{inputType}_{GlobalConstants.KeyboardDeviceIdentifier}"))
            {
                playersWhoJustPressedButton.Add(GlobalConstants.KeyboardDeviceIdentifier);
            }

            return playersWhoJustPressedButton;
        }
    }
}
