using Godot;
using System;

namespace BasketballPlayer
{
    public partial class BasketballPlayer : CharacterBody3D
    {
        #region Components


        #endregion

        #region Movement Properties

        Vector3 moveInput = Vector3.Zero;
        float moveInputDeadzone = 0.1f;

        float moveDeadzone = 0.32f;
        protected Vector3 moveDirection = Vector3.Zero;

        float speed = 10.0f;

        #endregion

        #region Player Identification Properties

        public string DeviceIdentifier = "1";

        #endregion

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            GetMovementInput();
        }

        protected void GetMovementInput()
        {
            moveInput.X = Input.GetActionStrength($"MoveEast_{DeviceIdentifier}") - Input.GetActionStrength($"MoveWest_{DeviceIdentifier}");
            moveInput.Z = Input.GetActionStrength($"MoveSouth_{DeviceIdentifier}") - Input.GetActionStrength($"MoveNorth_{DeviceIdentifier}");

            if (Vector3.Zero.DistanceTo(moveInput) > moveDeadzone * Math.Sqrt(2.0))
            {
                //float speed = (float)((float)(100 + CharacterStats.Speed) / 100);
                var normalizedMoveInput = moveInput.Normalized();

                moveDirection = new Vector3(normalizedMoveInput.X, 0, normalizedMoveInput.Z);
            }
            else
            {
                moveDirection = Vector3.Zero;
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            MovePlayer();
        }

        private void MovePlayer()
        {
            Velocity = moveDirection * speed;

            MoveAndSlide();
        }
    }
}

