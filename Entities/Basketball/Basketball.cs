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

        public Timer FloorBounceTimer = null;

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

            FloorBounceTimer = GetNode("FloorBounceTimer") as Timer;
        }

        //Necessary for INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == nameof(BasketballState))
            {
                if (BasketballState != BasketballState.IsUpForGrabs)
                {
                    //FloorBounceTimer.WaitTime = _bounceTimerMaxTime;
                    //GD.Print("Floor bounce wait time was reset");
                    _floorBounceCount = 0;

                    _isRolling = false;
                    _rollingCount = 0;
                }
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        #region Shot Motion Properties

        public int ShotAscensionCount = 1;

        #endregion

        #region Floor Bounce Properties

        /// <summary>
        /// Goes up as ball ascends from bounce, slowing the ball as it goes up, then goes down as ball descends from bounce, speeding the ball as it goes down
        /// </summary>
        public int BounceAscensionCount = 1;

        /// <summary>
        /// The highest the floor bounce timer can be set to
        /// </summary>
        private const float _bounceTimerMaxTime = .5f;

        /// <summary>
        /// The lowest the floor bounce timer can be set to
        /// </summary>
        private const float _bounceTimerMinTime = .01f;

        /// <summary>
        /// The constant used to determine new FloorBouncTimer wait time after a bounce
        /// </summary>
        public const float BounceRatioNumber = 15;

        /// <summary>
        /// Number of floor bounces that have occurred in current sequence
        /// </summary>
        private int _floorBounceCount = 0;

        #endregion

        #region Rolling On Floor Properties

        private bool _isRolling = false;
        private int _rollingCount = 0;

        #endregion


        public bool IsCollidingWithFloor = false;

        public override void _PhysicsProcess(double delta)
        {
            if (BasketballState != BasketballState.IsBeingDribbled)
            {
                //GD.Print($"BasketballState: {BasketballState}");
            }

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

                //_bounceCount = 0;
                //BounceAscensionCount = 1;

                MoveAndCollide(LinearVelocity * (float)delta);
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
                    ShotAscensionCount++;

                    LinearVelocity = new Vector3(LinearVelocity.X, (changeInGravity / (float)ShotAscensionCount) * modifier, LinearVelocity.Z);
                }
                //Ball should be falling
                else
                {
                    if (GlobalPosition.Y >= BasketballCourtLevel.HoopArea.GlobalPosition.Y)
                    {
                        if (ShotAscensionCount > 0)
                        {
                            float newYLinearVelocity = Mathf.Clamp(-(changeInGravity / (float)ShotAscensionCount) * modifier, -30f, float.MaxValue);

                            LinearVelocity = new Vector3(LinearVelocity.X, newYLinearVelocity, LinearVelocity.Z);
                            ShotAscensionCount--;
                        }
                    }
                }

                MoveAndCollide(LinearVelocity * (float)delta);
            }
            else if (BasketballState == BasketballState.IsUpForGrabs) //Bouncing on floor or rebounding off basket, etc.
            {
                float changeInGravity = 100f;

                float modifier = 1;

                KinematicCollision3D collisionInfo = MoveAndCollide(LinearVelocity * (float)delta);

                ////Bouncing off of stuff, including rolling
                //if (collisionInfo != null || IsCollidingWithFloor)
                //{
                //    //GD.Print("1");

                //    if (collisionInfo != null && !IsCollidingWithFloor)
                //    {
                //        //GD.Print($"Hit ---------{(collisionInfo.GetCollider() as Node).Name}---------");

                //        //GD.Print($"CollisionInfo's normal is {collisionInfo.GetNormal()}");

                //        LinearVelocity = LinearVelocity.Bounce(collisionInfo.GetNormal());

                //        GD.Print($"Bounced LinearVelocity from option A: {LinearVelocity}");
                //    }
                //    else if (IsCollidingWithFloor)
                //    {
                //        //GD.Print($"++++++++++++++++++++ IsCollidingWithFloor ++++++++++++++++++++");

                //        Vector3 groundNormalVector = new Vector3(0, 1, 0);

                //        LinearVelocity = LinearVelocity.Bounce(groundNormalVector);

                //        GD.Print($"Bounced LinearVelocity from option B: {LinearVelocity}");

                //        _floorBounceCount++;
                //    }

                //    //if (LinearVelocity.Y == -0 && this.GlobalPosition.Y < 1) //If ball is at -0 LinearVelocity and is near the ground
                //    //{
                //    //    GD.Print("Was -0");
                //    //    LinearVelocity = new Vector3(LinearVelocity.X, 1f, LinearVelocity.Z);

                //    //    GD.Print($"Current LinearVelocity {LinearVelocity}");
                //    //}

                //    //GD.Print($"Current LinearVelocity {LinearVelocity}");

                //    BounceAscensionCount = 1;

                //    Vector3 horizontalLinearVelocity = new Vector3(LinearVelocity.X, 0, LinearVelocity.Z);

                //    float rollingSlowDownSpeed = ((float)_floorBounceCount / 100);

                //    if (LinearVelocity.X >= 0 && LinearVelocity.Z >= 0)
                //    {
                //        horizontalLinearVelocity = new Vector3(Mathf.Clamp(LinearVelocity.X - rollingSlowDownSpeed, 0, float.MaxValue), 0, Mathf.Clamp(LinearVelocity.Z - rollingSlowDownSpeed, 0, float.MaxValue));
                //    }
                //    else if (LinearVelocity.X <= 0 && LinearVelocity.Z >= 0)
                //    {
                //        horizontalLinearVelocity = new Vector3(Mathf.Clamp(LinearVelocity.X + rollingSlowDownSpeed, float.MinValue, 0), 0, Mathf.Clamp(LinearVelocity.Z - rollingSlowDownSpeed, 0, float.MaxValue));
                //    }
                //    else if (LinearVelocity.X <= 0 && LinearVelocity.Z <= 0)
                //    {
                //        horizontalLinearVelocity = new Vector3(Mathf.Clamp(LinearVelocity.X + rollingSlowDownSpeed, float.MinValue, 0), 0, Mathf.Clamp(LinearVelocity.Z + rollingSlowDownSpeed, float.MinValue, 0));
                //    }
                //    else if (LinearVelocity.X >= 0 && LinearVelocity.Z <= 0)
                //    {
                //        horizontalLinearVelocity = new Vector3(Mathf.Clamp(LinearVelocity.X - rollingSlowDownSpeed, 0, float.MaxValue), 0, Mathf.Clamp(LinearVelocity.Z + rollingSlowDownSpeed, float.MinValue, 0));
                //    }

                //    if (_floorBounceCount > 0)
                //    {
                //        LinearVelocity = new Vector3(horizontalLinearVelocity.X, Mathf.Clamp(LinearVelocity.Y / (_floorBounceCount * 2), 0, float.MaxValue), horizontalLinearVelocity.Z);
                //    }

                //    //BounceTimer.WaitTime = Mathf.Clamp(BounceTimer.WaitTime * (LinearVelocity.Y / (BounceRatioNumber)), _bounceTimerMinTime, _bounceTimerMaxTime);
                //    FloorBounceTimer.WaitTime = Mathf.Clamp((LinearVelocity.Y / (_floorBounceCount)), _bounceTimerMinTime, _bounceTimerMaxTime);

                //    //GD.Print($"New WaitTime: {BounceTimer.WaitTime}\n");

                //    //if (IsCollidingWithFloor)
                //    //{
                //    //    FloorBounceTimer.Start();
                //    //}

                //    FloorBounceTimer.Start();

                //    IsCollidingWithFloor = false;
                //}
                ////Is in air
                //else if (collisionInfo == null)
                //{
                //    //GD.Print("2");

                //    //Dropping
                //    if (FloorBounceTimer.IsStopped() && FloorBounceTimer.TimeLeft <= 0)
                //    {
                //        if (BounceAscensionCount > 0 && _floorBounceCount > 0)
                //        {
                //            //GD.Print("2A");
                //            float newYLinearVelocity = Mathf.Clamp(-(changeInGravity / (float)(BounceAscensionCount * _floorBounceCount)) * modifier, -30f, float.MaxValue);

                //            LinearVelocity = new Vector3(LinearVelocity.X, newYLinearVelocity, LinearVelocity.Z);
                //            BounceAscensionCount--;
                //        }
                //    }
                //    //Rising
                //    else
                //    {
                //        //GD.Print("2B");
                //        BounceAscensionCount++;

                //        if (BounceAscensionCount > 0 && _floorBounceCount > 0)
                //        {
                //            LinearVelocity = new Vector3(LinearVelocity.X, (changeInGravity / (float)(BounceAscensionCount * _floorBounceCount)) * modifier, LinearVelocity.Z);
                //        }
                //    }
                //}
            }
            else
            {
                if (BasketballState == BasketballState.IsBeingPassed && TargetPlayer != null)   //Used to send ball to player
                {
                    if (TargetPlayer != GetParent() as BasketballPlayer)
                    {
                        var moveInput = GlobalPosition.DirectionTo(TargetPlayer.GlobalPosition);

                        var normalizedMoveInput = moveInput.Normalized();

                        var moveDirection = new Vector3(normalizedMoveInput.X, 0, normalizedMoveInput.Z);

                        LinearVelocity = moveInput * 40f;
                    }
                }

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
                    
                    if (velocity.Length() < MinBounceVelocity)
                    {
                        velocity = Vector3.Zero;
                    }
                }
            }
        }

        private void OnDetectionAreaEntered(Area3D area)
        {
            if (area.IsInGroup(GroupTags.HoopArea))
            {
                ShotAscensionCount = 1;
                BasketballState = BasketballState.IsInBasket;

                GD.Print($"Got into HoopArea.\n" +
                         $"Starting position was {GlobalPositionAtPointOfShot.X}, {GlobalPositionAtPointOfShot.Y}, {GlobalPositionAtPointOfShot.Z}\n" +
                         $"Hoop Area position was {area.GlobalPosition.X}, {area.GlobalPosition.Y}, {area.GlobalPosition.Z}");
            }
        }

        private void OnDetectionAreaBodyEntered(Node3D body)
        {
            if (body.IsInGroup(GroupTags.Bounceable) && BasketballState != BasketballState.IsBeingDribbled)
            {
                //GD.Print($"Entered body. Should feel collision ---------{body.Name}---------");

                //KinematicCollision3D collisionInfo = MoveAndCollide(LinearVelocity);

                //GD.Print($"CollisionInfo at this point is {collisionInfo}");

                ShotAscensionCount = 1;
                BasketballState = BasketballState.IsUpForGrabs;

                //if (body.IsInGroup("Floor"))
                //{
                //    //Vector3 groundNormalVector = new Vector3(0, 1, 0);

                //    //LinearVelocity = LinearVelocity.Bounce(groundNormalVector);

                //    IsCollidingWithFloor = true;
                //}

                //if (!FloorBounceTimer.IsStopped())
                //{
                //    FloorBounceTimer.WaitTime = _bounceTimerMaxTime;
                //}
            }
        }
    }
}
