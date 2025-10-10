using Constants;
using Enums;
using Godot;
using Levels;
using System;
using System.ComponentModel;

namespace Entities
{
    public partial class EntityShadow: MeshInstance3D
    {
        [Export]
        public Node3D ParentObject;

        float groundY = -4.25f;

        public override void _Ready()
        {
        }

        public override void _Process(double delta)
        {
            GlobalPosition = new Vector3(ParentObject.GlobalPosition.X, groundY + 0.1f, ParentObject.GlobalPosition.Z);
            GlobalRotation = Vector3.Zero;
        }
    }
}
