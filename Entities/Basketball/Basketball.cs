using Constants;
using Enums;
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

        public Timer DribbleTimer = null;

        public Timer BounceTimer = null;

        

        #endregion

        #region State Properties

        public BasketballState BasketballState
        {
            get { return _basketballState; }
            set
            {
                if (_basketballState != value)
                {
                    _basketballState = value;
                    OnPropertyChanged(nameof(BasketballState));
                }
            }
        }
        private BasketballState _basketballState;

        #region Shot Properties

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

        public bool IsDestinedToSucceed
        {
            get { return _isDestinedToSucceed; }
            set
            {
                if (_isDestinedToSucceed != value)
                {
                    _isDestinedToSucceed = value;
                    OnPropertyChanged(nameof(IsDestinedToSucceed));
                }
            }
        }
        private bool _isDestinedToSucceed;

        #endregion

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

            DribbleTimer = GetNode("DribbleTimer") as Timer;

            BounceTimer = GetNode("BounceTimer") as Timer;
        }

        //Necessary for INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            //if (propertyName == nameof(Parent))
            //{
               
            //}
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public int ascensionCount = 1;

        public override void _PhysicsProcess(double delta)
        {
            if (BasketballState == BasketballState.IsBeingDribbled)
            {
                if (DribbleTimer.IsStopped() && DribbleTimer.TimeLeft <= 0)
                {
                    Velocity = new Vector3(0, -10f, 0);
                }

                KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

                if (collisionInfo != null)
                {
                    Velocity = Velocity.Bounce(collisionInfo.GetNormal());

                    DribbleTimer.Start();
                }
            }
            else if (BasketballState == BasketballState.IsInBasket)
            {
                Velocity = new Vector3(0, -10f, 0);

                MoveAndSlide();
            }
            else if (BasketballState == BasketballState.IsBeingShot)
            {
                float fullDistanceToTarget = new Vector3(GlobalPositionAtPointOfShot.X - DestinationGlobalPosition.X, 0, GlobalPositionAtPointOfShot.Z - DestinationGlobalPosition.Z).Length();

                float currentDistanceToTarget = new Vector3(GlobalPosition.X - DestinationGlobalPosition.X, 0, GlobalPosition.Z - DestinationGlobalPosition.Z).Length();

                float changeInGravity = 100f;

                float modifier = 1;

                //Ball should be rising
                if (currentDistanceToTarget > fullDistanceToTarget / 2)
                {
                    ascensionCount++;

                    Velocity = new Vector3(Velocity.X, (changeInGravity / (float)ascensionCount) * modifier, Velocity.Z);
                }
                //Ball should be falling
                else
                {
                    if (GlobalPosition.Y >= BasketballCourtLevel.HoopArea.GlobalPosition.Y)
                    {
                        if (ascensionCount > 0)
                        {
                            float newYVelocity = Mathf.Clamp(-(changeInGravity / (float)ascensionCount) * modifier, -30f, float.MaxValue);

                            Velocity = new Vector3(Velocity.X, newYVelocity, Velocity.Z);
                            ascensionCount--;
                        }
                    }
                }

                MoveAndSlide();
            }
            else if (BasketballState == BasketballState.IsBouncingOffBasket)
            {
                //if (BounceTimer.IsStopped() && BounceTimer.TimeLeft <= 0)
                //{
                //    Velocity = new Vector3(Velocity.X, -10f, Velocity.Z);
                //}

                KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

                if (collisionInfo != null)
                {
                    Velocity = Velocity.Bounce(collisionInfo.GetNormal());

                    //Vector3 directionToDestination = GlobalPosition.DirectionTo(DestinationGlobalPosition);

                    //Velocity = new Vector3(directionToDestination.X, Velocity.Y, directionToDestination.Z);

                    //Velocity = new Vector3(Velocity.X, -10f, Velocity.Z);

                    BounceTimer.Start();
                }





                //if (IsDestinedToSucceed)
                //{
                //if (BounceTimer.IsStopped() && BounceTimer.TimeLeft <= 0)
                //{
                //    Velocity = new Vector3(Velocity.X, -10f, Velocity.Z);
                //}

                //KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

                //if (collisionInfo != null)
                //{
                //    Velocity = Velocity.Bounce(collisionInfo.GetNormal());

                //    Vector3 directionToDestination = GlobalPosition.DirectionTo(DestinationGlobalPosition);

                //    Velocity = new Vector3(directionToDestination.X, Velocity.Y, directionToDestination.Z);

                //    //Velocity = new Vector3(Velocity.X, -10f, Velocity.Z);

                //    BounceTimer.Start();
                //}
                //}
                //else
                //{
                //if (BounceTimer.IsStopped() && BounceTimer.TimeLeft <= 0)
                //{
                //    Velocity = new Vector3(Velocity.X, -10f, Velocity.Z);
                //}

                //KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

                //if (collisionInfo != null)
                //{
                //    Velocity = Velocity.Bounce(collisionInfo.GetNormal());

                //    Vector3 directionToDestination = GlobalPosition.DirectionTo(DestinationGlobalPosition);

                //    Velocity = new Vector3(directionToDestination.X, Velocity.Y, directionToDestination.Z);

                //    //Velocity = new Vector3(Velocity.X, -10f, Velocity.Z);

                //    BounceTimer.Start();
                //}
                //}


                //else
                //{
                //    Vector3 directionToDestination = GlobalPosition.DirectionTo(DestinationGlobalPosition);

                //    Velocity = new Vector3(directionToDestination.X, Velocity.Y, directionToDestination.Z);


                //}
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
                ascensionCount = 1;
                BasketballState = BasketballState.IsInBasket;

                GD.Print($"Got into HoopArea.\n" +
                         $"Starting position was {GlobalPositionAtPointOfShot.X}, {GlobalPositionAtPointOfShot.Y}, {GlobalPositionAtPointOfShot.Z}\n" +
                         $"Hoop Area position was {area.GlobalPosition.X}, {area.GlobalPosition.Y}, {area.GlobalPosition.Z}");
            }
        }

        private void OnDetectionAreaBodyEntered(Node3D body)
        {
            if (body.IsInGroup(GroupTags.HoopBody))
            {
                ascensionCount = 1;
                BasketballState = BasketballState.IsBouncingOffBasket;
            }
        }
    }
}
