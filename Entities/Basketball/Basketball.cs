using Constants;
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

        public Vector3 DestinationGlobalPosition
        {
            get { return _destinationGlobalPosition; }
            set
            {
                if (_destinationGlobalPosition != value)
                {
                    _destinationGlobalPosition = value;
                    OnPropertyChanged(nameof(DestinationGlobalPosition));
                }
            }
        }
        private Vector3 _destinationGlobalPosition = Vector3.Zero;

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

        private int ascensionCount = 0;

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
                float fullDistanceToTarget = new Vector3(GlobalPositionAtPointOfShot.X - DestinationGlobalPosition.X, 0, GlobalPositionAtPointOfShot.Z - DestinationGlobalPosition.Z).Length();

                float currentDistanceToTarget = new Vector3(GlobalPosition.X - DestinationGlobalPosition.X, 0, GlobalPosition.Z - DestinationGlobalPosition.Z).Length();

                float changeInGravity = 50f;

                ////Ball should be rising
                //if (currentDistanceToTarget > fullDistanceToTarget/2)
                //{
                //    Velocity = new Vector3(Velocity.X, Mathf.Clamp(Velocity.Y + changeInGravity, float.MinValue, 5), Velocity.Z);
                //}
                ////Ball should be falling
                //else
                //{
                //    Velocity = new Vector3(Velocity.X, Mathf.Clamp(Velocity.Y - changeInGravity, float.MinValue, 5), Velocity.Z);
                //}

                //Ball should be rising
                if (currentDistanceToTarget > fullDistanceToTarget / 2)
                {
                    ascensionCount++;

                    Velocity = new Vector3(Velocity.X, changeInGravity/ascensionCount, Velocity.Z);
                }
                //Ball should be falling
                else
                {
                    //var newYPosition = Mathf.Lerp(GlobalPosition.Y, DestinationGlobalPosition.Y, .1f);

                    //GlobalPosition = new Vector3(GlobalPosition.X, newYPosition, GlobalPosition.Z);


                    //GlobalPosition = GlobalPosition.Lerp(DestinationGlobalPosition, .01f); 


                    if (GlobalPosition.Y >= BasketballCourtLevel.HoopArea.GlobalPosition.Y)
                    {
                        if (changeInGravity > 0)
                        {
                            Velocity = new Vector3(Velocity.X, -changeInGravity / ascensionCount, Velocity.Z);
                            ascensionCount--;
                        }
                    }
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

        private void OnDetectionAreaEntered(Area3D area)
        {
            if (area.IsInGroup(GroupTags.HoopArea))
            {
                IsBeingShot = false;
                ascensionCount = 0;

                GD.Print($"Got into HoopArea.\n" +
                         $"Starting position was {GlobalPositionAtPointOfShot.X}, {GlobalPositionAtPointOfShot.Y}, {GlobalPositionAtPointOfShot.Z}\n" +
                         $"Hoop Area position was {area.GlobalPosition.X}, {area.GlobalPosition.Y}, {area.GlobalPosition.Z}");
            }
        }
    }
}
