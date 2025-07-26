using Godot;
using System;

namespace Entities
{
    public partial class Basketball : Node3D
    {
        public BasketballPlayer ParentPlayer = null;

        public OmniLight3D OmniLight = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            OmniLight = GetNode("OmniLight3D") as OmniLight3D;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }
}
