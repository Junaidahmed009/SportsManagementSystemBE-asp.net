using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using SportsManagementSystemBE.Models;

namespace SportsManagementSystemBE.Controllers
{
    public class ChairpersonController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        //[HttpGet]
        //public HttpResponseMessage GetEventmanagerselection()
        //{
        //    try
        //    {

        //        {
        //            // First query to fetch data from the User table
        //            var userList = (from u in db.Users
        //                            select new
        //                            {
        //                                userid=u.id,
        //                                username=u.name,
        //                                //regno = u.registration_no
        //                            }).ToList();

        //            // Second query to fetch data from the Sports table
        //            var sportsList = (from s in db.Sports
        //                              select new
        //                              {
        //                                  sportid=s.id,
        //                                  Sports = s.games
        //                              }).ToList();

        //            // Returning two lists as part of a single response
        //            var response = new
        //            {
        //                Users = userList,
        //                Sports = sportsList
        //            };

        //            // Returning the response with the data
        //            return Request.CreateResponse(HttpStatusCode.OK, response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}


        //rule of games.
        [HttpGet]
        public HttpResponseMessage GetSports()
        {
            try
            {
                var sports = db.Sports
                                     .Select(s => new { s.id, s.games })
                                     .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, sports); // Returning 200 OK with the sports data
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }



    }
}