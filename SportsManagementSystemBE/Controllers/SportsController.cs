    using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SportsManagementSystemBE.Controllers
{
    public class SportsController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();
        [HttpGet]
        public HttpResponseMessage GetSports()
        {
            try
            {
                var sports = db.Sports
                               .Select(s => new { s.id, s.games })
                               .ToList();
                if (sports == null || !sports.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, sports);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetSportsandManagers()
        {
            try
            {
                var sports = db.Sports
                               .Select(s => new { s.id, s.games })
                               .ToList();

                var eventManagers = db.Users
                                      .Where(u => u.role == "EventManager")
                                      .Select(u => new { u.id, u.name, u.registration_no })
                                      .ToList();

                if ((sports == null || !sports.Any()) && (eventManagers == null || !eventManagers.Any()))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No data found.");
                }

                var responseContent = new
                {
                    Sports = sports,
                    EventManagers = eventManagers
                };

                return Request.CreateResponse(HttpStatusCode.OK, responseContent);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage SaveManagersdata(SessionSport data)
        {
            try
            {
                var latestSessiondate = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();
                if (latestSessiondate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                // Set the session ID before checking for duplicates
                data.session_id = latestSessiondate.id;

                // Check if the event manager is already managing any game in the current session
                var existingmanagercheck = db.SessionSports.FirstOrDefault(gs => gs.managed_by == data.managed_by && gs.session_id == latestSessiondate.id);
                if (existingmanagercheck != null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }


                // Check if the game is already added in the latest session
                var uniqueGameperSession = db.SessionSports.FirstOrDefault(gs => gs.sports_id == data.sports_id && gs.session_id == latestSessiondate.id);
                if (uniqueGameperSession != null)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);
                }

                // Add the game to the latest session
                db.SessionSports.Add(data);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        //this is for creating user to eventmanager.
        [HttpPost]
        public HttpResponseMessage PostEventManagers(User data)
        {
            try
            {
                //if (data == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user data.");
                //}
                var existingUser = db.Users.Any(u => u.registration_no == data.registration_no);
                if (!existingUser)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User not found.");
                }
                var alredymanager = db.Users.Any(u => u.role == "EventManager" && u.registration_no == data.registration_no);
                if (alredymanager)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Already Manager");
                }

                var updatedManager = db.Users.FirstOrDefault(u => u.registration_no == data.registration_no);
                if (updatedManager != null)
                {
                    updatedManager.role = "EventManager";

                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.Created, updatedManager);
                }

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Unable to update user.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

    }
}