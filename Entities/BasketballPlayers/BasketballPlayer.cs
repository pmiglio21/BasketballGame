using Constants;
using Enums;
using Godot;
using Levels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Entities
{
    public partial class BasketballPlayer : CharacterBody3D, INotifyPropertyChanged
    {
        #region Parents

        public BasketballCourtLevel ParentBasketballCourtLevel = new BasketballCourtLevel();

        #endregion

        #region Components

        public MeshInstance3D _characterBodyMesh = new MeshInstance3D();

        private StaticBody3D _hasFocusIndicator = new StaticBody3D();
        
        private StaticBody3D _passTargetIndicator = new StaticBody3D();

        private Timer _jumpTimer = new Timer();

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

        #endregion

        #region Skill Properties


        public SkillStats SkillStats = new SkillStats();

        #endregion

        #region Movement Properties

        Vector3 moveInput = Vector3.Zero;
        float moveInputDeadzone = 0.1f;

        float moveDeadzone = 0.32f;
        protected Vector3 moveDirection = Vector3.Zero;
        protected float moveAngle = 0;

        float speed = 20.0f;

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

            _hasFocusIndicator = GetNode("HasFocusIndicator") as StaticBody3D;

            _passTargetIndicator = GetNode("PassTargetIndicator") as StaticBody3D;

            _jumpTimer = GetNode("JumpTimer") as Timer;

            //Start target on the current player so TargetBasketballPlayer has something to go off of on the first target-selection input
            TargetPlayer = this;
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
                        GetPassFocusInput();
                    }

                    if (HasBasketball)
                    {
                        GetShootBasketballInput();

                        if (TargetPlayer != this)
                        {
                            if (HasBasketball)
                            {
                                GetPassBallInput();
                            }
                        }
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
                }
            }
            //CPU logic
            else
            {
                if (IsOnOffense)
                {
                    MagnetizeCpuToPairedPlayer();
                }
                else
                {
                    MagnetizeCpuToPairedPlayer();
                }
            }
        }

        #region Controller Inputs

        protected void MagnetizeCpuToPairedPlayer()
        {
            moveInput = GlobalPosition.DirectionTo(PairingPlayer.GlobalPosition);

            var normalizedMoveInput = moveInput.Normalized();

            moveDirection = new Vector3(normalizedMoveInput.X, 0, normalizedMoveInput.Z);
        }

        protected void GetSkillStatsData()
        {
            if (Input.IsActionJustPressed($"ShowSkillStats_{TeamIdentifier}"))
            {
                foreach (BasketballPlayer player in ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier))
                {
                    GD.Print($"Team: {player.TeamIdentifier}, Player: {player.PlayerIdentifier} \nSkill Stats:\n" +
                        $"2PT: {player.SkillStats.TwoPointShooting}\n" +
                        $"3PT: {player.SkillStats.ThreePointShooting}\n" +
                        $"DNK: {player.SkillStats.Dunking}\n" +
                        $"REB: {player.SkillStats.Rebounding}\n" +
                        $"STL: {player.SkillStats.Stealing}\n" +
                        $"BLK: {player.SkillStats.Blocking}\n" +
                        $"HDL: {player.SkillStats.BallHandling}\n" +
                        $"SPD: {player.SkillStats.Speed}\n");
                }
            }
        }

        protected void GetMovementInput(double delta)
        {
            var gravity = 80.0f; // Adjust gravity as needed
            var jumpVelocity = 200.0f; // Adjust jump velocity as needed

            float yMoveInput = 0;

            #region Jumping Logic

            //Is on floor and begins to jump
            if (IsOnFloor() && _jumpTimer.IsStopped() && Input.IsActionPressed($"Jump_{TeamIdentifier}"))
            {
                //GD.Print("1");

                yMoveInput = jumpVelocity * (float)delta;
                _jumpTimer.Start();
            }

            // Apply gravity if not on floor
            else if (!IsOnFloor() && _jumpTimer.IsStopped())
            {
                //GD.Print("2");

                yMoveInput = -gravity * (float)delta;
            }

            //Is in air and continues to hold jump
            else if (!IsOnFloor() && !_jumpTimer.IsStopped() && Input.IsActionPressed($"Jump_{TeamIdentifier}"))
            {
                //GD.Print("3");

                yMoveInput = jumpVelocity * (float)delta;
            }

            #endregion

            moveInput.X = Input.GetActionStrength($"MoveEast_{TeamIdentifier}") - Input.GetActionStrength($"MoveWest_{TeamIdentifier}");
            moveInput.Z = Input.GetActionStrength($"MoveSouth_{TeamIdentifier}") - Input.GetActionStrength($"MoveNorth_{TeamIdentifier}");

            //TODO: Do I need this if statement?
            if (Vector3.Zero.DistanceTo(moveInput) > moveDeadzone * Math.Sqrt(2.0))
            {
                //float speed = (float)((float)(100 + CharacterStats.Speed) / 100);
                //var normalizedMoveInput = moveInput.Normalized();

                var normalizedMoveInput = moveInput;

                moveDirection =  new Vector3(normalizedMoveInput.X, yMoveInput, normalizedMoveInput.Z);
            }
            else
            {
                //var normalizedMoveInput = moveInput.Normalized();

                var normalizedMoveInput = moveInput;

                moveDirection = new Vector3(normalizedMoveInput.X, yMoveInput, normalizedMoveInput.Z);
            }
        }

        protected void GetShootBasketballInput()
        {
            //TODO: Maybe do something with IsOnFloor() here?
            if (Input.IsActionJustReleased($"Jump_{TeamIdentifier}"))
            {
                GD.Print($"ShootBall triggered by player {PlayerIdentifier}");

                this.HasBasketball = false;

                ParentBasketballCourtLevel.Basketball.Reparent(ParentBasketballCourtLevel.HoopArea);

                if (IsInThreePointLine)
                {
                    int chanceOfShotGoingIn = 0;

                    if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatLow)
                    {
                        chanceOfShotGoingIn = 5;

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);
                    }
                    else if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatAverage)
                    {
                        chanceOfShotGoingIn = 35;

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);
                    }
                    else if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatHigh)
                    {
                        chanceOfShotGoingIn = 95;

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);
                    }

                    int randomValue = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(0, 100);

                    if (randomValue <= chanceOfShotGoingIn)
                    {
                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);

                        ParentBasketballCourtLevel.Basketball.OmniLight.LightColor = new Color(0, 1, 0);
                    }
                    else if (SkillStats.TwoPointShooting == GlobalConstants.SkillStatAverage)
                    {
                        float chanceOfSkew = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(1, 2);

                        float randomXOffset = 0;

                        if (chanceOfSkew == 1)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(.5f, 1f);
                        }
                        else if (chanceOfSkew == 2)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-1f, -.5f);
                        }

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, 1f, 0);

                        ParentBasketballCourtLevel.Basketball.OmniLight.LightColor = new Color(1, 0, 0);
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

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, 1f, 0);

                        ParentBasketballCourtLevel.Basketball.OmniLight.LightColor = new Color(1, 0, 0);
                    }
                }
                else
                {
                    int chanceOfShotGoingIn = 0;

                    if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatLow)
                    {
                        chanceOfShotGoingIn = 1;

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatAverage)
                    {
                        chanceOfShotGoingIn = 25;

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatHigh)
                    {
                        chanceOfShotGoingIn = 80;

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);
                    }

                    int randomValue = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(0, 100);

                    if (randomValue <= chanceOfShotGoingIn)
                    {
                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(0, 1f, 0);

                        ParentBasketballCourtLevel.Basketball.OmniLight.LightColor = new Color(0, 1, 0); 
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatAverage)
                    {
                        float chanceOfSkew = ParentBasketballCourtLevel.RandomNumberGenerator.RandiRange(1, 2);

                        float randomXOffset = 0;

                        if (chanceOfSkew == 1)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(.5f, 1f);
                        }
                        else if (chanceOfSkew == 2)
                        {
                            randomXOffset = ParentBasketballCourtLevel.RandomNumberGenerator.RandfRange(-1f, -.5f);
                        }

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, 1f, 0);

                        ParentBasketballCourtLevel.Basketball.OmniLight.LightColor = new Color(1, 0, 0);
                    }
                    else if (SkillStats.ThreePointShooting == GlobalConstants.SkillStatLow)
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

                        ParentBasketballCourtLevel.Basketball.GlobalPosition = ParentBasketballCourtLevel.HoopArea.GlobalPosition + new Vector3(randomXOffset, 1f, 0);

                        ParentBasketballCourtLevel.Basketball.OmniLight.LightColor = new Color(1, 0, 0);
                    }
                }

                TargetPlayer = this;

                ParentBasketballCourtLevel.BasketballResetTimer.Start();
            }
        }

        #region Pass Target Input

        protected void GetPassTargetSelectionInput()
        {
            if (Input.IsActionJustPressed($"SelectTargetLeft_{TeamIdentifier}"))
            {
                FindPassTargetPlayer(true);
            }
            else if (Input.IsActionJustPressed($"SelectTargetRight_{TeamIdentifier}"))
            {
                FindPassTargetPlayer(false);
            }
        }


        private void FindPassTargetPlayer(bool fromLeftToRight)
        {
            List<BasketballPlayer> availablePlayers = GetOrganizedAvailablePassTargets(fromLeftToRight);

            if (TargetPlayer != this)
            {
                availablePlayers.Remove(this);
            }

            int indexOfCurrentTargetPlayer = availablePlayers.IndexOf(TargetPlayer);

            if (indexOfCurrentTargetPlayer == 0)
            {
                TargetPlayer = availablePlayers.Last();
            }
            else
            {
                TargetPlayer = availablePlayers.ElementAt(indexOfCurrentTargetPlayer - 1);
            }

            TargetPlayer.IsTargeted = true;

            //GD.Print($"Player {DeviceIdentifier} will pass to Player {PassTargetPlayer?.DeviceIdentifier}\n");
        }

        private List<BasketballPlayer> GetOrganizedAvailablePassTargets(bool fromLeftToRight)
        {
            List<BasketballPlayer> availablePassTargets;

            //Reset pass target indicators for all players
            ParentBasketballCourtLevel.AllBasketballPlayers.ForEach(player => player.IsTargeted = false);

            if (fromLeftToRight)
            {
                availablePassTargets = ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier).OrderBy(player => player.GlobalPosition.X).ToList();
            }
            else
            {
                availablePassTargets = ParentBasketballCourtLevel.AllBasketballPlayers.Where(player => player.TeamIdentifier == TeamIdentifier).OrderByDescending(player => player.GlobalPosition.X).ToList();
            }

            return availablePassTargets;
        }

        #endregion

        protected void GetPassFocusInput()
        {
            if (Input.IsActionJustPressed($"PassFocus_{TeamIdentifier}"))
            {
                GD.Print($"PassFocus triggered by player {PlayerIdentifier}");

                TargetPlayer.HasFocus = true;

                this.HasFocus = false;

                TargetPlayer = this;
            }
        }

        protected void GetPassBallInput()
        {
            if (Input.IsActionJustPressed($"PassBall_{TeamIdentifier}"))
            {
                GD.Print($"PassBall triggered by player {PlayerIdentifier}");

                TargetPlayer.HasFocus = true;
                TargetPlayer.HasBasketball = true;

                this.HasBasketball = false;
                this.HasFocus = false;

                ParentBasketballCourtLevel.Basketball.Reparent(TargetPlayer);

                ParentBasketballCourtLevel.Basketball.GlobalPosition = TargetPlayer.GlobalPosition + new Vector3(0, 0, 1.5f);

                TargetPlayer = this;
            }
        }

        #endregion

        #endregion

        #region Physics Handling - Process

        public override void _PhysicsProcess(double delta)
        {
            MovePlayer();

            if (IsOnFloor() && !_jumpTimer.IsStopped())
            {
                //GD.Print("Is considered on floor");

                _jumpTimer.Stop();
            }
        }

        private void MovePlayer()
        {
            Velocity = moveDirection * speed;
            MoveAndSlide();

            if (moveDirection != Vector3.Zero)
            {
                //LookAt((GlobalPosition + moveDirection), Vector3.Up);

                float newAngle = 0;  

                if (IsOnFloor())
                {
                    newAngle = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(moveDirection.X, moveDirection.Z), .1f);
                }
                else
                {
                    newAngle = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(moveDirection.X, moveDirection.Z), .01f);
                }

                GlobalRotation = new Vector3(GlobalRotation.X, newAngle, GlobalRotation.Z);


                //Rotation = GlobalPosition.Rotated(moveDirection, 0);
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
        }

        private void OnBodyDetectionAreaExited(Area3D area)
        {
            if (area.IsInGroup(GroupTags.ThreePointLine))
            {
                IsInThreePointLine = false;
            }
        }

        #endregion
    }
}

