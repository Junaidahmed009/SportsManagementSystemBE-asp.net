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

}