using Microsoft.Ajax.Utilities;
using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static System.Collections.Specialized.BitVector32;

namespace SportsManagementSystemBE.Controllers
{
    public class TeamController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpGet]
        public HttpResponseMessage CheckTeamNames(string tname)
        {
            try
            {
                var checkname = db.Teams.Any(t => t.name == tname);
                if (checkname) {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Team is already registered");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        public HttpResponseMessage GetLatestCricketTeams()
        {
            try
            {
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                var latestteamsList = db.Teams
                    .Where(t => t.session_id == latestSession.id)
                    .Select(t => new { t.id, t.name })
                    .ToList();
                if (latestSession == null || latestteamsList == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);
                }
                return Request.CreateResponse(HttpStatusCode.OK, latestteamsList);


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage PostTeamadata(Team team)
        {
            try
            {
                TeamDTOs teamDto = new TeamDTOs
                {
                    Name = team.name,
                    ClassName = team.className,
                    Caption_id=team.caption_id,
                    Sports_id = team.sports_id,
                    Image_path = team.image_path,
                    TeamStatus = team.teamStatus,
                    //Session_id = latestSession.id // Assign session ID from latest session
                };
                //Get Latest session object
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                if (latestSession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict,new {errorcode=1});//"not found latest session"
                }
                if (DateTime.Now > latestSession.startDate)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict,new { errorcode = 2});//"The latest session has ended"
                }
                // Check if the Team name already exists in this session
                bool teamnameExists = db.Teams.Any(t => t.name == teamDto.Name && t.session_id == latestSession.id );
                if (teamnameExists)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, new { errorcode = 3});//, "team exists with the same name"
                }

                // Check if the sport exists in the sports table
                var sportsExists = db.Sports.FirstOrDefault(s => s.id == teamDto.Sports_id);
                if (sportsExists == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorcode = 1});//"No sport exists with the given ID."
                }

                // Check if the captain exists in the users table with the appropriate role
                var captionExists = db.Users.FirstOrDefault(u => u.id == teamDto.Caption_id &&
                                                                  (u.role == "user" || u.role == "caption"));
                if (captionExists == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorcode = 2});
                }
                //check if the user is caption of another team in latest session
                bool isalreadycaption = db.Teams.Any(t => t.caption_id == captionExists.id && t.session_id == latestSession.id);
                if (isalreadycaption) {
                    return Request.CreateResponse(HttpStatusCode.Conflict, new { errorcode = 4});//"User is already the captain of a team in the latest session."
                }
                //team status
                if (teamDto.TeamStatus != true && teamDto.TeamStatus != false)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, new { errorcode = 5}); //"Invalid TeamStatus value: it must be true (1) or false (0)."
                }
                var newTeam = new Team
                {
                    name = teamDto.Name,
                    className = teamDto.ClassName,
                    caption_id = teamDto.Caption_id,
                    session_id = latestSession.id,
                    sports_id = teamDto.Sports_id,
                    image_path = teamDto.Image_path,
                    teamStatus = teamDto.TeamStatus
                };

                db.Teams.Add(newTeam);
                db.SaveChanges();
                //sending object to front end.
                var response = new
                {
                    newTeam.id
                };

                return Request.CreateResponse(HttpStatusCode.Created,response);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
       

        public HttpResponseMessage GetCricketTeams()
        {
            try
            {
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                var CricketId = db.Sports.
                    Where(s => s.games == "Cricket").
                    Select(s => s.id).SingleOrDefault();
                var latestCricketteams = db.Teams
                    .Where(t => t.session_id == latestSession.id && t.sports_id==CricketId)
                    .Join(db.Users,
                    t=>t.caption_id,
                    u=>u.id,
                    (t,u) => new
                    {t.id,t.name,t.image_path,t.teamStatus,regno=u.registration_no,username=u.name})
                    .ToList();

                    

                    //.Select(t => new { t.id, t.name,t.image_path,t.teamStatus })
                    //.ToList();
                if (latestCricketteams==null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK,latestCricketteams);


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPut]
        public HttpResponseMessage UpdateTeamStatus(int id)
        {
            try
            {
                var teamdetails = db.Teams.FirstOrDefault(t => t.id == id);
                if (teamdetails == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Team not found.");
                }

                if (teamdetails.teamStatus)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Team status is already true.");
                }

                // Update the team's status
                teamdetails.teamStatus = true;
                db.Entry(teamdetails).State = EntityState.Modified;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Team status updated successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
