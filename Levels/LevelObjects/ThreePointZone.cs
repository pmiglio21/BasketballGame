using Godot;

namespace Levels
{
    public partial class ThreePointZone : Node3D
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

        private void OnThreePointAreaZoneEntered(Area3D area)
        {

        }

        private void OnThreePointAreaZoneExited(Area3D area)
        {

        }

        #endregion
    }
}
