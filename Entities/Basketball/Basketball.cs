using Godot;
using Levels;
using System;
using System.ComponentModel;

namespace Entities
{
    public partial class Basketball : CharacterBody3D, INotifyPropertyChanged
    {
        #region Player Relations

        public BasketballPlayer PreviousPlayer = null;

        public BasketballPlayer TargetPlayer = null;

        public BasketballCourtLevel BasketballCourtLevel = null;

        #endregion

        #region Components

        public OmniLight3D OmniLight = null;

        public Timer Timer = null;

        #endregion

        #region State Properties

        public bool IsDribbling
        {
            get { return _isDribbling; }
            set
            {
                if (_isDribbling != value)
                {
                    _isDribbling = value;
                    OnPropertyChanged(nameof(IsDribbling));

                    _isBeingShot = false;
                }
            }
        }
        private bool _isDribbling = false;

        public bool IsBeingShot
        {
            get { return _isBeingShot; }
            set
            {
                if (_isBeingShot != value)
                {
                    _isBeingShot = value;
                    OnPropertyChanged(nameof(IsBeingShot));

                    _isDribbling = false;
                }
            }
        }
        private bool _isBeingShot = false;

        public Vector3 GlobalPositionAtPointOfShot
        {
            get { return _globalPositionAtPointOfShot; }
            set
            {
                if (_globalPositionAtPointOfShot != value)
                {
                    _globalPositionAtPointOfShot = value;
                    OnPropertyChanged(nameof(GlobalPositionAtPointOfShot));
                }
            }
        }
        private Vector3 _globalPositionAtPointOfShot = Vector3.Zero;

        #endregion

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Node3D parentNode = GetParent() as Node3D;

            if (parentNode is BasketballPlayer)
            {
                BasketballCourtLevel = parentNode.GetParent() as BasketballCourtLevel;
            }
            else if (parentNode is BasketballCourtLevel)
            {
                BasketballCourtLevel = parentNode as BasketballCourtLevel;
            }

            OmniLight = GetNode("OmniLight3D") as OmniLight3D;

            Timer = GetNode("BounceTimer") as Timer;
        }

        //Necessary for INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            //if (propertyName == nameof(HasFocus))
            //{
            //    if (HasFocus)
            //    {
            //        _hasFocusIndicator.Show();
            //    }
            //    else
            //    {
            //        _hasFocusIndicator.Hide();
            //    }
            //}
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public override void _PhysicsProcess(double delta)
        {
            if (IsDribbling)
            {
                if (Timer.IsStopped() && Timer.TimeLeft <= 0)
                {
                    Velocity = new Vector3(0, -10f, 0);
                }

                KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

                if (collisionInfo != null)
                {
                    Velocity = Velocity.Bounce(collisionInfo.GetNormal());

                    Timer.Start();

                    IsBeingShot = false;
                }

            }
            else if (IsBeingShot)
            {
                float fullDistanceToTarget = new Vector3(GlobalPositionAtPointOfShot.X - BasketballCourtLevel.HoopArea.GlobalPosition.X, 0, GlobalPositionAtPointOfShot.Z - BasketballCourtLevel.HoopArea.GlobalPosition.Z).Length();

                float currentDistanceToTarget = new Vector3(GlobalPosition.X - BasketballCourtLevel.HoopArea.GlobalPosition.X, 0, GlobalPosition.Z - BasketballCourtLevel.HoopArea.GlobalPosition.Z).Length();

                float changeInGravity = 1f;

                //Ball should be rising
                if (currentDistanceToTarget > fullDistanceToTarget/2)
                {
                    Velocity = new Vector3(Velocity.X, Mathf.Clamp(Velocity.Y + changeInGravity, float.MinValue, 5), Velocity.Z);
                }
                //Ball should be falling
                else
                {
                    Velocity = new Vector3(Velocity.X, Mathf.Clamp(Velocity.Y - changeInGravity, float.MinValue, 5), Velocity.Z);
                }

                if (IsOnFloor())
                {
                    IsBeingShot = false;
                    Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
                }

                MoveAndSlide();
            }
            else
            {
                if (TargetPlayer != null)   //Used to send ball to player
                {
                    if (TargetPlayer != GetParent() as BasketballPlayer)
                    {
                        var moveInput = GlobalPosition.DirectionTo(TargetPlayer.GlobalPosition);

                        var normalizedMoveInput = moveInput.Normalized();

                        var moveDirection = new Vector3(normalizedMoveInput.X, 0, normalizedMoveInput.Z);

                        Velocity = moveInput * 40f;
                    }
                }

                MoveAndSlide();
            }
        }
    }
}
