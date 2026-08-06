using Godot;
using System;

namespace Online
{
    public partial class TestButton : Button
    {
        public long OnlinePlayerIdentifier;

        public override void _EnterTree()
        {
            SetMultiplayerAuthority((int)OnlinePlayerIdentifier);
        }

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
