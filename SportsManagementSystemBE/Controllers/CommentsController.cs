using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SportsManagementSystemBE.Controllers
{
    public class CommentsController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpPost]
        public HttpResponseMessage PostComments(Comment comment)
        {
            try
            {
                var fixtureid = db.Fixtures.Any(f => f.id == comment.fixtures_id);
                if (fixtureid)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No id found");
                }
                db.Comments.Add(comment);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }

        }
    }
}
