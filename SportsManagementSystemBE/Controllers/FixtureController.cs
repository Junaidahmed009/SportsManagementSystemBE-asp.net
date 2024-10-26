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
        public HttpResponseMessage PostFixtures(Fixture fixture) {
            try
            {
                bool team1idexists = db.Teams.Any(t => t.id == fixture.team1_id);
                bool team2idexists = db.Teams.Any(t => t.id == fixture.team2_id);
                if (!team1idexists || !team2idexists)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,"Temas id not found");
                }
                if (fixture == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Team data is empty");

                }
                db.Fixtures.Add(fixture);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);

            }
            catch (Exception ex) {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            
        }
        
        }

        [HttpPost]
        public HttpResponseMessage postFixtureimages(FixturesImage fixture)
        {
            try
            {
                var fixtureid = db.Fixtures.Any(f => f.id == fixture.fixtures_id);
                if (fixtureid)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,"No id found");
                }
                db.FixturesImages.Add(fixture);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }

        }
    }
}