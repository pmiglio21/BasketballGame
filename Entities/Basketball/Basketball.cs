using Constants;
using Enums;
using Godot;
using Levels;
using System;
using System.ComponentModel;

namespace Entities
{
    public partial class Basketball : RigidBody3D, INotifyPropertyChanged
    {
        #region Player Relations

        public BasketballPlayer PreviousPlayer = null;

        public BasketballPlayer TargetPlayer = null;

        public BasketballCourtLevel BasketballCourtLevel = null;

        #endregion

        #region Components

        public OmniLight3D OmniLight = null;

        public Timer DribbleTimer = null;

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

        public int _shotAscensionCount = 1;

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
        }

        //Necessary for INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            //if (propertyName == nameof(BasketballState))
            //{
                
            //}
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public override void _PhysicsProcess(double delta)
        {
            if (BasketballState == BasketballState.IsBeingDribbled)
            {
                if (DribbleTimer.IsStopped() && DribbleTimer.TimeLeft <= 0)
                {
                    LinearVelocity = new Vector3(0, -3f, 0);
                }

                KinematicCollision3D collisionInfo = MoveAndCollide(LinearVelocity * (float)delta);

                if (collisionInfo != null)
                {
                    LinearVelocity = LinearVelocity.Bounce(collisionInfo.GetNormal());

                    DribbleTimer.Start();
                }
            }
            else if (BasketballState == BasketballState.IsInBasket)
            {
                LinearVelocity = new Vector3(0, -10f, 0);

                MoveAndCollide(LinearVelocity * (float)delta);
            }
            else if (BasketballState == BasketballState.IsBeingShot)
            {
                float fullDistanceToTarget = new Vector3(GlobalPositionAtPointOfShot.X - DestinationGlobalPosition.X, 0, GlobalPositionAtPointOfShot.Z - DestinationGlobalPosition.Z).Length();

                float currentDistanceToTarget = new Vector3(GlobalPosition.X - DestinationGlobalPosition.X, 0, GlobalPosition.Z - DestinationGlobalPosition.Z).Length();

                float changeInGravity = 60f;

                float modifier = 1;

                //Ball should be rising
                if (currentDistanceToTarget > fullDistanceToTarget / 2)
                {
                    _shotAscensionCount++;

                    LinearVelocity = new Vector3(LinearVelocity.X, (changeInGravity / (float)_shotAscensionCount) * modifier, LinearVelocity.Z);
                }
                //Ball should be falling
                else
                {
                    if (GlobalPosition.Y >= BasketballCourtLevel.HoopArea.GlobalPosition.Y)
                    {
                        if (_shotAscensionCount > 0)
                        {
                            float newYLinearVelocity = Mathf.Clamp(-(changeInGravity / (float)_shotAscensionCount) * modifier, -20f, float.MaxValue);

                            LinearVelocity = new Vector3(LinearVelocity.X, newYLinearVelocity, LinearVelocity.Z);
                            _shotAscensionCount--;
                        }
                    }
                }

                MoveAndCollide(LinearVelocity * (float)delta);
            }
            else if (BasketballState == BasketballState.IsBeingPassed)//Used to send ball to player
            {
                if (TargetPlayer != null && TargetPlayer != GetParent() as BasketballPlayer)
                {
                    var moveInput = GlobalPosition.DirectionTo(TargetPlayer.GlobalPosition);

                    var normalizedMoveInput = moveInput.Normalized();

                    var moveDirection = new Vector3(normalizedMoveInput.X, 0, normalizedMoveInput.Z);

                    LinearVelocity = moveInput * 40f;
                }

                MoveAndCollide(LinearVelocity * (float)delta);
            }
            else if (BasketballState == BasketballState.IsUpForGrabs) //Bouncing on floor or rebounding off basket, etc.
            {
                KinematicCollision3D collisionInfo = MoveAndCollide(LinearVelocity * (float)delta);
            }
            else
            {
                MoveAndCollide(LinearVelocity * (float)delta);
            }
        }

        public const float BounceDampeningFactor = .85f;
        public const float MinBounceVelocity = .1f;

        public override void _IntegrateForces(PhysicsDirectBodyState3D state)
        {
            var velocity = state.LinearVelocity;

            //Detect any collision
            if (state.GetContactCount() > 0)
            {
                Vector3 normal = state.GetContactLocalNormal(0);

                //Only adjust if ball is moving into the surface
                if (velocity.Dot(normal) < 0)
                {
                    //Reflect velocity vector
                    velocity = velocity.Bounce(normal) * BounceDampeningFactor;
                    
                    //if (velocity.Length() < MinBounceVelocity)
                    //{
                    //    velocity = Vector3.Zero;
                    //}
                }
            }

             state.LinearVelocity = velocity;
        }

        private void OnDetectionAreaEntered(Area3D area)
        {
            if (area.IsInGroup(GroupTags.HoopArea))
            {
                _shotAscensionCount = 1;
                BasketballState = BasketballState.IsInBasket;

                GD.Print($"Got into HoopArea.\n" +
                         $"Starting position was {GlobalPositionAtPointOfShot.X}, {GlobalPositionAtPointOfShot.Y}, {GlobalPositionAtPointOfShot.Z}\n" +
                         $"Hoop Area position was {area.GlobalPosition.X}, {area.GlobalPosition.Y}, {area.GlobalPosition.Z}");
            }
            else if (area.IsInGroup(GroupTags.ForceShotDownArea))
            {
                if (IsDestinedToSucceed)
                {
                    LinearVelocity = new Vector3(0, -10f, 0);

                    GD.Print($"Got into ForceShotDownArea");
                }
            }
        }

        private void OnDetectionAreaBodyEntered(Node3D body)
        {
            if (body.IsInGroup(GroupTags.Bounceable) && BasketballState != BasketballState.IsBeingDribbled && BasketballState != BasketballState.IsBeingPassed)
            {
                _shotAscensionCount = 1;
                BasketballState = BasketballState.IsUpForGrabs;
            }
        }
    }
}
