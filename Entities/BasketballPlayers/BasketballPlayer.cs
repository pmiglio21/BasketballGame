using Constants;
using Enums;
using Godot;
using Helpers;
using Levels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Entities
{
    public partial class BasketballPlayer : CharacterBody3D, INotifyPropertyChanged
    {
        #region Parents

        public BasketballCourtLevel ParentBasketballCourtLevel = new BasketballCourtLevel();

        #endregion

        #region Components

        private MeshInstance3D _characterBodyMesh = new();
        private StandardMaterial3D _characterBodyMeshMaterial = new StandardMaterial3D();
        private Color _originalCharacterBodyColor = new Color();

        private StaticBody3D _hasFocusIndicator = new();
        
        private StaticBody3D _passTargetIndicator = new();

        private StaticBody3D _shotBlockBody = new();
        private CollisionShape3D _shotBlockCollisionShape = new();

        private Node3D _cpuTargetPivot = new();
        public StaticBody3D CpuTargetBody = new();

        private Node3D _dribblingBallTargetPivot = new();
        private StaticBody3D _dribblingBallTargetBody = new();

        private Timer _jumpAscensionTimer = new();
        private Timer _jumpStartupTimer = new();

        private Timer _stealTimer = new();

        #endregion

        #region Player Identification Properties

        //[Export]
        //public string DeviceIdentifier = "1";

        [Export]
        public string TeamIdentifier = "1";

        [Export]
        public string PlayerIdentifier = "1";

        #endregion

        #region State Properties

        public bool IsOnOffense
        {
            get { return _isOnOffense; }
            set
            {
                if (_isOnOffense != value)
                {
                    _isOnOffense = value;
                    OnPropertyChanged(nameof(IsOnOffense));
                }
            }
        }
        private bool _isOnOffense = false;

        public bool HasFocus
        {
            get { return _hasFocus; }
            set
            {
                if (_hasFocus != value)
                {
                    _hasFocus = value;
                    OnPropertyChanged(nameof(HasFocus));

                    if (_hasFocus)
                    {
                        IsTargeted = false;
                    }
                }
            }
        }
        private bool _hasFocus = false;

        public bool HasBasketball
        {
            get { return _hasBasketball; }
            set
            {
                if (_hasBasketball != value)
                {
                    _hasBasketball = value;
                    OnPropertyChanged(nameof(HasBasketball));

                    if (_hasBasketball)
                    {
                        IsTargeted = false;
                    }
                }
            }
        }
        private bool _hasBasketball = false;

        public PlayerState PlayerState
        {
            get { return _playerState; }
            set
            {
                if (_playerState != value)
                {
                    _playerState = value;
                    OnPropertyChanged(nameof(PlayerState));
                }
            }
        }
        private PlayerState _playerState = PlayerState.IsIdle;

        public bool IsTargeted
        {
            get { return _isTargeted; }
            set
            {
                if (_isTargeted != value)
                {
                    _isTargeted = value;
                    OnPropertyChanged(nameof(IsTargeted));

                    if (_isTargeted)
                    {
                        _passTargetIndicator.Show();
                    }
                    else
                    {
                       _passTargetIndicator.Hide();
                    }
                }
            }
        }
        private bool _isTargeted = false;

        public bool IsInDunkZone
        {
            get { return _isInDunkZone; }
            set
            {
                if (_isInDunkZone != value)
                {
                    _isInDunkZone = value;
                    OnPropertyChanged(nameof(IsInDunkZone));
                }
            }
        }
        private bool _isInDunkZone = false;

        public bool IsInThreePointLine
        {
            get { return _isInThreePointLine; }
            set
            {
                if (_isInThreePointLine != value)
                {
                    _isInThreePointLine = value;
                    OnPropertyChanged(nameof(IsInThreePointLine));
                }
            }
        }
        private bool _isInThreePointLine = false;

        public bool IsBasketballInDetectionArea
        {
            get { return _isBasketballInDetectionArea; }
            set
            {
                if (_isBasketballInDetectionArea != value)
                {
                    _isBasketballInDetectionArea = value;
                    OnPropertyChanged(nameof(IsBasketballInDetectionArea));

                    //GD.Print("Ball is definitely in detection area");
                }
            }
        }
        private bool _isBasketballInDetectionArea = false;

        private bool _isSuperJumpComplete = false;

        #endregion

        #region Skill Properties

        public SkillStats SkillStats = new SkillStats();

        #endregion

        #region Box Score Stats Properties

        public BoxScoreStats BoxScoreStats
        {
            get { return _boxScoreStats; }
            set
            {
                if (_boxScoreStats != value)
                {
                    _boxScoreStats = value;
                    OnPropertyChanged(nameof(BoxScoreStats));
                }
            }
        }
        private BoxScoreStats _boxScoreStats = new BoxScoreStats();

        #endregion 

        #region Movement Properties

        Vector3 moveInput = Vector3.Zero;
        float moveInputDeadzone = 0.1f;

        float moveDeadzone = 0.32f;
        protected Vector3 moveDirection = Vector3.Zero;
        protected float moveAngle = 0;

        private float _standardMovementSpeed = 15.0f;

        bool _isStuckOnFloor = false;

        bool _isHorizontalControlLocked = false;

        #region Jump Properties

        private const float _weakjumpTime = .1f;
        private const float _normaljumpShootingTime = .2f;
        private const float _normaljumpReboundingTime = .25f;
        private const float _normaljumpBlockingTime = .3f;
        private const float _superJumpTime = 1f;
        private int _jumpAscensionCount = 1;
        private bool _isJumpStartupFinished = false;
        private bool _isJumpFinished = true;

        private const float _jumpStartupTimeNoBall = .05f;
        private const float _jumpStartupTimeBlocking = .1f;
        private const float _jumpStartupTimeShooting = .15f;

        #endregion

        #endregion

        #region Pairing Properties

        public BasketballPlayer PairingPlayer = null;

        #endregion

        #region Focus Passing Properties

        public BasketballPlayer TargetPlayer = null;

        #endregion

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ParentBasketballCourtLevel = GetParent() as BasketballCourtLevel;

            _characterBodyMesh = GetNode("CharacterBodyMesh") as MeshInstance3D;

            if (TeamIdentifier == "1")
            {
                StandardMaterial3D blueTeamMaterial = GD.Load<Material>(MaterialPaths.BlueTeamMaterialPath) as StandardMaterial3D;

                _characterBodyMesh.SetSurfaceOverrideMaterial(0, blueTeamMaterial);
            }
            else if (TeamIdentifier == "2")
            {
                StandardMaterial3D redTeamMaterial = GD.Load<Material>(MaterialPaths.RedTeamMaterialPath) as StandardMaterial3D;

                _characterBodyMesh.SetSurfaceOverrideMaterial(0, redTeamMaterial);
            }

            // Ensure material is unique and accessible
            if (_characterBodyMesh != null && _characterBodyMesh.GetSurfaceOverrideMaterial(0) is StandardMaterial3D material)
            {
                _characterBodyMeshMaterial = (StandardMaterial3D)material.Duplicate();
                _characterBodyMesh.SetSurfaceOverrideMaterial(0, _characterBodyMeshMaterial);
                _originalCharacterBodyColor = _characterBodyMeshMaterial.AlbedoColor;
            }

            _hasFocusIndicator = GetNode("HasFocusIndicator") as StaticBody3D;

            _passTargetIndicator = GetNode("PassTargetIndicator") as StaticBody3D;

            _shotBlockBody = GetNode("ShotBlockBody") as StaticBody3D;
            _shotBlockCollisionShape = _shotBlockBody.GetNode("CollisionShape3D") as CollisionShape3D;

            _cpuTargetPivot = GetNode("CpuTargetPivot") as Node3D;
            CpuTargetBody = _cpuTargetPivot.GetNode("CpuTargetBody") as StaticBody3D;

            _dribblingBallTargetPivot = GetNode("DribblingBallTargetPivot") as Node3D;
            _dribblingBallTargetBody = _dribblingBallTargetPivot.GetNode("DribblingBallTargetBody") as StaticBody3D;

            _jumpAscensionTimer = GetNode("JumpAscensionTimer") as Timer;

            _jumpStartupTimer = GetNode("JumpStartupTimer") as Timer;

            _jumpStartupTimer.Timeout += () =>
            {
                _isJumpStartupFinished = true;
            };

            _stealTimer = GetNode("StealTimer") as Timer;

            //TODO: Maybe move this to the next available player by default
            //Start target on the current player so TargetBasketballPlayer has something to go off of on the first target-selection input
            //List<BasketballPlayer> playersOnTeam = ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier && player != this).OrderBy(player => player.PlayerIdentifier).ToList();
            TargetPlayer = this;// playersOnTeam.FirstOrDefault();

            //Offensive players start outside the three point line
            IsInThreePointLine = IsOnOffense;
        }

        //Necessary for INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == nameof(HasFocus))
            {
                if (HasFocus)
                {
                    _hasFocusIndicator.Show();
                }
                else
                {
                    _hasFocusIndicator.Hide();
                }
            }
            else if (propertyName == nameof(HasBasketball))
            {
                if (HasBasketball && PlayerState == PlayerState.IsRebounding && SkillStats.Rebounding == GlobalConstants.SkillStatHigh && !IsOnFloor())
                {
                    _isSuperJumpComplete = true;
                }
            }
            else if (propertyName == nameof(IsOnOffense))
            {
                if (IsOnOffense)
                {
                    ToggleShotBlockBody(false);
                }
            }
        }

        #region Input Handling - Process

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            //Human-controlled logic
            if (HasFocus)
            {
                if (IsOnOffense)
                {
                    GetSkillStatsData();

                    GetMovementInput(delta);

                    GetPassTargetSelectionInput();

                    if (TargetPlayer != this)
                    {
                        if (HasBasketball)
                        {
                            GetPassBallInput();
                        }
                    }

                    if (HasBasketball)
                    {
                        GetShootBasketballInput();
                    }
                }
                else
                {
                    GetSkillStatsData();

                    GetMovementInput(delta);

                    GetPassTargetSelectionInput();

                    if (TargetPlayer != this)
                    {
                        GetPassFocusInput();
                    }

                    if (_stealTimer.IsStopped())
                    {
                        GetStealInput();
                    }
                }
            }
            //else if (IsTargeted)
            //{
            //    GetMovementInput(delta);
            //}
            //CPU logic
            else
            {
                List<BasketballPlayer> playersOnTeam = ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier).ToList();

                BasketballPlayer playerClosestToBasketball = playersOnTeam.OrderBy(player => player.GlobalPosition.DistanceTo(ParentBasketballCourtLevel.Basketball.GlobalPosition)).FirstOrDefault();

                if (ParentBasketballCourtLevel.Basketball.GetParent() is BasketballCourtLevel && !ParentBasketballCourtLevel.Basketball.HasBeenScored && playerClosestToBasketball == this)
                {
                    GoAfterBasketball();
                }
                else
                {
                    if (IsOnOffense)
                    {
                        //TODO: Make offensive CPUs try to get open
                        //MagnetizeCpuToPairedPlayer();

                        moveDirection = new Vector3(0, -10f, 0);
                    }
                    else
                    {
                        MagnetizeCpuToPairedPlayer();
                    }
                }
            }
        }

        #region Controller Inputs

        #region CPU Movement

        protected void MagnetizeCpuToPairedPlayer()
        {
            moveInput = GlobalPosition.DirectionTo(PairingPlayer.CpuTargetBody.GlobalPosition);

            var normalizedMoveInput = moveInput.Normalized();

            moveDirection = new Vector3(normalizedMoveInput.X, -10f, normalizedMoveInput.Z);

            if (this.GlobalPosition.DistanceTo(PairingPlayer.CpuTargetBody.GlobalPosition) <= .5f)
            {
                moveDirection = Vector3.Zero;
            }
        }

        protected void GoAfterBasketball()
        {
            moveInput = GlobalPosition.DirectionTo(ParentBasketballCourtLevel.Basketball.GlobalPosition);

            var normalizedMoveInput = moveInput.Normalized();

            moveDirection = new Vector3(normalizedMoveInput.X, -10f, normalizedMoveInput.Z);
        }

        #endregion

        protected void GetSkillStatsData()
        {
            if (Input.IsActionJustPressed($"ShowSkillStats_{TeamIdentifier}"))
            {
                foreach (BasketballPlayer player in ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier))
                {
                    GD.Print($"Team: {player.TeamIdentifier}, Player: {player.PlayerIdentifier} \n" +
                        $"IsOnOffense: {IsOnOffense}\n"+
                        $"Skill Stats:\n" +
                        $"2PT: {player.SkillStats.TwoPointShooting}\n" +
                        $"3PT: {player.SkillStats.ThreePointShooting}\n" +
                        $"DNK: {player.SkillStats.Dunking}\n" +
                        $"REB: {player.SkillStats.Rebounding}\n" +
                        $"STL: {player.SkillStats.Stealing}\n" +
                        $"BLK: {player.SkillStats.Blocking}\n" +
                        $"HDL: {player.SkillStats.BallHandling}\n" +
                        $"PAS: {player.SkillStats.Passing}\n");
                }
            }
        }

        private const float _minimumFallVelocity = -4f;
        private const float _maximumFallVelocity = -.5f;
        private const float _maximumRiseVelocity = 6f;
        private const float _minimumRiseVelocity = 1f;
        private const float _maximumBlockRiseVelocity = 10f;
        private const float _minimumBlockRiseVelocity = 8f;

        protected void GetMovementInput(double delta)
        {
            var superJumpVelocity = 200.0f; // Adjust jump velocity as needed

            float yMoveInput = 0;

            #region Jumping Logic

            BasketballPlayer playerWithBasketball = ParentBasketballCourtLevel.AllBasketballPlayers.FirstOrDefault(player => player.HasBasketball);

            //bool conditionsForSuperBlockAreMet = SkillStats.Blocking == GlobalConstants.SkillStatHigh && (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotAscending || ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsHeldByAirbornePlayerAndShootable) && playerWithBasketball != null && playerWithBasketball.PlayerState == PlayerState.IsShooting && playerWithBasketball != this && playerWithBasketball != null && PhysicsMathHelper.GetHorizontalDistance(GlobalPosition, playerWithBasketball.GlobalPosition) <= 10;

            bool conditionsForSuperBlockAreMet = SkillStats.Blocking == GlobalConstants.SkillStatHigh && (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotAscending || (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsHeldByAirbornePlayerAndShootable && playerWithBasketball != null && playerWithBasketball.PlayerState == PlayerState.IsShooting && playerWithBasketball != this && playerWithBasketball != null));

            //bool conditionsForSuperReboundAreMet = SkillStats.Rebounding == GlobalConstants.SkillStatHigh && 
            //    (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsReboundable || (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsUpForGrabsOnGround && ParentBasketballCourtLevel.Basketball.GlobalPosition.Y > 2.5f)) && playerWithBasketball != null && PhysicsMathHelper.GetHorizontalDistance(GlobalPosition, ParentBasketballCourtLevel.Basketball.GlobalPosition) <= 10;

            bool conditionsForSuperReboundAreMet = SkillStats.Rebounding == GlobalConstants.SkillStatHigh &&
                (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsReboundable || (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsUpForGrabsOnGround && ParentBasketballCourtLevel.Basketball.GlobalPosition.Y > 2.5f));


            bool conditionsForWeakBlockAreMet = SkillStats.Blocking == GlobalConstants.SkillStatLow && (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotAscending || ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsHeldByAirbornePlayerAndShootable) && playerWithBasketball != this && playerWithBasketball != null && PhysicsMathHelper.GetHorizontalDistance(GlobalPosition, playerWithBasketball.GlobalPosition) <= 10;

            bool conditionsForWeakReboundAreMet = SkillStats.Rebounding == GlobalConstants.SkillStatLow &&
                (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsReboundable || (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsUpForGrabsOnGround && ParentBasketballCourtLevel.Basketball.GlobalPosition.Y > 2.5f)) && PhysicsMathHelper.GetHorizontalDistance(GlobalPosition, ParentBasketballCourtLevel.Basketball.GlobalPosition) <= 10;

            bool conditionsForWeakDunkAreMet = SkillStats.Rebounding == GlobalConstants.SkillStatLow && HasBasketball && IsInDunkZone && ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotAscending;

            bool conditionsForLayupAreMet = SkillStats.Rebounding == GlobalConstants.SkillStatAverage && HasBasketball && IsInDunkZone && ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotAscending;

            bool conditionsForSuperDunkAreMet = SkillStats.Rebounding == GlobalConstants.SkillStatHigh && HasBasketball && IsInDunkZone;


            //Is finished with super jump (ball has been blocked or rebounded) and must descend now
            if (HasFocus && _isSuperJumpComplete)
            {
                yMoveInput = Mathf.Clamp(-GetStandardJumpYValue((float)delta), _minimumFallVelocity, _maximumFallVelocity) * .33f;

                _jumpAscensionCount = Mathf.Clamp(_jumpAscensionCount - 1, 1, int.MaxValue);
            }
            //Is on floor and begins jump startup
            else if (HasFocus && _isJumpFinished && _jumpStartupTimer.IsStopped() && IsOnFloor() && Input.IsActionPressed($"Jump_{TeamIdentifier}"))
            {
                if (HasBasketball)
                {
                    _jumpStartupTimer.WaitTime = _jumpStartupTimeShooting;
                }
                else if (IsAttemptingBlock(playerWithBasketball)) 
                {
                    _jumpStartupTimer.WaitTime = _jumpStartupTimeBlocking;
                    PlayerState = PlayerState.IsBlocking;
                }
                else
                {
                    _jumpStartupTimer.WaitTime = _jumpStartupTimeNoBall;
                }
                
                _jumpStartupTimer.Start();

                if (HasBasketball)
                {
                    ParentBasketballCourtLevel.Basketball.GlobalPosition = GlobalPosition + new Vector3(0, 1.2f, 0);

                    if (IsOnOffense)
                    {
                        PlayerState = PlayerState.IsShooting;
                    }
                }
            }
            //Is on floor still when jump startup finishes but player decides not to continue with jump
            else if (HasFocus && _jumpStartupTimer.IsStopped() && IsOnFloor() && !Input.IsActionPressed($"Jump_{TeamIdentifier}"))
            {
                _isJumpStartupFinished = false;
            }
            //Is on floor and jump startup completes, continuing to jump
            else if (HasFocus && _isJumpStartupFinished && IsOnFloor() && Input.IsActionPressed($"Jump_{TeamIdentifier}"))
            {
                _isJumpFinished = false;

                _isStuckOnFloor = false;

                if (HasBasketball)
                {
                    ParentBasketballCourtLevel.Basketball.BasketballState = BasketballState.IsHeldByAirbornePlayerAndShootable;
                }

                _jumpAscensionCount = 1;

                if (PlayerState == PlayerState.IsBlocking)
                {
                    yMoveInput = GetStandardJumpYValue((float)delta);

                    //Mathf.Clamp(GetStandardJumpYValue((float)delta), _minimumBlockRiseVelocity, _maximumBlockRiseVelocity);

                    //GD.Print($"IsBlocking: {yMoveInput}");
                }
                else
                {
                    yMoveInput = Mathf.Clamp(GetStandardJumpYValue((float)delta), _minimumRiseVelocity, _maximumRiseVelocity);
                    //GD.Print($"Is not Blocking: {yMoveInput}");
                }

                if (conditionsForSuperBlockAreMet || conditionsForSuperReboundAreMet || conditionsForSuperDunkAreMet)
                {
                    _jumpAscensionTimer.WaitTime = _superJumpTime;

                    FlashColor(new Color(1, 1, 1));
                }
                else if (conditionsForWeakBlockAreMet || conditionsForWeakReboundAreMet || conditionsForWeakDunkAreMet)
                {
                    _jumpAscensionTimer.WaitTime = _weakjumpTime;

                    FlashColor(new Color(0, 0, 0));
                }
                else if (PlayerState == PlayerState.IsShooting)
                {
                    _jumpAscensionTimer.WaitTime = _normaljumpShootingTime;
                }
                else if (PlayerState == PlayerState.IsBlocking)
                {
                    _jumpAscensionTimer.WaitTime = _normaljumpBlockingTime;
                }
                else
                {
                    _jumpAscensionTimer.WaitTime = _normaljumpReboundingTime;
                }

                _jumpAscensionTimer.Start();

                //_isHorizontalControlLocked = true;
            }
            //Is in air and continues to hold jump while ascending is still allowed (jumpAscensionTimer is not stopped yet)
            else if (HasFocus && _isJumpStartupFinished && !IsOnFloor() && !_jumpAscensionTimer.IsStopped() && Input.IsActionPressed($"Jump_{TeamIdentifier}"))
            {
                if (!IsOnOffense)
                {
                    ToggleShotBlockBody(true);
                }

                _jumpAscensionCount++;

                yMoveInput = Mathf.Clamp(GetStandardJumpYValue((float)delta), _minimumRiseVelocity, _maximumRiseVelocity);

                if (HasBasketball)
                {
                    ParentBasketballCourtLevel.Basketball.GlobalPosition = GlobalPosition + new Vector3(0, 1.2f, 0);
                }
            }
            //Is in air and jump button is released before ascending is finished
            else if (!IsOnFloor() && !_jumpAscensionTimer.IsStopped() && !Input.IsActionPressed($"Jump_{TeamIdentifier}"))
            {
                //PlayerState = PlayerState.IsIdle;

                yMoveInput = Mathf.Clamp(-GetStandardJumpYValue((float)delta), _minimumFallVelocity, _maximumFallVelocity);

                _jumpAscensionCount = Mathf.Clamp(_jumpAscensionCount - 1, 1, int.MaxValue);
            }
            //Is in air and ascending is finished. Falls whether jump button is held or not
            else if (!IsOnFloor() && _jumpAscensionTimer.IsStopped())
            {
                yMoveInput = Mathf.Clamp(-GetStandardJumpYValue((float)delta), _minimumFallVelocity, _maximumFallVelocity);

                _jumpAscensionCount = Mathf.Clamp(_jumpAscensionCount - 1, 1, int.MaxValue);
            }

            #endregion

            if (HasFocus && !_isStuckOnFloor && !_isHorizontalControlLocked)
            {
                moveInput.X = Input.GetActionStrength($"MoveEast_{TeamIdentifier}") - Input.GetActionStrength($"MoveWest_{TeamIdentifier}");
                moveInput.Z = Input.GetActionStrength($"MoveSouth_{TeamIdentifier}") - Input.GetActionStrength($"MoveNorth_{TeamIdentifier}");
            }
            //else if (IsTargeted)
            //{
            //    float targetedMovementX = Input.GetActionStrength($"MoveTargetEast_{TeamIdentifier}") - Input.GetActionStrength($"MoveTargetWest_{TeamIdentifier}");
            //    float targetedMovementZ = Input.GetActionStrength($"MoveTargetSouth_{TeamIdentifier}") - Input.GetActionStrength($"MoveTargetNorth_{TeamIdentifier}");

            //    //Keep them moving from last movement as long as they aren't standing still
            //    if (targetedMovementX != 0 || targetedMovementZ != 0)
            //    {
            //        moveInput.X = targetedMovementX;
            //        moveInput.Z = targetedMovementZ;
            //    }
            //}

            if (yMoveInput > 0 && conditionsForSuperBlockAreMet)
            {
                Vector3 directionToBall = GlobalPosition.DirectionTo(ParentBasketballCourtLevel.Basketball.GlobalPosition);

                PlayerState = PlayerState.IsBlocking;

                moveDirection = directionToBall * superJumpVelocity * (float)delta;
            }
            else if (yMoveInput > 0 && conditionsForSuperReboundAreMet)
            {
                Vector3 directionToBall = GlobalPosition.DirectionTo(ParentBasketballCourtLevel.Basketball.GlobalPosition);

                PlayerState = PlayerState.IsRebounding;

                moveDirection = directionToBall * superJumpVelocity * (float)delta;
            }
            else if (yMoveInput > 0 && conditionsForSuperDunkAreMet)
            {
                Vector3 directionToHoop = GlobalPosition.DirectionTo(ParentBasketballCourtLevel.BasketballHoop.GlobalPosition);
                directionToHoop = new Vector3(directionToHoop.X, directionToHoop.Y * 2, directionToHoop.Z);

                PlayerState = PlayerState.IsDunking;

                //moveDirection = directionToHoop * (float)delta;

                moveDirection = directionToHoop;
            }
            //TODO: This doesn't work for some reason.
            ////Make them move towards rebound a little more aggressively
            //else if (yMoveInput > 0 && (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsReboundable || ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsReboundable) && ParentBasketballCourtLevel.Basketball.GlobalPosition.DistanceTo(GlobalPosition) <= 100)
            //{
            //    Vector3 directionToBall = GlobalPosition.DirectionTo(ParentBasketballCourtLevel.Basketball.GlobalPosition);

            //    PlayerState = PlayerState.IsRebounding;

            //    moveDirection = directionToBall * yMoveInput * (float)delta;
            //}
            else
            {
                moveDirection = new Vector3(moveInput.X, yMoveInput, moveInput.Z);

                
                
                //Player should move pretty slowly horizontally while shooting a three
                if (ParentBasketballCourtLevel.Basketball.PreviousPlayer == this && 
                    (ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsHeldByAirbornePlayerAndShootable ||
                     ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotAscending) &&
                    !IsInThreePointLine && !IsOnFloor())
                {
                    moveDirection = new Vector3(moveDirection.X / 8, yMoveInput, moveDirection.Z / 8);
                }
                //Player should move a little slowly horizontally while shooting a two
                else if (HasBasketball && yMoveInput > 0)
                {
                    moveDirection = new Vector3(moveDirection.X / 4, yMoveInput, moveDirection.Z / 4);
                }
                else if (Input.IsActionPressed($"Jump_{TeamIdentifier}") && IsOnFloor())
                {
                    moveDirection = new Vector3(moveDirection.X / 2, yMoveInput, moveDirection.Z / 2);
                }
            }

            float horizontalDistanceFromBall = PhysicsMathHelper.GetHorizontalDistance(ParentBasketballCourtLevel.Basketball.GlobalPosition, GlobalPosition);
            
            //If you are near ball, do not have the ball, and are jumping, I'm assuming you are trying to rebound
            if (horizontalDistanceFromBall <= 4 && !HasBasketball && !IsOnFloor())
            {
                PlayerState = PlayerState.IsRebounding;
            }

            if (Input.IsActionJustReleased($"Jump_{TeamIdentifier}"))
            {
                _isJumpFinished = true;

                if (HasBasketball && IsOnFloor())
                {
                    ParentBasketballCourtLevel.Basketball.BasketballState = BasketballState.IsBeingHeldByStationaryPlayer;
                    ParentBasketballCourtLevel.Basketball.GlobalPosition = GlobalPosition + new Vector3(0, 0, 1f);

                    _isStuckOnFloor = true;
                }
            }
        }

        //If you are near player with ball, are on defense, and jumping, I'm assuming you are trying to block them
        private bool IsAttemptingBlock(BasketballPlayer playerWithBasketball)
        {
            if (playerWithBasketball == null)
            {
                return false;
            }
            else
            {
                var distanceBetweenPlayerAndPlayerWithBall = PhysicsMathHelper.GetHorizontalDistance(playerWithBasketball.GlobalPosition, GlobalPosition);

                return distanceBetweenPlayerAndPlayerWithBall <= 10 && !IsOnOffense && IsOnFloor();
            }
        }

        private float GetStandardJumpYValue(float delta)
        {
            float jumpVelocity = 0;

            if (PlayerState == PlayerState.IsBlocking)
            {
                jumpVelocity = 500.0f;
            }
            else
            {
                jumpVelocity = 150.0f;
            }

            return ((jumpVelocity) / (_jumpAscensionCount)) * (float)delta;
        }

        protected void GetShootBasketballInput()
        {
            if (_isJumpStartupFinished && PlayerState != PlayerState.IsRebounding && Input.IsActionJustReleased($"Jump_{TeamIdentifier}"))
            {
                _isStuckOnFloor = false;

                this.HasBasketball = false;

                if (ParentBasketballCourtLevel.Basketball.GetParent() != ParentBasketballCourtLevel)
                {
                    ParentBasketballCourtLevel.Basketball.Reparent(ParentBasketballCourtLevel);
                }

                ParentBasketballCourtLevel.Basketball.BasketballState = BasketballState.IsBeingShotAscending;

                ParentBasketballCourtLevel.Basketball.GlobalPositionAtPointOfShot = ParentBasketballCourtLevel.Basketball.GlobalPosition;


                Vector3 basketballDestinationGlobalPosition = ParentBasketballCourtLevel.Basketball.GlobalPosition;
                Color newBasketballLightColor = ParentBasketballCourtLevel.Basketball.OmniLight.LightColor;


                float yOffset = 1f;

                if (IsInThreePointLine)
                {
                    int chanceOfShotGoingIn = 0;

                    if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatLow)
                    {
                        chanceOfShotGoingIn = 5;

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);
                    }
                    else if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatAverage)
                    {
                        chanceOfShotGoingIn = 35;

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);
                    }
                    else if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatHigh)
                    {
                        chanceOfShotGoingIn = 95;

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);
                    }

                    int randomValue = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(0, 100);

                    if (randomValue <= chanceOfShotGoingIn)
                    {
                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);

                        ParentBasketballCourtLevel.Basketball.IsDestinedToSucceed = true;

                        newBasketballLightColor = new Color(0, 1, 0);
                    }
                    else if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatAverage)
                    {
                        float chanceOfSkew = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(1, 2);

                        float randomXOffset = 0;

                        if (chanceOfSkew == 1)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(1f, 1.5f);
                        }
                        else if (chanceOfSkew == 2)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-1.5f, -1f);
                        }

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, yOffset, 0);

                        ParentBasketballCourtLevel.Basketball.IsDestinedToSucceed = false;

                        newBasketballLightColor = new Color(1, 0, 0);
                    }
                    else if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatLow)
                    {
                        float chanceOfSkew = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(1, 2);

                        float randomXOffset = 0;

                        if (chanceOfSkew == 1)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(1.5f, 3f);
                        }
                        else if (chanceOfSkew == 2)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-3f, -1.5f);
                        }

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, yOffset, 0);

                        ParentBasketballCourtLevel.Basketball.IsDestinedToSucceed = false;

                        newBasketballLightColor = new Color(1, 0, 0);
                    }

                    ParentBasketballCourtLevel.Basketball.PointsExpected = 2;
                }
                else
                {
                    int chanceOfShotGoingIn = 0;

                    if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatLow)
                    {
                        chanceOfShotGoingIn = 1;

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatAverage)
                    {
                        chanceOfShotGoingIn = 25;

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatHigh)
                    {
                        chanceOfShotGoingIn = 80;

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);
                    }

                    int randomValue = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(0, 100);

                    if (randomValue <= chanceOfShotGoingIn)
                    {
                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, yOffset, 0);

                        ParentBasketballCourtLevel.Basketball.IsDestinedToSucceed = true;

                        newBasketballLightColor = new Color(0, 1, 0);
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatAverage)
                    {
                        float chanceOfSkew = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(1, 2);

                        float randomXOffset = 0;

                        if (chanceOfSkew == 1)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(1f, 2f);
                        }
                        else if (chanceOfSkew == 2)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-2f, -1f);
                        }

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, yOffset, 0);

                        ParentBasketballCourtLevel.Basketball.IsDestinedToSucceed = false;

                        newBasketballLightColor = new Color(1, 0, 0);
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatLow)
                    {
                        float chanceOfSkew = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(1, 2);

                        float randomXOffset = 0;

                        if (chanceOfSkew == 1)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(2f, 3f);
                        }
                        else if (chanceOfSkew == 2)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-3f, -2f);
                        }

                        basketballDestinationGlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, yOffset, 0);

                        ParentBasketballCourtLevel.Basketball.IsDestinedToSucceed = false;

                        newBasketballLightColor = new Color(1, 0, 0);
                    }

                    ParentBasketballCourtLevel.Basketball.PointsExpected = 3;
                }

                float ballSpeed = .5f;

                ParentBasketballCourtLevel.Basketball.SetShotAsensionCountModifier(this);

                ParentBasketballCourtLevel.Basketball.LinearVelocity = new Vector3(basketballDestinationGlobalPosition.X - ParentBasketballCourtLevel.Basketball.GlobalPosition.X,
                                                                                   0,
                                                                                   basketballDestinationGlobalPosition.Z - ParentBasketballCourtLevel.Basketball.GlobalPosition.Z) * ballSpeed;

                ParentBasketballCourtLevel.Basketball.DestinationGlobalPosition = basketballDestinationGlobalPosition;

                ParentBasketballCourtLevel.Basketball.OmniLight.LightColor = newBasketballLightColor;

                TargetPlayer = this;

                ParentBasketballCourtLevel.BasketballResetTimer.Start();
            }
        }

        #region Pass Target Input

        protected void GetPassTargetSelectionInput()
        {
            if (Input.IsActionJustPressed($"SelectTarget_{TeamIdentifier}"))
            {
                FindPassTargetPlayer();
            }
        }

        //Not going in any specific direction, just incrementing player number
        private void FindPassTargetPlayer()
        {
            //Reset pass target indicators for all players
            ParentBasketballCourtLevel.AllBasketballPlayers.ForEach(player => player.IsTargeted = false);

            List<BasketballPlayer> availablePlayersToPassTo = ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier && player != this).OrderBy(player => player.PlayerIdentifier).ToList();

            BasketballPlayer nextTargetPlayer = null;
            nextTargetPlayer = availablePlayersToPassTo.FirstOrDefault(player => int.Parse(player.PlayerIdentifier) > int.Parse(TargetPlayer.PlayerIdentifier));

            //Current player is the highest-numbered player, use first numbered player
            if (nextTargetPlayer == null)
            {
                TargetPlayer = availablePlayersToPassTo.First();
            }
            else
            {
               TargetPlayer = nextTargetPlayer;
            }

            TargetPlayer.IsTargeted = true;
        }

        protected void GetPassFocusInput()
        {
            if (Input.IsActionJustPressed($"PassFocus_{TeamIdentifier}"))
            {
                TargetPlayer.HasFocus = true;

                this.HasFocus = false;

                TargetPlayer = this;
            }
        }

        protected void GetPassBallInput()
        {
            if (Input.IsActionJustPressed($"PassFocus_{TeamIdentifier}"))
            {
                if (HasBasketball)
                {
                    ParentBasketballCourtLevel.Basketball.PreviousPlayer = this;
                    ParentBasketballCourtLevel.Basketball.TargetPlayer = TargetPlayer;

                    this.HasBasketball = false;

                    if (ParentBasketballCourtLevel.Basketball.GetParent() != ParentBasketballCourtLevel)
                    {
                        ParentBasketballCourtLevel.Basketball.Reparent(ParentBasketballCourtLevel);
                    }
                    ParentBasketballCourtLevel.Basketball.BasketballState = BasketballState.IsBeingPassed;

                    TargetPlayer.TargetPlayer = this;

                    IsTargeted = true;
                    TargetPlayer = this;
                }
            }
        }

        #endregion

        protected void GetStealInput()
        {
            if (Input.IsActionJustPressed($"StealBall_{TeamIdentifier}"))
            {
                Basketball basketball = ParentBasketballCourtLevel.Basketball;

                IsBasketballInDetectionArea = basketball.GlobalPosition.DistanceTo(GlobalPosition) <= 4.0f;

                if (IsBasketballInDetectionArea)
                {
                    if (basketball.GetParent() is BasketballCourtLevel)
                    {
                        ReceiveTheBall(basketball);

                        GD.Print($"Ball has been stolen by player {PlayerIdentifier}!");
                    }
                    else if (basketball.GetParent() is BasketballPlayer)
                    {
                        BasketballPlayer playerWithBall = basketball.GetParent() as BasketballPlayer;

                        if (playerWithBall.IsOnOffense != IsOnOffense && basketball.BasketballState == BasketballState.IsBeingDribbled)
                        {
                            int chanceOfSuccessfulSteal = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(0, 100);

                            int attemptAtSteal = 0;

                            if (SkillStats.Stealing == GlobalConstants.SkillStatHigh)
                            {
                                attemptAtSteal = 100;
                            }
                            else if (SkillStats.Stealing == GlobalConstants.SkillStatAverage)
                            {
                                attemptAtSteal = 40;
                            }
                            else if (SkillStats.Stealing == GlobalConstants.SkillStatLow)
                            {
                                attemptAtSteal = 10;
                            }

                            if (chanceOfSuccessfulSteal <= attemptAtSteal)
                            {
                                if (playerWithBall.SkillStats.BallHandling == GlobalConstants.SkillStatHigh)
                                {
                                    int chanceOfStealAgain = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(0, 100);

                                    if (chanceOfStealAgain <= 50)
                                    {
                                        ReceiveTheBall(basketball);

                                        FlashColor(new Color(1, 1, 1));
                                    }
                                    else
                                    {
                                        FlashColor(new Color(0, 0, 0));
                                    }
                                }
                                else
                                {
                                    ReceiveTheBall(basketball);

                                    FlashColor(new Color(1, 1, 1));
                                }
                            }
                            else
                            {
                                FlashColor(new Color(0, 0, 0));
                            }
                        }
                    }
                }
                else
                {
                    FlashColor(new Color(0, 0, 0));
                }

                _stealTimer.Start();
            }
        }

        #endregion

        #endregion

        #region Physics Handling - Process

        public override void _PhysicsProcess(double delta)
        {
            MovePlayer();

            RotateCpuTargetBody();

            //TODO: Trying to make shot block body face player that's being blocked. Not working... maybe because ShotBlockBody is static body? idk
            //if (PlayerState == PlayerState.IsBlocking)
            //{
            //    BasketballPlayer playerWithBasketball = ParentBasketballCourtLevel.AllBasketballPlayers.FirstOrDefault(player => player.HasBasketball);

               

            //    if (playerWithBasketball != null)
            //    {
            //        //Vector3 directionToPlayerWithBall = _shotBlockBody.GlobalPosition.DirectionTo(playerWithBasketball.GlobalPosition);

            //        //float newAngle = Mathf.LerpAngle(_shotBlockBody.GlobalRotation.Y, Mathf.Atan2(directionToPlayerWithBall.X, directionToPlayerWithBall.Z), .01f);

            //        //_shotBlockBody.GlobalRotation = new Vector3(_shotBlockBody.GlobalRotation.X, newAngle, _shotBlockBody.GlobalRotation.Z);

            //        //_shotBlockBody.LookAt(playerWithBasketball.GlobalPosition, Vector3.Up);
            //    }
            //    //else
            //    //{
            //    //    _shotBlockBody.LookAt(GlobalPosition, Vector3.Up);
            //    //}

            //    //GD.Print($"ShotBlockBody Rotation Y: {_shotBlockBody.GlobalRotation.Y}");
            //}
        }

        private void MovePlayer()
        {
            float yMoveInput = moveDirection.Y;

            //Fall slower than you rise
            if (yMoveInput < 0)
            {
                yMoveInput = -1;

                //GD.Print("In here");

                //if (PlayerState == PlayerState.IsRebounding || PlayerState == PlayerState.IsBlocking)
                //{
                //    //GD.Print("In here 2");
                //    yMoveInput = -1;

                //    GD.Print($"yMoveInput: {yMoveInput}");
                //}
                //else
                //{
                //    yMoveInput *= .125f;
                //}
            }

            if (SkillStats.BallHandling == GlobalConstants.SkillStatHigh && HasBasketball)
            {
                Velocity = new Vector3(moveDirection.X * _standardMovementSpeed * 1.5f, yMoveInput * _standardMovementSpeed, moveDirection.Z * _standardMovementSpeed * 1.5f);
            }
            else if (SkillStats.BallHandling == GlobalConstants.SkillStatLow && HasBasketball)
            {
                Velocity = new Vector3(moveDirection.X * _standardMovementSpeed * .5f, yMoveInput * _standardMovementSpeed, moveDirection.Z * _standardMovementSpeed * .5f);
            }
            else
            {
                Velocity = new Vector3(moveDirection.X, yMoveInput, moveDirection.Z) * _standardMovementSpeed;
            }

            MoveAndSlide();

            if (moveDirection != Vector3.Zero)
            {
                float newAngle;

                //TODO: Fix rotating to a single direction when shooting, should be aiming towards hoop
                if (PlayerState == PlayerState.IsShooting ||
                    ((ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotAscending || ParentBasketballCourtLevel.Basketball.BasketballState == BasketballState.IsBeingShotDescending) && ParentBasketballCourtLevel.Basketball.PreviousPlayer == this))
                {
                    newAngle = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(ParentBasketballCourtLevel.HoopArea.GlobalPosition.X, ParentBasketballCourtLevel.HoopArea.GlobalPosition.Z), 1f);

                    GlobalRotation = new Vector3(GlobalRotation.X, newAngle, GlobalRotation.Z);
                }
                else if (IsOnFloor())
                {
                    //if ((moveDirection.X != 0 || moveDirection.Z != 0) && SkillStats.BallHandling == GlobalConstants.SkillStatLow && HasBasketball)
                    //{
                    //    int chanceOfLosingBall = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(0, 100);

                    //    if (chanceOfLosingBall <= 1) //1% to lose ball every time they move
                    //    {
                    //        LoseTheBall(ParentBasketballCourtLevel.Basketball);

                    //        FlashColor(new Color(0,0,0));
                    //    }
                    //}
                    //else
                    //{
                        newAngle = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(moveDirection.X, moveDirection.Z), .2f);

                        GlobalRotation = new Vector3(GlobalRotation.X, newAngle, GlobalRotation.Z);
                    //}
                }

                //if (HasBasketball)
                //{
                //    GD.Print($"Move Direction Y is {moveDirection.Y}");
                //}
            }
        }

        private void RotateCpuTargetBody()
        {
            if (IsOnOffense)
            {
                //Get between player and hoop
                if (HasBasketball)
                {
                    Vector3 directionToHoop = GlobalPosition.DirectionTo(ParentBasketballCourtLevel.HoopArea.GlobalPosition);

                    float newAngle = Mathf.LerpAngle(_cpuTargetPivot.GlobalRotation.Y, Mathf.Atan2(directionToHoop.X, directionToHoop.Z), 1f);

                    _cpuTargetPivot.GlobalRotation = new Vector3(_cpuTargetPivot.GlobalRotation.X, newAngle, _cpuTargetPivot.GlobalRotation.Z);
                }
                //Get between player and the player with the ball
                else
                {
                    BasketballPlayer playerWithBall = ParentBasketballCourtLevel.AllBasketballPlayers.FirstOrDefault(x => x.TeamIdentifier == TeamIdentifier && x.IsOnOffense && x.HasBasketball);

                    if (playerWithBall != null)
                    {
                        Vector3 directionToPlayerWithBall = GlobalPosition.DirectionTo(playerWithBall.GlobalPosition);

                        float newAngle = Mathf.LerpAngle(_cpuTargetPivot.GlobalRotation.Y, Mathf.Atan2(directionToPlayerWithBall.X, directionToPlayerWithBall.Z), .1f);

                        _cpuTargetPivot.GlobalRotation = new Vector3(_cpuTargetPivot.GlobalRotation.X, newAngle, _cpuTargetPivot.GlobalRotation.Z);
                    }
                }
            }
        }

        #endregion

        #region Signal Receptions

        private void OnBodyDetectionAreaEntered(Area3D area)
        {
            if (area.IsInGroup(GroupTags.ThreePointLine))
            {
                IsInThreePointLine = true;
            }
            else if (area.IsInGroup(GroupTags.DunkZone))
            {
                IsInDunkZone = true;
            }
        }

        private void OnBodyDetectionAreaExited(Area3D area)
        {
            if (area.IsInGroup(GroupTags.ThreePointLine))
            {
                IsInThreePointLine = false;

                if (IsTargeted)
                {
                    moveInput = Vector3.Zero;
                }
            }
            else if (area.IsInGroup(GroupTags.DunkZone))
            {
                IsInDunkZone = false;
            }
        }

        private void OnBallDetectionAreaEntered(Area3D area)
        {
            if (area.IsInGroup(GroupTags.BasketballDetectionArea))
            {
                Basketball basketball = area.GetParent() as Basketball;

                if (basketball.GetParent() is BasketballCourtLevel)
                {
                    IsBasketballInDetectionArea = true;

                    if (basketball.BasketballState == BasketballState.IsBeingPassed && basketball.PreviousPlayer != this && basketball.TargetPlayer == this)
                    {
                        ReceiveTheBall(basketball);
                    }
                    else if (basketball.BasketballState == BasketballState.IsUpForGrabsOnGround || basketball.BasketballState == BasketballState.IsReboundable)
                    {
                        ReceiveTheBall(basketball);
                    }
                }
            }
        }

        private void OnBallDetectionAreaExited(Area3D area)
        {
            if (area.IsInGroup(GroupTags.BasketballDetectionArea))
            {
                Basketball basketball = area.GetParent() as Basketball;

                if (basketball.GetParent() is BasketballCourtLevel)
                {
                    IsBasketballInDetectionArea = false;
                }
            }
        }

        private void OnBodyDetectionAreaBodyEntered(Node3D body)
        {
            //This all should happen if player touches the ground
            if (body.IsInGroup(GroupTags.Floor))
            {
                if (!_jumpAscensionTimer.IsStopped())
                {
                    _jumpAscensionTimer.Stop();
                }

                if (HasBasketball)
                {
                    ParentBasketballCourtLevel.Basketball.BasketballState = BasketballState.IsBeingDribbled;
                }

                PlayerState = PlayerState.IsIdle;

                ToggleShotBlockBody(false);

                _isJumpStartupFinished = false;
                _isSuperJumpComplete = false;
                _isHorizontalControlLocked = false;
            }
        }

        #endregion

        private bool _isReparenting = false; //Doesn't necessarily work

        private void ReceiveTheBall(Basketball basketball)
        {
            if (!_isReparenting)
            {
                List<BasketballPlayer> playersOnTeam = ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier).ToList();

                foreach (BasketballPlayer player in playersOnTeam)
                {
                    player.HasFocus = false;
                }

                HasFocus = true;
                HasBasketball = true;

        //        if body == player and not is_reparenting:
        //        is_reparenting = true
        //# Reparent logic here
        //var new_parent = get_node("/root/Map2")
        //get_parent().remove_child(self)
        //new_parent.add_child(self)

                if (ParentBasketballCourtLevel.Basketball.GetParent() != this)
                {
                    _isReparenting = true;

                    basketball.Reparent(this);

                    _isReparenting = false;
                }

                basketball.LinearVelocity = Vector3.Zero;

                //Vector3 distanceBetweenPlayerAndBall = new Vector3(0, 0, 1.5f);
                //Vector3 rotatedDistance = distanceBetweenPlayerAndBall.Rotated(Vector3.Up, this.GlobalPosition.Y);
                //basketball.GlobalPosition = this.GlobalPosition + rotatedDistance;

                //basketball.GlobalPosition = this.GlobalPosition + new Vector3(0, 0, 1.5f);

                basketball.GlobalPosition = _dribblingBallTargetBody.GlobalPosition;

                if (IsOnFloor())
                {
                    ParentBasketballCourtLevel.Basketball.BasketballState = BasketballState.IsBeingDribbled;
                }
                else
                {
                    ParentBasketballCourtLevel.Basketball.BasketballState = BasketballState.IsBeingHeldByAirbornePlayer;
                }

                //basketball.TargetPlayer = null;
                basketball.PreviousPlayer = this;

                ParentBasketballCourtLevel.FlipTeamIsOnOffense(TeamIdentifier, true);
            }
        }

        private void LoseTheBall(Basketball basketball)
        {
            if (basketball.GetParent() != ParentBasketballCourtLevel)
            {
                basketball.Reparent(ParentBasketballCourtLevel);
            }

            HasBasketball = false;

            basketball.BasketballState = BasketballState.IsUpForGrabsOnGround;

            basketball.LinearVelocity = new Vector3(ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-2, 2), ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(0, 5), ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-2, 2));

            //Vector3 currentPlayerVelocity = new Vector3(Velocity.X, Velocity.Y, Velocity.Z);

            //basketball.LinearVelocity = currentPlayerVelocity;
        }

        private void ToggleShotBlockBody(bool isVisible)
        {
            if (isVisible)
            {
                _shotBlockBody.CollisionLayer = 1;

                _shotBlockBody.Show();
            }
            else
            {
                _shotBlockBody.CollisionLayer = 0;

                _shotBlockBody.Hide();
            }
        }

        public async void FlashColor(Color newMeshColor)
        {
            if (_characterBodyMeshMaterial == null) return;

            _characterBodyMeshMaterial.AlbedoColor = newMeshColor;
            // Wait for a short duration (e.g., 0.2 seconds)
            await Task.Delay(200);
            _characterBodyMeshMaterial.AlbedoColor = _originalCharacterBodyColor;
        }
    }
}

