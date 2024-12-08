using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SportsManagementSystemBE.DTOs
{
    //in this class i am reciving the list of schedules and user id only for fixturecontroller function post fixtures.
    public class FixturesList
    {
        public List<Schedule> Schedules { get; set; }
        public int UserId { get; set; }
    }
    public class Schedule
    {
        public int Team1_id { get; set; }
        public int Team2_id { get; set; }
        public DateTime MatchDate { get; set; }
        public string Match_type { get; set; }
        public string Venue { get; set; }
   
    }
}