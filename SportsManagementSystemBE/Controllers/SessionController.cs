using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SportsManagementSystemBE.Models;

namespace SportsManagementSystemBE.Controllers
{

    public class SessionController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpGet]
        public HttpResponseMessage GetsessionsandSports()
        {
            try
            {
                var sports=db.Sports.
                    Select(s => new {s.id,s.games}).
                    ToList();
                var sessionDetails = db.Sessions
               .Select(s => new { s.id, s.name })
               .ToList();
                var response = new
                {
                    Sport = sports,
                    session = sessionDetails
                };
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }

        }
        

        [HttpPost]
        public HttpResponseMessage PostSession(Session session)
        {
            try
            {
                var existingname=db.Sessions.FirstOrDefault(s=> s.name == session.name);
                if (existingname != null) { 
                return Request.CreateResponse(HttpStatusCode.Conflict);
                }
                db.Sessions.Add(session);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex.Message);
            }
        }



    }
}