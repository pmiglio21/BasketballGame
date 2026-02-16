using Constants;
using Entities;
using Enums;
using Godot;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Levels
{
    public partial class BasketballCourtLevel : Node3D
    {
        #region Children Objects

        public Control ScoreboardControl = new();

        public RichTextLabel BlueScoreRichTextLabel = new();

        public RichTextLabel RedScoreRichTextLabel = new();

        public Basketball Basketball = new();

        public StaticBody3D BasketballHoop = new();

        public Area3D HoopArea = new();

        public List<BasketballPlayer> AllBasketballPlayers = new();

        public Timer BasketballResetTimer = new();

        public List<PlayerStartPoint> PlayerStartPoints = new();

        public List<Node3D> DunkPoints = new();

        public List<Node3D> LayupPoints = new();

        public List<CpuOccupationZone> CpuOccupationZones = new();

        #endregion

        public RandomNumberGenerator RandomNumberGenerator = new();

        public HashSet<SkillStatType> AllPlayersHighSkillStatsFilled_Team1 = new();

        public HashSet<SkillStatType> AllPlayersHighSkillStatsFilled_Team2 = new();

        public HashSet<SkillStatType> AllPlayersLowSkillStatsFilled_Team1 = new();

        public HashSet<SkillStatType> AllPlayersLowSkillStatsFilled_Team2 = new();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ScoreboardControl = GetNode("Scoreboard") as Control;
            BlueScoreRichTextLabel = ScoreboardControl.GetNode("BlueTeamScore") as RichTextLabel;
            RedScoreRichTextLabel = ScoreboardControl.GetNode("RedTeamScore") as RichTextLabel;

            Basketball = GetNode("Basketball") as Basketball;
            BasketballHoop = GetNode("BasketballHoop") as StaticBody3D;
            HoopArea = GetNode("HoopArea") as Area3D;
            BasketballResetTimer = GetNode("BasketballResetTimer") as Timer;
            BasketballResetTimer.Timeout += ResetBasketballOnTimeout;

            PlayerStartPoints = GetTree().GetNodesInGroup(GroupTags.PlayerStartPoint).Cast<PlayerStartPoint>().ToList();

            DunkPoints = GetTree().GetNodesInGroup(GroupTags.DunkPoint).Cast<Node3D>().ToList();

            LayupPoints = GetTree().GetNodesInGroup(GroupTags.LayupPoint).Cast<Node3D>().ToList();

            CpuOccupationZones = GetTree().GetNodesInGroup(GroupTags.CpuOccupationZone).Cast<CpuOccupationZone>().ToList();

            GetAllBasketballPlayers();

            AssignPlayersToStartPoints();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            AssignPlayersToCpuOccupationZones();
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

        public void GiveBasketballToPlayer(BasketballPlayer basketballPlayer)
        {
            if (basketballPlayer != null && Basketball.GetParent() is not BasketballPlayer)
            {
                if (Basketball.GetParent() != basketballPlayer)
                {
                    Basketball.Reparent(basketballPlayer);
                }
                
                //Basketball.ParentPlayer = basketballPlayer;
                basketballPlayer.HasBasketball = true;
                basketballPlayer.HasFocus = true;

                Vector3 distanceBetweenPlayerAndBall = new Vector3(0, 0, 1.5f);
                Vector3 rotatedDistance = distanceBetweenPlayerAndBall.Rotated(Vector3.Up, basketballPlayer.GlobalRotation.Y);
                Basketball.GlobalPosition = basketballPlayer.GlobalPosition + rotatedDistance;
                Basketball.BasketballState = BasketballState.IsBeingDribbled;

                Basketball.PreviousPlayer = basketballPlayer;

                FlipTeamIsOnOffense(basketballPlayer.TeamIdentifier, basketballPlayer.IsOnOffense);
            }
        }

        public void FlipTeamIsOnOffense(string teamIdentifier, bool isOnOffense)
        {
            string otherTeamIdentifier = teamIdentifier == "1" ? "2" : "1";

            List<BasketballPlayer> playersOnTeam = AllBasketballPlayers.Where(player => player.TeamIdentifier == teamIdentifier).ToList();
            List<BasketballPlayer> playersOnOtherTeam = AllBasketballPlayers.Where(player => player.TeamIdentifier == otherTeamIdentifier).ToList();

            foreach (BasketballPlayer player in playersOnTeam)
            {
                player.IsOnOffense = isOnOffense;
            }

            foreach (BasketballPlayer player in playersOnOtherTeam)
            {
                player.IsOnOffense = !isOnOffense;
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
                        basketballPlayer.SkillStats.Passing = GlobalConstants.SkillStatHigh;
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
                        basketballPlayer.SkillStats.Passing = GlobalConstants.SkillStatLow;
                    }
                }
            }

            basketballPlayer.SkillStats.Rebounding = GlobalConstants.SkillStatHigh;
            basketballPlayer.SkillStats.Blocking = GlobalConstants.SkillStatHigh;
        }

        public void AssignPlayersToStartPoints()
        {
            foreach (BasketballPlayer basketballPlayer in AllBasketballPlayers.Where(x => x.TeamIdentifier == "1"))
            {
                PlayerStartPoint assignedStartPoint = PlayerStartPoints.FirstOrDefault(sp => sp.PlayerIdentifier == basketballPlayer.PlayerIdentifier);

                if (assignedStartPoint != null)
                {
                    basketballPlayer.GlobalPosition = assignedStartPoint.GlobalPosition;
                }
            }
        }

        public void AssignPlayersToCpuOccupationZones()
        {
            List<Tuple<BasketballPlayer, CpuOccupationZone, float>> playersOccupationZonesAndDistances = new List<Tuple<BasketballPlayer, CpuOccupationZone, float>>();

            //Get every iteration of the three players and three zones
            for (int i = 0; i < 3; i++)
            {
                foreach (BasketballPlayer basketballPlayer in AllBasketballPlayers.Where(x => x.IsOnOffense))
                {
                    float distanceBetweenPlayerAndZone = PhysicsMathHelper.GetHorizontalDistance(basketballPlayer.GlobalPosition, CpuOccupationZones[i].GlobalPosition);

                    playersOccupationZonesAndDistances.Add(new Tuple<BasketballPlayer, CpuOccupationZone, float>(basketballPlayer, CpuOccupationZones[i], distanceBetweenPlayerAndZone));
                }
            }

            //Make sure all offensive players have been assigned a zone
            List<int> takenOccupationZones = new List<int>();

            //Smallest distance is first
            playersOccupationZonesAndDistances = playersOccupationZonesAndDistances.OrderBy(x => x.Item3).ToList();

            //Deal with player with ball first
            Tuple<BasketballPlayer, CpuOccupationZone, float> playerWithBall = playersOccupationZonesAndDistances.FirstOrDefault(p => p.Item1.HasBasketball);

            if (playerWithBall != null)
            {
                //If player with ball is getting a new zone, THEN CPUs can find new destination position
                if (playerWithBall.Item1.CurrentCpuOccupationZone == null || playerWithBall.Item2.ZoneNumber != playerWithBall.Item1.CurrentCpuOccupationZone.ZoneNumber)
                {
                    takenOccupationZones.Add(playerWithBall.Item2.ZoneNumber);

                    playerWithBall.Item1.CurrentCpuOccupationZone = playerWithBall.Item2;

                    playersOccupationZonesAndDistances.RemoveAll(x => x.Item2.ZoneNumber == playerWithBall.Item2.ZoneNumber);

                    playersOccupationZonesAndDistances.RemoveAll(x => x.Item1.PlayerIdentifier == playerWithBall.Item1.PlayerIdentifier);

                    playerWithBall.Item1.CpuDestinationPosition = playerWithBall.Item1.CurrentCpuOccupationZone.GlobalPosition;

                    while (takenOccupationZones.Count != 3)
                    {
                        Tuple<BasketballPlayer, CpuOccupationZone, float> nextClosestPlayer = playersOccupationZonesAndDistances.FirstOrDefault();

                        if (nextClosestPlayer != null && (nextClosestPlayer.Item1.CurrentCpuOccupationZone == null || nextClosestPlayer.Item2.ZoneNumber != nextClosestPlayer.Item1.CurrentCpuOccupationZone.ZoneNumber))
                        {
                            nextClosestPlayer.Item1.CurrentCpuOccupationZone = nextClosestPlayer.Item2;

                            float diffInX = RandomNumberGenerator.RandfRange(-5, 5);
                            float diffInZ = RandomNumberGenerator.RandfRange(-5, 5);

                            nextClosestPlayer.Item1.CpuDestinationPosition = nextClosestPlayer.Item1.CurrentCpuOccupationZone.GlobalPosition + new Vector3(diffInX, 0, diffInZ);
                        }

                        takenOccupationZones.Add(nextClosestPlayer.Item2.ZoneNumber);

                        playersOccupationZonesAndDistances.RemoveAll(x => x.Item2.ZoneNumber == nextClosestPlayer.Item2.ZoneNumber);

                        playersOccupationZonesAndDistances.RemoveAll(x => x.Item1.PlayerIdentifier == nextClosestPlayer.Item1.PlayerIdentifier);
                    }
                }
            }


            //foreach (var player in AllBasketballPlayers.Where(x => x.IsOnOffense))
            //{
            //    GD.Print($"Player {player.PlayerIdentifier} assigned to Zone {(player.CurrentCpuOccupationZone != null ? player.CurrentCpuOccupationZone.ZoneNumber.ToString() : "None")}");
            //}
        }

        private void ResetBasketballOnTimeout()
        {
            if (Basketball.GetParent() is not BasketballPlayer)
            {
                Basketball.OmniLight.LightColor = new Color(1, 1, 1); // Reset light color to white

                AllBasketballPlayers.ForEach(player => player.HasFocus = false);

                GiveBasketballToPlayer(Basketball.PreviousPlayer);

                BasketballPlayer focusedDefensePlayer = AllBasketballPlayers.FirstOrDefault(p => p.TeamIdentifier != Basketball.PreviousPlayer.TeamIdentifier && p.PlayerIdentifier == Basketball.PreviousPlayer.PlayerIdentifier);

                if (focusedDefensePlayer != null)
                {
                    focusedDefensePlayer.HasFocus = true;
                }
            }
        }

        public void UpdateScoreboard()
        {
            int blueTeamScore = AllBasketballPlayers.Where(p => p.TeamIdentifier == "1").Sum(p => p.BoxScoreStats.TotalPointsScored);
            int redTeamScore = AllBasketballPlayers.Where(p => p.TeamIdentifier == "2").Sum(p => p.BoxScoreStats.TotalPointsScored);

            BlueScoreRichTextLabel.Text = blueTeamScore.ToString();
            RedScoreRichTextLabel.Text = redTeamScore.ToString();
        }
    }
}
