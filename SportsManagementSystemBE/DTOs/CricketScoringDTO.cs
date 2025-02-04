using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SportsManagementSystemBE.DTOs
{
    public class CricketScoringDTO
    {
        public int Teamid { get; set; }
        public int Score { get; set; }
        public string Over { get; set; }
        public int Wickets { get; set; }
        public int FixtureId { get; set; }
    }

    public class GoalBaseDTO
    {
        public int Teamid { get; set; }
        public int Goals { get; set; }
        public int Fixture_id { get; set; }
    }

    public class BallByBallDto
    {
        public int Over { get; set; }
        public int Ball { get; set; }
        public string Striker { get; set; }
        public string NonStriker { get; set; }
        public string Bowler { get; set; }
        public int BatsmanRuns { get; set; }
        public int ExtraRuns { get; set; }
        public string ExtraType { get; set; }
        public bool IsWicket { get; set; }
        public string WicketType { get; set; }
        public string DismissedPlayer { get; set; }
        public string Fielder { get; set; }
        public int TeamId { get; set; } // Internal use only
    }

}