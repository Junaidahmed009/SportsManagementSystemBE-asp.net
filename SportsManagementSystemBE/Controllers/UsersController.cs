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
    public class UsersController : ApiController
    {
        private SportsManagementSystemEntities db=new SportsManagementSystemEntities();


        [HttpGet]
        public HttpResponseMessage GetUsers()
        {
            try
            {
                var users = db.Users.ToList();

                if (users == null || users.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No users found.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, users);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        public HttpResponseMessage SignupUser(User user)
        {
            try
            {
                var existingUser = db.Users
                    .FirstOrDefault(u => u.registration_no == user.registration_no);

                if (existingUser != null)
                {
                   
                    return Request.CreateResponse(HttpStatusCode.Conflict);
                }
                //var nameExists = db.Users
                //    .FirstOrDefault(u => u.name == user.name);

                //if (nameExists != null)
                //{
                    
                //    return Request.CreateResponse(HttpStatusCode.BadRequest);
                //}

                db.Users.Add(user);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        //[HttpPost]
        //public HttpResponseMessage LoginUser(User logindata)
        //{
        //    try
        //    {
        //        var user = db.Users.FirstOrDefault(u => u.registration_no == logindata.registration_no);

        //        if (user == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound); 
        //        }

        //        if (user.password != logindata.password)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.Unauthorized); 
        //        }

        //        var userid = db.Users.FirstOrDefault(u => u.registration_no == logindata.registration_no);
        //        var latestSession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();
        //        var checkmangerid=db.SessionSports
        //            .Where(s=>s.managed_by==)


        //        var responseUser = new
        //        {
        //            user.name,
        //            user.registration_no,
        //            user.role
        //        };

        //        return Request.CreateResponse(HttpStatusCode.OK, responseUser); 
        //    }
        //    catch (Exception ex)
        //    {

        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
        //    }
        //}

        [HttpPost]
        public HttpResponseMessage LoginUser(User logindata)
        {
            try
            {
                var user = db.Users.FirstOrDefault(u => u.registration_no == logindata.registration_no && u.password==logindata.password);

                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                //if (user.password != logindata.password)
                //{
                //    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                //}

                // Get the latest session based on endDate
                var latestSession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();

                // Prepare response data
                var responseUser = new
                {
                    user.id,
                    user.name,
                    user.registration_no,
                    user.role
                };

                // Handle conditions based on user role
                if (user.role == "EventManager")
                {
                    if (latestSession != null)
                    {
                        var sessionSport = db.SessionSports
                            .FirstOrDefault(s => s.managed_by == user.id && s.session_id == latestSession.id);

                        if (sessionSport != null)
                        {
                            var responseWithSport = new
                            {
                                user.id,
                                user.name,
                                user.registration_no,
                                user.role,
                                SportId = sessionSport.sports_id
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, responseWithSport);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.Conflict);//, "Event manager does not have a managed session."
                        }
                    }
                }
                else if (user.role == "Admin" || user.role == "user")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, responseUser);
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest);//, "Invalid role."
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        public HttpResponseMessage Forgetpasswordverification(User data)
        {
            try
            {
                var user = db.Users.FirstOrDefault(u => u.name == data.name && u.registration_no == data.registration_no);

                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut]
        public HttpResponseMessage Submitnewpassword(User data)
        {
            try
            {
                var user = db.Users.FirstOrDefault( u=>u.name == data.name && u.registration_no == data.registration_no);

                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//404
                }
                user.password = data.password;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created);//201
            }
            catch (Exception ex)
            {
                // Return 500 Internal Server Error with the exception message
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }



    }
}