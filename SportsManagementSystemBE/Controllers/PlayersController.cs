using Microsoft.Win32;
using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace SportsManagementSystemBE.Controllers
{ 
    public class PlayersController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpGet]
        public HttpResponseMessage HandleUser(int userid)
        {
            //if (userRequest.UserId == 0)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, "UserId is required.");
            //}

            try
            {
                var userdata = db.Users.FirstOrDefault(u => u.id == userid);
                var Studentdata = db.Students.
                    Where(s => s.reg_no == userdata.registration_no).
                Select(s=>new { Reg_no = s.reg_no, Name = s.name, Gender=s.gender}).
                ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, Studentdata);
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public HttpResponseMessage PostPlayers([FromBody] PlayersRequest request)
        {
            try
            {
                var teamDetails=db.Teams.FirstOrDefault(t=>t.id==request.TeamNo);
                var playersToAdd = new List<Player>();
                foreach (var rollno in request.RollNumbers.Distinct())
                {
                    var UserExists = db.Students.Any(s => s.reg_no == rollno);
                    if (!UserExists)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);//, $"Student with roll number {userdata.registration_no} does not exist."
                    }
                    // Check if player is part of another team in the same sport and session
                    var existingPlayerInSameSportAndSession = (from p in db.Players
                                                               join t in db.Teams on p.team_id equals t.id
                                                               where p.reg_no == rollno &&
                                                                     t.sports_id == teamDetails.sports_id &&
                                                                     t.session_id == teamDetails.session_id
                                                               select p).FirstOrDefault();

                    if (existingPlayerInSameSportAndSession != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.Conflict,new { Rollno = rollno });
                    }

                    playersToAdd.Add(new Player
                    {
                        reg_no = rollno,
                        team_id = request.TeamNo
                    });
                }

                if (playersToAdd.Any())
                {
                    db.Players.AddRange(playersToAdd);
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetTeamPlayers(int id)
        {
            try
            {
                var TeamDetails = db.Players.
                    Where(p => p.team_id == id)
                    .Join(db.Students,
                    p => p.reg_no,
                    s => s.reg_no,
                    (p, s) => new { p.id, p.reg_no, names = s.name, })
                    .ToList();


                //.Select(t => new { t.id, t.name,t.image_path,t.teamStatus })
                //.ToList();
                if (TeamDetails == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, TeamDetails);


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        

        [HttpGet]
        public HttpResponseMessage GetUserTeams(string regno)
        {
            try
            {
                if (regno == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user ID.");
                }
                var Studentregno = db.Students.FirstOrDefault(s=>s.reg_no == regno );
                if (Studentregno == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);//, "Reg-no not found in student table "
                }

                // Get the latest session
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                if (latestSession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No sessions found.");
                }

                // Get all teams the user is part of
                var userTeams = db.Players
                    .Where(p => p.reg_no == regno)
                    .Select(p => p.team_id)
                    .ToList();

                if (!userTeams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, "User is not part of any team."
                }

                // Get fixtures for the user's teams
                var userFixtures = (
                    from f in db.Fixtures
                    join t1 in db.Teams on f.team1_id equals t1.id
                    join t2 in db.Teams on f.team2_id equals t2.id
                    join ss1 in db.SessionSports on t1.sports_id equals ss1.sports_id
                    join s1 in db.Sports on ss1.sports_id equals s1.id
                    join ss2 in db.SessionSports on t1.sports_id equals ss2.sports_id
                    join s2 in db.Sports on ss2.sports_id equals s2.id
                    join ss in db.SessionSports on latestSession.id equals ss.id
                    //join s in db.Sports on ss.sports_id equals s.id
                    where userTeams.Contains(t1.id) || userTeams.Contains(t2.id) // Check if the fixture involves any of the user's teams
                    select new
                    {
                        fixtureid= f.id,
                        team1id= t1.id,
                        team2id= t2.id,
                        team1name = t1.name,
                        team2name = t2.name,
                        matchdate = f.matchDate,
                        venue = f.venue,
                        sportname = userTeams.Contains(t1.id) ? s1.games : s2.games,
                        winnerteam = f.winner_id == null ? "NotDecided" : db.Teams.Where(t => t.id == f.winner_id)
                        .Select(t => t.name).FirstOrDefault()
                        //teamstatus=t1.teamStatus
                    }).ToList();

                // Get all teams the user is part of, even if they don't have fixtures
                var allUserTeams = (
                    from t in db.Teams
                    where userTeams.Contains(t.id)
                    select new
                    {
                        teamname = t.name,
                        hasFixtures = db.Fixtures.Any(f => f.team1_id == t.id || f.team2_id == t.id) // Check if the team has any fixtures
                    }).ToList();
                var studentdata = db.Students.
                     Where(s => s.reg_no == regno).
                     Select(s => new {s.name,s.reg_no,s.discipline,s.section,s.semNo});

                // Combine results
                var result = new
                {
                    Fixtures = userFixtures,
                    Teams = allUserTeams,
                    userData= studentdata
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetplayerPerformancebysession(string regNo, int sessionId)
        {
            try
            {


                var players = db.Players
                                .Where(p => p.reg_no == regNo)
                                .Select(p => new { p.id, p.team_id })
                                .ToList();

                if (!players.Any())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Player not found.");
                }

                var playerTeamIds = players.Select(p => p.team_id).ToList();
                var playerIds = players.Select(p => p.id).ToList();

                var userFixtures = (
                    from f in db.Fixtures
                    join ss in db.SessionSports on f.sessionSports_id equals ss.id
                    where ss.session_id == sessionId &&
                          (playerTeamIds.Contains(f.team1_id ?? 0) || playerTeamIds.Contains(f.team2_id ?? 0))
                    select new { f.id }
                ).ToList();

                var fixtureIds = userFixtures.Select(f => f.id).ToList();

                var cricketStats = db.deliveries
                    .Where(d => fixtureIds.Contains(d.fixture_id ?? 0) &&
                               (playerIds.Contains(d.striker_id ?? 0) || playerIds.Contains(d.bowler_id ?? 0)))
                    .GroupBy(d => d.fixture_id)
                    .Select(g => new
                    {
                        Fixtureid = g.Key,
                        totalRuns = g.Sum(d => (d.striker_id.HasValue && playerIds.Contains(d.striker_id.Value)) ? (d.runs_scored ?? 0) : 0),
                        totalWickets = g.Count(d => d.bowler_id.HasValue && playerIds.Contains(d.bowler_id.Value) &&
                                                   d.wicket_type != null &&
                                                   (d.wicket_type == "Bowled" || d.wicket_type == "Stumped" ||
                                                    d.wicket_type == "Hit Wicket" || d.wicket_type == "Caught"))
                    })
                    .ToList();
                var FootballStats = db.Match_events
                    .Where(m => fixtureIds.Contains(m.fixture_id ?? 0) &&
                               (playerIds.Contains(m.player_id ?? 0)))
                    .GroupBy(m => m.fixture_id)
                    .Select(g => new
                    {
                        Fixtureid = g.Key,
                        totalgoals = g.Count(m => m.player_id.HasValue && playerIds.Contains(m.player_id.Value) && m.event_type=="Goal")
                    })
                    .ToList();
                 var totalCricketMatches = cricketStats.Count;   // Count unique fixture IDs in Cricket
                var totalFootballMatches = FootballStats.Count; // Count unique fixture IDs in Football
                var totalruns=cricketStats.Sum(d => d.totalRuns);
                var totalwickets=cricketStats.Sum (d => d.totalWickets);
                var FootballStat = FootballStats.Sum(m => m.totalgoals);

                var totaldata = new
                {
                    Crickettotalruns = totalruns,
                    Crickettotalwickets = totalwickets,
                    totalCricketMatches = totalCricketMatches,
                    totalFootballMatches = totalFootballMatches,
                    FootballGoals= FootballStat,
                    //cricketStats = cricketStats
                };

                return Request.CreateResponse(HttpStatusCode.OK, totaldata);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
    }

        [HttpGet]
        public HttpResponseMessage Getplayerrunsbybymatches(string regNo, int sessionId)
        {
            try
            {
                var players = db.Players
                                .Where(p => p.reg_no == regNo)
                                .Select(p => new { p.id, p.team_id })
                                .ToList();

                if (!players.Any())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Player not found.");
                }

                var playerTeamIds = players.Select(p => p.team_id).ToList();
                var playerIds = players.Select(p => p.id).ToList();

                // Fetch Fixtures & Sports Data Correctly
                var userFixtures = db.Fixtures
                    .Where(f => db.SessionSports.Any(ss => ss.id == f.sessionSports_id && ss.session_id == sessionId) &&
                                (playerTeamIds.Contains(f.team1_id ?? 0) || playerTeamIds.Contains(f.team2_id ?? 0)))
                    .Select(f => new
                    {
                        f.id,
                        Team1Name = db.Teams.Where(t => t.id == f.team1_id).Select(t => t.name).FirstOrDefault() ?? "Unknown",
                        Team2Name = db.Teams.Where(t => t.id == f.team2_id).Select(t => t.name).FirstOrDefault() ?? "Unknown",
                        SportId = db.SessionSports.Where(ss => ss.id == f.sessionSports_id)
                                                   .Select(ss => ss.sports_id)
                                                   .FirstOrDefault()
                    })
                    .ToList()  // ✅ Fetch fixtures first
                    .Select(f => new  // ✅ Now safely fetch SportName
                    {
                        f.id,
                        f.Team1Name,
                        f.Team2Name,
                        f.SportId,
                        SportName = db.Sports.Where(s => s.id == f.SportId)
                                             .Select(s => s.games)
                                             .FirstOrDefault() ?? "Unknown"
                    })
                    .ToList();

                var fixtureIds = userFixtures.Select(f => f.id).ToList();

                var cricketStats = db.deliveries
                    .Where(d => fixtureIds.Contains(d.fixture_id ?? 0) &&
                                (playerIds.Contains(d.striker_id ?? 0) || playerIds.Contains(d.bowler_id ?? 0)))
                    .GroupBy(d => d.fixture_id)
                    .Select(g => new
                    {
                        Fixtureid = g.Key,
                        totalRuns = g.Sum(d => (d.striker_id.HasValue && playerIds.Contains(d.striker_id.Value)) ? (d.runs_scored ?? 0) : 0),
                        totalWickets = g.Count(d => d.bowler_id.HasValue && playerIds.Contains(d.bowler_id.Value) &&
                                                    d.wicket_type != null &&
                                                    (d.wicket_type == "Bowled" || d.wicket_type == "Stumped" ||
                                                     d.wicket_type == "Hit Wicket" || d.wicket_type == "Caught"))
                    })
                    .ToList();
                var FootballStats = db.Match_events
                    .Where(m => fixtureIds.Contains(m.fixture_id ?? 0) &&
                               (playerIds.Contains(m.player_id ?? 0)))
                    .GroupBy(m => m.fixture_id)
                    .Select(g => new
                    {
                        Fixtureid = g.Key,
                        totalgoals = g.Count(m => m.player_id.HasValue && playerIds.Contains(m.player_id.Value) && m.event_type == "Goal")
                    })
                    .ToList();

                var results = userFixtures
                    .Select(fixture => new
                    {
                        Fixtureid = fixture.id,
                        Team1Name = fixture.Team1Name,
                        Team2Name = fixture.Team2Name,
                        SportName = fixture.SportName,
                        totalRuns = fixture.SportName == "Cricket"
                          ? (cricketStats.FirstOrDefault(cs => cs.Fixtureid == fixture.id)?.totalRuns ?? 0)
                                   : 0,
                        totalWickets = fixture.SportName == "Cricket"
                               ? (cricketStats.FirstOrDefault(cs => cs.Fixtureid == fixture.id)?.totalWickets ?? 0)
                                      : 0,
                        totalGoals = fixture.SportName == "Football"
                                ? (FootballStats.FirstOrDefault(cs => cs.Fixtureid == fixture.id)?.totalgoals ?? 0)
                                             : 0

                    });

                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


    }
}
