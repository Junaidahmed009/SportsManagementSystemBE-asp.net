using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SportsManagementSystemBE.Controllers
{
    public class FixtureController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpGet]
        public HttpResponseMessage GetFixtures()
        {
            try
            {
                var latestsession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();

                if (latestsession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No sessions found.");
                }

                var latestsessionteams = db.Teams
                    .Where(t => t.session_id == latestsession.id)
                    .Select(t => new { t.id, t.name, t.className })
                    .ToList();

                if (latestsessionteams.Count == 0) 
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No teams found for the latest session.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, latestsessionteams);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
[HttpPost]
public HttpResponseMessage PostFixtures([FromBody] FixturesList schedulelist)
{
    try
    {
        //// Validate the request object
        //if (schedulelist == null || schedulelist.Schedules == null || !schedulelist.Schedules.Any())
        //{
        //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid request: No schedules provided.");
        //}

        // Check if the user exists
        var user = db.SessionSports.FirstOrDefault(s => s.managed_by == schedulelist.UserId);
        //if (user == null)
        //{
        //    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found.");
        //}

        foreach (var schedule in schedulelist.Schedules)
        {
            try
            {
                // Replace `0` with `null` for team IDs
                int? team1Id = schedule.Team1_id == 0 ? (int?)null : schedule.Team1_id;
                int? team2Id = schedule.Team2_id == 0 ? (int?)null : schedule.Team2_id;

                // Create a new fixture
                var newFixture = new Fixture
                {
                    team1_id = team1Id,
                    team2_id = team2Id,
                    matchDate = schedule.MatchDate,
                    match_type = schedule.Match_type,
                    venue = schedule.Venue,
                    winner_id = null, // Default to null
                    sessionSports_id = user.id
                };

                // Add the fixture to the database
                db.Fixtures.Add(newFixture);
            }
            catch (Exception ex)
            {
                // Stop immediately and return the error response
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error while processing a fixture: {ex.Message}");
            }
        }

        // Save changes to the database
        db.SaveChanges();

        return Request.CreateResponse(HttpStatusCode.Created, "Fixtures added successfully.");
    }
    catch (Exception ex)
    {
        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
    }
}




        //[HttpPost]
        //public HttpResponseMessage PostFixtureimages(FixturesImage fixture)
        //{
        //    try
        //    {
        //        var fixtureid = db.Fixtures.Any(f => f.id == fixture.fixtures_id);
        //        if (fixtureid)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound,"No id found");
        //        }
        //        db.FixturesImages.Add(fixture);
        //        db.SaveChanges();
        //        return Request.CreateResponse(HttpStatusCode.Created);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
        //    }

        //}
    }
}