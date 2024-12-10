using Microsoft.Win32;
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
    public class PlayersController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpPost]
        public HttpResponseMessage HandleUser([FromBody] UserRequest userRequest)
        {
            if (userRequest.UserId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "UserId is required.");
            }

            try
            {
                var userdata = db.Users.FirstOrDefault(u => u.id == userRequest.UserId);

                if (userdata == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "User not found.");
                }
                var UserExists = db.Students.Any(s => s.reg_no == userdata.registration_no);
                if (!UserExists)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, $"Student with roll number {userdata.registration_no} does not exist."
                }

                if (userRequest.TeamNo == null)
                {
                    var data = new { userdata.name, userdata.registration_no };
                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
                else
                {
                    var playerData = new Player
                    {
                        reg_no = userdata.registration_no,
                        team_id = userRequest.TeamNo.Value
                    };
               
                    db.Players.Add(playerData);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, "Player created successfully.");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public HttpResponseMessage PostPlayers([FromBody] PlayersRequest request)
        {
            try
            {
                //if (request.RollNumbers == null || request.RollNumbers.Count == 0 || request.TeamNo <= 0)
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid roll numbers or team ID");
                //}

                //list to add players and teamid for full team entry at once 
                //var userdata = db.Users.FirstOrDefault(u => u.id == request.UserId);
                //if (userdata == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user ID.");
                //}
                //var userdataExistsAsPlayer = db.Players.Any(p => p.reg_no == userdata.registration_no && p.team_id == request.TeamNo);
                //if (userdataExistsAsPlayer)
                //{
                //    return Request.CreateResponse(HttpStatusCode.Conflict);// $\"User with registration number  {userdata.registration_no}  is already a player in team {request.TeamNo}
                //}


                //playersToAdd.Add(new Player
                //{
                //    reg_no = userdata.registration_no,
                //    team_id = request.TeamNo

                //});

                var playersToAdd = new List<Player>();
                foreach (var rollno in request.RollNumbers.Distinct())
                {
                    var playerExists = db.Players.Any(p => p.reg_no == rollno && p.team_id == request.TeamNo);
                    if (playerExists)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest);//$"Player with roll number {rollno} already exists in team {request.TeamNo}."
                    }
                    var UserExists = db.Students.Any(s => s.reg_no == rollno);
                    if (!UserExists)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);//, $"Student with roll number {userdata.registration_no} does not exist."
                    }

                    playersToAdd.Add(new Player
                    {
                        reg_no = rollno,
                        team_id = request.TeamNo
                    });
                }

                if (playersToAdd.Any())
                {
                    db.Players.AddRange(playersToAdd);
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.Created);// "Players added successfully"
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetTeamPlayers(int id)
        {
            try
            {
                var TeamDetails = db.Players.
                    Where(p => p.team_id == id)
                    .Join(db.Students,
                    p => p.reg_no,
                    s => s.reg_no,
                    (p, s) => new { p.id, p.reg_no, names = s.name, })
                    .ToList();


                //.Select(t => new { t.id, t.name,t.image_path,t.teamStatus })
                //.ToList();
                if (TeamDetails == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, TeamDetails);


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



    }
}