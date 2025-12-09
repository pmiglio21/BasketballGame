using Constants;
using Enums;
using Godot;
using Levels;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

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

        private float _shotAscensionCount = 1;

        private float _shotAscensionCountModifier = 4f;

        public int PointsExpected
        {
            get { return _pointsExpected; }
            set
            {
                if (_pointsExpected != value)
                {
                    _pointsExpected = value;
                    OnPropertyChanged(nameof(PointsExpected));
                }
            }
        }
        private int _pointsExpected;

        #endregion

        #endregion

        #region Scoring Properties

        public bool HasPassedIntoForceShotDownArea //First check
        {
            get { return _hasPassedIntoForceShotDownArea; }
            set
            {
                if (_hasPassedIntoForceShotDownArea != value)
                {
                    _hasPassedIntoForceShotDownArea = value;
                    OnPropertyChanged(nameof(HasPassedIntoForceShotDownArea));
                    OnPropertyChanged(nameof(HasBeenScored));
                }
            }
        }
        private bool _hasPassedIntoForceShotDownArea;

        public bool HasPassedIntoHoopArea //Second check
        {
            get { return _hasPassedIntoHoopArea; }
            set
            {
                if (_hasPassedIntoHoopArea != value)
                {
                    _hasPassedIntoHoopArea = value;
                    OnPropertyChanged(nameof(HasPassedIntoHoopArea));
                    OnPropertyChanged(nameof(HasBeenScored));
                }
            }
        }
        private bool _hasPassedIntoHoopArea;

        public bool HasBeenScored
        {
            get
            {
                return _hasPassedIntoForceShotDownArea && _hasPassedIntoHoopArea;
            }
        }

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
            if (propertyName == nameof(BasketballState))
            {
                if (BasketballState == BasketballState.IsBeingHeldByAirbornePlayer)
                {
                    GravityScale = 0;
                }
                else
                {
                    GravityScale = 1;
                }

                if (BasketballState != BasketballState.IsReboundable)
                {
                    BounceDampeningFactor = MaxBounceDampeningFactor;
                }
            }
            else if (propertyName == nameof(HasBeenScored))
            {
                if (HasBeenScored) 
                {
                    PreviousPlayer.BoxScoreStats.TotalPointsScored += PointsExpected;
                    BasketballCourtLevel.UpdateScoreboard();

                    //BasketballPlayer focusedDefensivePlayer = BasketballCourtLevel.AllBasketballPlayers.FirstOrDefault(player => player.HasFocus && !player.IsOnOffense);
                    //BasketballCourtLevel.GiveBasketballToPlayer(focusedDefensivePlayer);

                    //BasketballCourtLevel.AssignPlayersToStartPoints();
                }
            }
        }

        public override void _Process(double delta)
        {
            //For getting inputs
        }

        public void SetShotAsensionCountModifier(BasketballPlayer shootingPlayer)
        {
            float distanceXBetweenPlayerAndHoop = Mathf.Abs(shootingPlayer.GlobalPosition.X - BasketballCourtLevel.HoopArea.GlobalPosition.X);
            float distanceZBetweenPlayerAndHoop = Mathf.Abs(shootingPlayer.GlobalPosition.Z - BasketballCourtLevel.HoopArea.GlobalPosition.Z);

            if (Mathf.Sqrt(Mathf.Pow(distanceXBetweenPlayerAndHoop,2) + Mathf.Pow(distanceZBetweenPlayerAndHoop, 2)) <= 10)
            {
                _shotAscensionCountModifier = BasketballCourtLevel.RandomNumberGenerator.RandfRange(1.8f, 2.5f);
            }
            else
            {
                _shotAscensionCountModifier = BasketballCourtLevel.RandomNumberGenerator.RandfRange(.5f, 1.2f);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            float changeInGravity = 40f;

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
            else if (BasketballState == BasketballState.IsBeingShotAscending)
            {
                //Shot lower than hoop
                if (GlobalPosition.Y < BasketballCourtLevel.HoopArea.GlobalPosition.Y)
                {
                    LinearVelocity = new Vector3(LinearVelocity.X, (changeInGravity / (float)_shotAscensionCount), LinearVelocity.Z);
                }
                //Makes it to height of hoop, start incrementing shot ascension count
                else
                {
                    float fullDistanceToTarget = new Vector3(GlobalPositionAtPointOfShot.X - DestinationGlobalPosition.X, 0, GlobalPositionAtPointOfShot.Z - DestinationGlobalPosition.Z).Length();

                    float currentDistanceToTarget = new Vector3(GlobalPosition.X - DestinationGlobalPosition.X, 0, GlobalPosition.Z - DestinationGlobalPosition.Z).Length();

                    //Ball should be rising
                    if (currentDistanceToTarget > fullDistanceToTarget / 2)
                    {
                        _shotAscensionCount = _shotAscensionCount + _shotAscensionCountModifier;

                        LinearVelocity = new Vector3(LinearVelocity.X, (changeInGravity / (float)_shotAscensionCount), LinearVelocity.Z);
                    }
                    //Ball should be falling
                    else
                    {
                        BasketballState = BasketballState.IsBeingShotDescending;

                        if (GlobalPosition.Y >= BasketballCourtLevel.HoopArea.GlobalPosition.Y)
                        {
                            if (_shotAscensionCount > 0)
                            {
                                float newYLinearVelocity = Mathf.Clamp(-(changeInGravity / (float)_shotAscensionCount), -20f, float.MaxValue);

                                LinearVelocity = new Vector3(LinearVelocity.X, newYLinearVelocity, LinearVelocity.Z);
                                _shotAscensionCount = _shotAscensionCount - _shotAscensionCountModifier;
                            }
                        }
                    }

                    MoveAndCollide(LinearVelocity * (float)delta);
                }
            }
            else if (BasketballState == BasketballState.IsBeingShotDescending)
            {
                if (GlobalPosition.Y >= BasketballCourtLevel.HoopArea.GlobalPosition.Y)
                {
                    if (_shotAscensionCount > 0)
                    {
                        float newYLinearVelocity = Mathf.Clamp(-(changeInGravity / (float)_shotAscensionCount), -20f, float.MaxValue);

                        LinearVelocity = new Vector3(LinearVelocity.X, newYLinearVelocity, LinearVelocity.Z);
                        _shotAscensionCount = _shotAscensionCount - _shotAscensionCountModifier;
                    }
                }

                MoveAndCollide(LinearVelocity * (float)delta);
            }
            //Used to send ball to player
            else if (BasketballState == BasketballState.IsBeingPassed)
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
            //Bouncing on floor or rebounding off basket, etc.
            else if (BasketballState == BasketballState.IsUpForGrabsOnGround || BasketballState == BasketballState.IsReboundable)
            {
                KinematicCollision3D collisionInfo = MoveAndCollide(LinearVelocity * (float)delta);
            }
            else
            {
                MoveAndCollide(LinearVelocity * (float)delta);
            }
        }

        public const float MaxBounceDampeningFactor = .85f;
        public const float MinBounceDampeningFactor = .1f;
        public float BounceDampeningFactor = .85f;
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
                }
            }

             state.LinearVelocity = velocity;
        }

        private void OnDetectionAreaEntered(Area3D area)
        {
            if (area.IsInGroup(GroupTags.HoopArea))
            {
                if (BasketballState == BasketballState.IsBeingShotDescending || BasketballState == BasketballState.IsReboundable)
                {
                    _shotAscensionCount = 1;
                    BasketballState = BasketballState.IsInBasket;
                }

                HasPassedIntoHoopArea = true;
            }
            else if (area.IsInGroup(GroupTags.ForceShotDownArea))
            {
                HasPassedIntoForceShotDownArea = true;
            }
        }

        private void OnDetectionAreaExited(Area3D area)
        {
            if (area.IsInGroup(GroupTags.HoopArea))
            {
                HasPassedIntoHoopArea = false;
            }
            else if (area.IsInGroup(GroupTags.ForceShotDownArea))
            {
                HasPassedIntoForceShotDownArea = false;
            }
        }

        private void OnDetectionAreaBodyEntered(Node3D body)
        {
            if (body.IsInGroup(GroupTags.HoopBody) && BasketballState != BasketballState.IsBeingDribbled && BasketballState != BasketballState.IsBeingPassed)
            {
                _shotAscensionCount = 1;
                BasketballState = BasketballState.IsReboundable;
            }
            else if (body.IsInGroup(GroupTags.Bounceable) && BasketballState != BasketballState.IsBeingDribbled && BasketballState != BasketballState.IsBeingPassed)
            {
                _shotAscensionCount = 1;
                BasketballState = BasketballState.IsUpForGrabsOnGround;
            }
        }
    }
}
