using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;

namespace SportsManagementSystemBE.Controllers
{
    public class FixtureController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpGet]
        public HttpResponseMessage GetFixtures(int userid)
        {
            try
            {
                var latestsession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();
                //if (DateTime.Now > latestsession.endDate) { 
                
                //}
                var sessionsportid = db.SessionSports.FirstOrDefault(s => s.session_id == latestsession.id && s.managed_by==userid);
                if (latestsession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No sessions found.");
                }

                var Fixtures = db.Fixtures
                    .Where(f => f.sessionSports_id == sessionsportid.id && f.winner_id == null &&
                    f.team1_id == null && f.team2_id == null )
                    .Select(f => new { f.id, f.matchDate, f.venue,f.match_type })
                    .ToList();
                

                if (!Fixtures.Any()) 
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No teams found for the latest session.");
                }

                return Request.CreateResponse(HttpStatusCode.OK,Fixtures);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        //this function is used to get fixtures and display for users and scoring.
        [HttpGet]
        public HttpResponseMessage GetUsersFixtures(int sportsId)
        {
            try
            {
                //var latestsesion = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                //var user = db.SessionSports.FirstOrDefault(s => s.managed_by == userid && s.session_id == latestsesion.id);
                var sport = db.Sports.FirstOrDefault(s => s.id == sportsId);
                // Query for the relevant fixtures based on the sport name
                var fixturesQuery =
                from f in db.Fixtures
                join t1 in db.Teams on f.team1_id equals t1.id into t1Teams
                from t1 in t1Teams.DefaultIfEmpty()
                join t2 in db.Teams on f.team2_id equals t2.id into t2Teams
                from t2 in t2Teams.DefaultIfEmpty()
                join w in db.Teams on f.winner_id equals w.id into winnerTeams
                from w in winnerTeams.DefaultIfEmpty()
                join ss in db.SessionSports on f.sessionSports_id equals ss.id
                join s in db.Sports on ss.sports_id equals s.id
                where s.id == sport.id
                select new
                {
                    fixture_id = f.id,
                    //teams1id=f.team1_id,
                    //teams2id = f.team2_id,
                    team1_name = f.team1_id == null ? "Yet to Decide" : (t1 != null ? t1.name : "Yet to Decide"),
                    team2_name = f.team2_id == null ? "Yet to Decide" : (t2 != null ? t2.name : "Yet to Decide"),
                    matchdate = f.matchDate,
                    venuee = f.venue,
                    winner_name = f.winner_id == null ? "Match Not Started" : (w != null ? w.name : "Match Not Started"),
                    winnerId = f.winner_id,
                    matchType = f.match_type,
                    sport_name = s.games,
                    sport_type = s.game_type
                };

                // Execute the query and get the results
                var results = fixturesQuery.ToList();
                if (!results.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                // Return the match list as a response
                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }
        //this function is used to get fixtures and display for users and scoring.
        [HttpGet]
        public HttpResponseMessage GetManagerFixtures(int userid)
        {
            try
            {
                var latestsesion = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();

                var user = db.SessionSports.FirstOrDefault(s => s.managed_by == userid && s.session_id == latestsesion.id);

                var fixturesQuery =
                from f in db.Fixtures
                join t1 in db.Teams on f.team1_id equals t1.id into t1Teams
                from t1 in t1Teams.DefaultIfEmpty()
                join t2 in db.Teams on f.team2_id equals t2.id into t2Teams
                from t2 in t2Teams.DefaultIfEmpty()
                join w in db.Teams on f.winner_id equals w.id into winnerTeams
                from w in winnerTeams.DefaultIfEmpty()
                join ss in db.SessionSports on f.sessionSports_id equals ss.id
                join s in db.Sports on ss.sports_id equals s.id
                where ss.managed_by == userid
                // ss.id == latestsesion.id &&
                select new
                {
                    fixture_id = f.id,
                    teams1id = f.team1_id,
                    teams2id = f.team2_id,
                    team1_name = f.team1_id == null ? "Not Selected" : (t1 != null ? t1.name : "Not Selected"),
                    team2_name = f.team2_id == null ? "Not Selected" : (t2 != null ? t2.name : "Not Selected"),
                    matchdate = f.matchDate,
                    venuee = f.venue,
                    winner_name = f.winner_id == null ? "Match Not Started" : (w != null ? w.name : "Match Not Started"),
                    winnerId = f.winner_id,
                    matchType = f.match_type,
                    sport_name = s.games,
                    sport_type = s.game_type
                };

                var results = fixturesQuery.ToList();
                if (!results.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No fixtures found for this user.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }



        [HttpGet]
        public HttpResponseMessage DisplayFixtures(int sportid)
        {
            try
            {
                // Get the latest session
                var latestsession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();

                if (latestsession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No sessions found.");
                }

                // Find the corresponding SessionSport ID for the latest session
                var sessionsportid = db.SessionSports.FirstOrDefault(s => s.session_id == latestsession.id && s.sports_id==sportid);

                if (sessionsportid == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No session sports found for the latest session.");
                }

                // Retrieve fixtures for the latest session, ensuring non-null team IDs and winner ID before joining
                var Fixtures = db.Fixtures
                    .Where(f => f.sessionSports_id == sessionsportid.id)
                    .Where(f => f.team1_id != null && f.team2_id != null && f.winner_id != null) // Ensure no null IDs before join
                    .Join(db.Teams,
                        f => f.team1_id,
                        t => t.id,
                        (f, team1) => new { f, team1 })
                    .Join(db.Teams,
                        f => f.f.team2_id,
                        t => t.id,
                        (f, team2) => new { f.f, f.team1, team2 })
                    .Join(db.Teams,
                        f => f.f.winner_id,
                        t => t.id,
                        (f, winner) => new { f.f, f.team1, f.team2, winner })
                    .Select(f => new
                    {
                        f.f.id,
                        team1 = f.team1.name,
                        team2 = f.team2.name,
                        f.f.venue,
                        f.f.matchDate,
                        winner = f.winner.name // Winner will always exist as we filtered out null before joining
                    })
                    .ToList();

                // Check if no fixtures are found
                if (!Fixtures.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No fixtures found for the latest session.");
                }

                // Return the fixtures
                return Request.CreateResponse(HttpStatusCode.OK, Fixtures);
            }
            catch (Exception ex)
            {
                // Return error message if something goes wrong
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

       

        [HttpPost]
 public HttpResponseMessage PostFixtures([FromBody] FixturesList schedulelist)
  {
    try
    { 
      var latestsession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault(); 
      var user = db.SessionSports.FirstOrDefault(s => s.managed_by == schedulelist.UserId && s.session_id==latestsession.id);
        //if (user == null)
        //{
        //    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found.");
        //}

        foreach (var schedule in schedulelist.Schedules)
        {
            try
            {
                int? team1Id = schedule.Team1_id == 0 ? (int?)null : schedule.Team1_id;
                int? team2Id = schedule.Team2_id == 0 ? (int?)null : schedule.Team2_id;

                var newFixture = new Fixture
                {
                    team1_id = team1Id,
                    team2_id = team2Id,
                    matchDate = schedule.MatchDate,
                    match_type = schedule.Match_type,
                    venue = schedule.Venue,
                    winner_id = null, 
                    sessionSports_id = user.id
                };

                db.Fixtures.Add(newFixture);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error while processing a fixture: {ex.Message}");
            }
        }
        db.SaveChanges();

        return Request.CreateResponse(HttpStatusCode.Created, "Fixtures added successfully.");
    }
    catch (Exception ex)
    {
        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
    }
}
        [HttpGet]
        public HttpResponseMessage UpdateTeams()
        {
            try
            {
                var latestSessions = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                var sessionsportsid = db.SessionSports.FirstOrDefault(s => s.session_id == latestSessions.id);
                var Matches = db.Fixtures.
                    Where(f => f.sessionSports_id == sessionsportsid.id).
                    Select(t => new { t.id, t.venue, t.match_type, t.matchDate }).
                    ToList();
                if (sessionsportsid == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No latest sessions teams Found");
                }
                if (Matches == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No league matches found.");
                }
                return Request.CreateResponse(HttpStatusCode.OK, Matches);
            }
            catch (Exception ex) {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);

            }
        }

        [HttpGet]
        public HttpResponseMessage GetManagerSport(int id)
        {
            try
            {
                var latestSessions = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                var sessionsports = db.SessionSports.FirstOrDefault(s => s.session_id == latestSessions.id && s.managed_by == id);
                //if (sessionsports == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.NotFound);
                //}
                var sportsData = db.Sports
                  .Where(s => s.id == sessionsports.sports_id)
                      .Select(s => new { game = s.games})
                        .FirstOrDefault();
                if (sportsData==null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                
                return Request.CreateResponse(HttpStatusCode.OK,sportsData);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);

            }
        }
        [HttpPut]
        public HttpResponseMessage UpdateFixtures([FromBody] FixtureUpdates fixtures)
        {
            try
            {

                foreach (var fixtureRequest in fixtures.UpdatedFixtures)
                {
                    var team1 = db.Teams.FirstOrDefault(t => t.id == fixtureRequest.Team1id);
                    var team2 = db.Teams.FirstOrDefault(t => t.id == fixtureRequest.Team2id);
                    if(team1 == null || team2 == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }
                     

                    var fixture = db.Fixtures.FirstOrDefault(f => f.id == fixtureRequest.Fixtureid);

                    if (fixture != null)
                    {
                        fixture.team1_id = team1.id;
                        fixture.team2_id = team2.id;
                    }
                }

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}