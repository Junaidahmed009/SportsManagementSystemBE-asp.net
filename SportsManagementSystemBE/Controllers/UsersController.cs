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

                // Check if there are no users in the database
                if (users == null || users.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No users found.");
                }

                // Return the list of users with a 200 OK status
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

                if (existingUser != null)  // Corrected logic: if a user exists, return conflict
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);
                }

                db.Users.Add(user);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created);  // Return 201 Created without any additional data
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage LoginUser(User logindata)
        {
           // if (logindata == null)
            //{
              //  return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

            // Find the user by registration number
            var user = db.Users.FirstOrDefault(u => u.registration_no == logindata.registration_no);

            if (user == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);//, "Registration number is incorrect"
            }

            // Verify the password
            if (user.password != logindata.password)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);//, "Password is incorrect"
            }

            // Return user details (without password) on successful login
            var responseUser = new
            {
                user.name,
                user.registration_no,
                user.role
            };

            return Request.CreateResponse(HttpStatusCode.OK, responseUser); // Status 200 if both registration number and password are correct
        }


    }
}