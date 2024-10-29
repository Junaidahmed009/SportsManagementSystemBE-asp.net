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


        [HttpPost]
        public HttpResponseMessage Teamandstudentsdata(Team team,Student student)
        {
            try
            {
                ////Check if the Object is null or not get all its properties and checked.
                //if (team == null || team.GetType().GetProperties().Any(prop => prop.GetValue(team) == null))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Team data is incomplete or missing.");
                //}
                if (team == null || team.name == null || team.className == null || team.managed_by==0  || team.image_path == null) {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Team data is incomplete or missing.");
                }

                // Step 2: Map the Team object to a TeamDTO
                TeamDTOs teamDto = new TeamDTOs
                {
                    Name = team.name,
                    ClassName = team.className,
                    Managed_by = team.managed_by,
                    Image_path = team.image_path,
                    TeamStatus=team.teamStatus
                       
                };

                //// Step 3: Validate the DTO (Team name and class name cannot be empty)
                //if (string.IsNullOrEmpty(teamDto.Name) || string.IsNullOrEmpty(teamDto.ClassName))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Team name and class name cannot be empty.");
                //}

                //Pick the lateset session id.
                var latestSession=db.Sessions.OrderByDescending(s=>s.endDate).FirstOrDefault();
                if (latestSession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound,"not  found any latest sessions");
                }

                var UserExists = db.Users.Any(s => s.id == teamDto.Managed_by);
                //var sportsExists = db.Sports.Any(s => s.id == teamDto.Sports_id);
                if (!UserExists)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No user Found in table");
                }

                var sessionsportsdata=db.SessionSports.FirstOrDefault(s=>s.managed_by == teamDto.Managed_by &&
                s.session_id==latestSession.id);
                if (sessionsportsdata==null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "the data from session sports table is empty");
                }

                // Step 5: Check if a team with the same name already exists in the same session
                var teamExists = db.Teams.Any(t => t.name == teamDto.Name &&
                                                    t.session_id == teamDto.Session_id);

                if (teamExists)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "A team with this name already exists in this session and sport.");
                }
                if (teamDto.TeamStatus != true && teamDto.TeamStatus != false)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid TeamStatus value: it must be true (1) or false (0).");
                }



                // Step 6: Create a new Team entity based on the DTO in db
                var newTeam = new Team
                {
                    name = teamDto.Name,
                    className = teamDto.ClassName,
                    session_id = latestSession.id,
                    managed_by = teamDto.Managed_by,
                    sports_id = sessionsportsdata.sports_id,    
                    image_path = teamDto.Image_path,
                    teamStatus = teamDto.TeamStatus,
                    
                };

                db.Teams.Add(newTeam);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created,newTeam);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




    }
}
