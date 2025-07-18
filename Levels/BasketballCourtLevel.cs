using Constants;
using Godot;
using Entities;
using System;
using System.Collections.Generic;

namespace BasketballCourtLevel
{
    public partial class BasketballCourtLevel : Node3D
    {
        public Basketball Basketball = new Basketball();

        public List<BasketballPlayer> AllBasketballPlayers = new List<BasketballPlayer>();


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Basketball = GetNode("Basketball") as Basketball;

            GetAllBasketballPlayers();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public void GetAllBasketballPlayers()
        {
            var basketballPlayerRootNodes = GetTree().GetNodesInGroup(GroupTags.BasketballPlayer);

            foreach (Node3D basketballPlayerRootNode in basketballPlayerRootNodes)
            {
                AllBasketballPlayers.Add(basketballPlayerRootNode as BasketballPlayer);

                if (AllBasketballPlayers.Count == 1)
                {
                    BasketballPlayer basketballPlayer = basketballPlayerRootNode as BasketballPlayer;

                    Basketball.Reparent(basketballPlayer);
                    basketballPlayer.HasBasketball = true;

                    Basketball.GlobalPosition = basketballPlayer.GlobalPosition + new Vector3(0, 0, 1.5f);
                }
            }
        }
    }
}
