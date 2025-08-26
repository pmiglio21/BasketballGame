using Constants;
using Godot;
using Entities;
using Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Levels
{
    public partial class BasketballCourtLevel : Node3D
    {
        #region Children Objects

        public Basketball Basketball = new Basketball();

        public StaticBody3D BasketballHoop = new StaticBody3D();

        public Area3D HoopArea = new Area3D();

        public List<BasketballPlayer> AllBasketballPlayers = new List<BasketballPlayer>();

        public Timer BasketballResetTimer = new Timer();

        #endregion

        public RandomNumberGenerator RandomNumberGenerator = new RandomNumberGenerator();

        public HashSet<SkillStatType> AllPlayersHighSkillStatsFilled_Team1 = new HashSet<SkillStatType>();

        public HashSet<SkillStatType> AllPlayersHighSkillStatsFilled_Team2 = new HashSet<SkillStatType>();

        public HashSet<SkillStatType> AllPlayersLowSkillStatsFilled_Team1 = new HashSet<SkillStatType>();

        public HashSet<SkillStatType> AllPlayersLowSkillStatsFilled_Team2 = new HashSet<SkillStatType>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Basketball = GetNode("Basketball") as Basketball;
            BasketballHoop = GetNode("BasketballHoop") as StaticBody3D;
            HoopArea = BasketballHoop.GetNode("HoopArea") as Area3D;
            BasketballResetTimer = GetNode("BasketballResetTimer") as Timer;
            BasketballResetTimer.Timeout += ResetBasketballOnTimeout;

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

                BasketballPlayer basketballPlayer = basketballPlayerRootNode as BasketballPlayer;

                if (basketballPlayer.TeamIdentifier == "1")
                {
                    if (basketballPlayer.PlayerIdentifier == "1")
                    {
                        GiveBasketballToPlayer(basketballPlayer);
                    }

                    basketballPlayer.IsOnOffense = true;

                    RandomlyAssignSkillStatsToPlayer(basketballPlayer, AllPlayersHighSkillStatsFilled_Team1, AllPlayersLowSkillStatsFilled_Team1);
                }
                else if (basketballPlayer.TeamIdentifier == "2")
                {
                    if (basketballPlayer.PlayerIdentifier == "1")
                    {
                        basketballPlayer.HasFocus = true;
                    }

                    RandomlyAssignSkillStatsToPlayer(basketballPlayer, AllPlayersHighSkillStatsFilled_Team2, AllPlayersLowSkillStatsFilled_Team2);
                }
            }

            foreach (BasketballPlayer basketballPlayer in AllBasketballPlayers)
            {
                basketballPlayer.PairingPlayer = AllBasketballPlayers.FirstOrDefault(p => p.TeamIdentifier != basketballPlayer.TeamIdentifier && p.PlayerIdentifier == basketballPlayer.PlayerIdentifier);
            }
        }

        private void GiveBasketballToPlayer(BasketballPlayer basketballPlayer)
        {
            if (basketballPlayer != null)
            {
                Basketball.Reparent(basketballPlayer);
                //Basketball.ParentPlayer = basketballPlayer;
                basketballPlayer.HasBasketball = true;
                basketballPlayer.HasFocus = true;

                Vector3 distanceBetweenPlayerAndBall = new Vector3(0, 0, 1.5f);
                Vector3 rotatedDistance = distanceBetweenPlayerAndBall.Rotated(Vector3.Up, basketballPlayer.GlobalRotation.Y);
                Basketball.GlobalPosition = basketballPlayer.GlobalPosition + rotatedDistance;
                Basketball.IsBeingShot = false;
                Basketball.IsDribbling = true;
            }
        }

        private void RandomlyAssignSkillStatsToPlayer(BasketballPlayer basketballPlayer, HashSet<SkillStatType> allPlayersHighSkillStatsFilled, HashSet<SkillStatType> allPlayersLowSkillStatsFilled)
        {
            while (basketballPlayer.SkillStats.HighSkillStatsFilled.Count < 2)
            {
                int skillStatTypeIndex = RandomNumberGenerator.RandiRange(0, 7);

                if (!allPlayersHighSkillStatsFilled.Contains((SkillStatType)skillStatTypeIndex) && basketballPlayer.SkillStats.AvailableSkillStatsToAlter.Contains((SkillStatType)skillStatTypeIndex))
                {
                    allPlayersHighSkillStatsFilled.Add((SkillStatType)skillStatTypeIndex);
                    basketballPlayer.SkillStats.HighSkillStatsFilled.Add((SkillStatType)skillStatTypeIndex);
                    basketballPlayer.SkillStats.AvailableSkillStatsToAlter.Remove((SkillStatType)skillStatTypeIndex);

                    if (skillStatTypeIndex == 0)
                    {
                        basketballPlayer.SkillStats.TwoPointShooting = GlobalConstants.SkillStatHigh;
                    }
                    else if (skillStatTypeIndex == 1)
                    {
                        basketballPlayer.SkillStats.ThreePointShooting = GlobalConstants.SkillStatHigh;
                    }
                    else if (skillStatTypeIndex == 2)
                    {
                        basketballPlayer.SkillStats.Dunking = GlobalConstants.SkillStatHigh;
                    }
                    else if (skillStatTypeIndex == 3)
                    {
                        basketballPlayer.SkillStats.Rebounding = GlobalConstants.SkillStatHigh;
                    }
                    else if (skillStatTypeIndex == 4)
                    {
                        basketballPlayer.SkillStats.Stealing = GlobalConstants.SkillStatHigh;
                    }
                    else if (skillStatTypeIndex == 5)
                    {
                        basketballPlayer.SkillStats.Blocking = GlobalConstants.SkillStatHigh;
                    }
                    else if (skillStatTypeIndex == 6)
                    {
                        basketballPlayer.SkillStats.BallHandling = GlobalConstants.SkillStatHigh;
                    }
                    else if (skillStatTypeIndex == 7)
                    {
                        basketballPlayer.SkillStats.Speed = GlobalConstants.SkillStatHigh;
                    }
                }
            }

            while (basketballPlayer.SkillStats.LowSkillStatsFilled.Count < 2)
            {
                int skillStatTypeIndex = RandomNumberGenerator.RandiRange(0, 7);

                if (!allPlayersLowSkillStatsFilled.Contains((SkillStatType)skillStatTypeIndex) && basketballPlayer.SkillStats.AvailableSkillStatsToAlter.Contains((SkillStatType)skillStatTypeIndex))
                {
                    allPlayersLowSkillStatsFilled.Add((SkillStatType)skillStatTypeIndex);
                    basketballPlayer.SkillStats.LowSkillStatsFilled.Add((SkillStatType)skillStatTypeIndex);
                    basketballPlayer.SkillStats.AvailableSkillStatsToAlter.Remove((SkillStatType)skillStatTypeIndex);

                    if (skillStatTypeIndex == 0)
                    {
                        basketballPlayer.SkillStats.TwoPointShooting = GlobalConstants.SkillStatLow;
                    }
                    else if (skillStatTypeIndex == 1)
                    {
                        basketballPlayer.SkillStats.ThreePointShooting = GlobalConstants.SkillStatLow;
                    }
                    else if (skillStatTypeIndex == 2)
                    {
                        basketballPlayer.SkillStats.Dunking = GlobalConstants.SkillStatLow;
                    }
                    else if (skillStatTypeIndex == 3)
                    {
                        basketballPlayer.SkillStats.Rebounding = GlobalConstants.SkillStatLow;
                    }
                    else if (skillStatTypeIndex == 4)
                    {
                        basketballPlayer.SkillStats.Stealing = GlobalConstants.SkillStatLow;
                    }
                    else if (skillStatTypeIndex == 5)
                    {
                        basketballPlayer.SkillStats.Blocking = GlobalConstants.SkillStatLow;
                    }
                    else if (skillStatTypeIndex == 6)
                    {
                        basketballPlayer.SkillStats.BallHandling = GlobalConstants.SkillStatLow;
                    }
                    else if (skillStatTypeIndex == 7)
                    {
                        basketballPlayer.SkillStats.Speed = GlobalConstants.SkillStatLow;
                    }
                }
            }
        }

        private void ResetBasketballOnTimeout()
        {
            Basketball.OmniLight.LightColor = new Color(1, 1, 1); // Reset light color to white

            AllBasketballPlayers.ForEach(player => player.HasFocus = false);

            GiveBasketballToPlayer(AllBasketballPlayers.FirstOrDefault());

            BasketballPlayer focusedDefensePlayer = AllBasketballPlayers.FirstOrDefault(p => p.TeamIdentifier != "1" && p.PlayerIdentifier == "1");

            if (focusedDefensePlayer != null)
            {
                focusedDefensePlayer.HasFocus = true;
            }
        }
    }
}
