using Godot;

namespace Levels
{
    public partial class PlayerStartPoint : Node3D
    {
        [Export]
        public string PlayerIdentifier { get; set; }

        public long PlayerIdentifierLONG { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
