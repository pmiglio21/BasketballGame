using Godot;
using System;
using System.ComponentModel;

namespace Entities
{
    public partial class Basketball : CharacterBody3D, INotifyPropertyChanged
    {
        #region Player Relations

        public BasketballPlayer PreviousPlayer = null;

        public BasketballPlayer TargetPlayer = null;

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

        #endregion

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
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
                Velocity = new Vector3(Velocity.X, Velocity.Y - .5f, Velocity.Z);


                //if (Timer.IsStopped() && Timer.TimeLeft <= 0)
                //{
                //    Velocity = new Vector3(0, -10, 0);
                //}

                //KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

                ////MoveAndSlide();

                //if (collisionInfo != null)
                //{
                //    Velocity = Velocity.Bounce(collisionInfo.GetNormal());

                //    Timer.Start();

                //    IsBeingShot = false;
                //}

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
            //else
            //{
            //    Velocity = new Vector3(0, Mathf.Clamp(Velocity.Y + .1f, -10, 0), 0);

            //    //if (Timer.IsStopped() && Timer.TimeLeft <= 0)
            //    //{
            //    //    Velocity = new Vector3(0, Mathf.Clamp(Velocity.Y + .1f, -10, 0), 0);
            //    //}

            //    KinematicCollision3D collisionInfo = MoveAndCollide(Velocity * (float)delta);

            //    //MoveAndSlide();

            //    if (collisionInfo != null)
            //    {
            //        Velocity = Velocity.Bounce(collisionInfo.GetNormal());

            //        //Timer.Start();
            //    }
            //}
        }
    }
}
