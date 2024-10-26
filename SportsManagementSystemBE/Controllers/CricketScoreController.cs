using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Management;

namespace SportsManagementSystemBE.Controllers
{
    public class CricketScoreController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpPost]
        public HttpResponseMessage CricketScorepost(CricketScore cricketScore)
        {
            try
            {
                if (cricketScore==null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                var fixtureteamidcheck = db.Fixtures.Any(f => f.id == cricketScore.fixture_id && db.Teams.Any(t => t.id == cricketScore.team_id));
                if (fixtureteamidcheck)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                
                db.CricketScores.Add(cricketScore);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex) {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }
    }
}