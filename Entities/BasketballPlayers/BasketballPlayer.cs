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

        private StaticBody3D _possessionIndicator = new StaticBody3D();
        
        private StaticBody3D _passTargetIndicator = new StaticBody3D();

        #endregion

        #region Player Identification Properties

        [Export]
        public string DeviceIdentifier = "1";

        #endregion

        #region State Properties

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
                        IsTargetedForPass = false;
                    }
                }
            }
        }
        private bool _hasBasketball = false;

        public bool IsTargetedForPass
        {
            get { return _isTargetedForPass; }
            set
            {
                if (_isTargetedForPass != value)
                {
                    _isTargetedForPass = value;
                    OnPropertyChanged(nameof(IsTargetedForPass));

                    if (_isTargetedForPass)
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
        private bool _isTargetedForPass = false;

        #endregion

        #region Movement Properties

        Vector3 moveInput = Vector3.Zero;
        float moveInputDeadzone = 0.1f;

        float moveDeadzone = 0.32f;
        protected Vector3 moveDirection = Vector3.Zero;
        protected float moveAngle = 0;

        float speed = 20.0f;

        #endregion

        public BasketballPlayer PassTargetPlayer = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ParentBasketballCourtLevel = GetParent() as BasketballCourtLevel;

            _possessionIndicator = GetNode("PossessionIndicator") as StaticBody3D;

            _passTargetIndicator = GetNode("PassTargetIndicator") as StaticBody3D;

            //Start target on the current player so TargetBasketballPlayer has something to go off of on the first target-selection input
            PassTargetPlayer = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == nameof(HasBasketball))
            {
                if (HasBasketball)
                {
                    _possessionIndicator.Show();
                }
                else
                {
                    _possessionIndicator.Hide();
                }
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            if (HasBasketball)
            {
                GetPassTargetInput();

                GetMovementInput();
            }
        }


        protected void GetPassTargetInput()
        {
            if (Input.IsActionJustPressed($"SelectPassTargetLeft_{DeviceIdentifier}"))
            {
                FindPassTargetPlayer(true);
            }
            else if (Input.IsActionJustPressed($"SelectPassTargetRight_{DeviceIdentifier}"))
            {
                FindPassTargetPlayer(false);
            }
        }


        private void FindPassTargetPlayer(bool fromLeftToRight)
        {
            List<BasketballPlayer> availablePlayers = GetOrganizedAvailablePassTargets(fromLeftToRight);

            if (PassTargetPlayer != this)
            {
                availablePlayers.Remove(this);
            }

            int indexOfCurrentTargetPlayer = availablePlayers.IndexOf(PassTargetPlayer);

            if (indexOfCurrentTargetPlayer == 0)
            {
                PassTargetPlayer = availablePlayers.Last();
            }
            else
            {
                PassTargetPlayer = availablePlayers.ElementAt(indexOfCurrentTargetPlayer - 1);
            }

            PassTargetPlayer.IsTargetedForPass = true;

            //GD.Print($"Player {DeviceIdentifier} will pass to Player {PassTargetPlayer?.DeviceIdentifier}\n");
        }

        private List<BasketballPlayer> GetOrganizedAvailablePassTargets(bool fromLeftToRight)
        {
            List<BasketballPlayer> availablePassTargets;

            //Reset pass target indicators for all players
            ParentBasketballCourtLevel.AllBasketballPlayers.ForEach(player => player.IsTargetedForPass = false);

            if (fromLeftToRight)
            {
                availablePassTargets = ParentBasketballCourtLevel.AllBasketballPlayers.OrderBy(player => player.GlobalPosition.X).ToList();
            }
            else
            {
                availablePassTargets = ParentBasketballCourtLevel.AllBasketballPlayers.OrderByDescending(player => player.GlobalPosition.X).ToList();
            }

            return availablePassTargets;
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
                //moveAngle = new Vector3(normalizedMoveInput.X, 0, normalizedMoveInput.Z);
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

            if (moveDirection != Vector3.Zero)
            {
                //LookAt((GlobalPosition + moveDirection), Vector3.Up);

                var newAngle = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(moveDirection.X, moveDirection.Z), .1f);

                GlobalRotation = new Vector3(GlobalRotation.X, newAngle, GlobalRotation.Z);

                //Rotation = GlobalPosition.Rotated(moveDirection, 0);
            }
        }
    }
}

