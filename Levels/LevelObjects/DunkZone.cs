using Godot;

namespace Levels
{
    public partial class DunkZone : Node3D
    {
        private Area3D _zoneArea;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _zoneArea = GetNode<Area3D>("ZoneArea");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }



        #region Signals

        private void OnDunkZoneAreaEntered(Area3D area)
        {

        }

        private void OnDunkZoneAreaExited(Area3D area)
        {

        }

        #endregion
    }
}
