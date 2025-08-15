using Godot;
using System;

namespace Entities
{
    public partial class Basketball : CharacterBody3D
    {

        public BasketballPlayer PreviousPlayer = null;

        public BasketballPlayer TargetPlayer = null;

        public OmniLight3D OmniLight = null;

        public Timer Timer = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            OmniLight = GetNode("OmniLight3D") as OmniLight3D;

            Timer = GetNode("BounceTimer") as Timer;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public override void _PhysicsProcess(double delta)
        {
            KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

            if (collisionInfo != null)
            {
                Velocity = Velocity.Bounce(collisionInfo.GetNormal());

                Timer.Start();
            }

            if (Timer.IsStopped() && Timer.TimeLeft <= 0)
            {
                Velocity = new Vector3(0, -10f, 0);
            }


            if (TargetPlayer != null)
            {
                var moveInput = GlobalPosition.DirectionTo(TargetPlayer.GlobalPosition);

                var normalizedMoveInput = moveInput.Normalized();

                var moveDirection = new Vector3(normalizedMoveInput.X, 0, normalizedMoveInput.Z);

                Velocity = moveInput * 40f;
            }
        }
    }
}
