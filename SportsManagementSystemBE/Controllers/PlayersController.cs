using Microsoft.Win32;
using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace SportsManagementSystemBE.Controllers
{ 
    public class PlayersController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpGet]
        public HttpResponseMessage HandleUser(int userid)
        {
            //if (userRequest.UserId == 0)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest, "UserId is required.");
            //}

            try
            {
                var userdata = db.Users.FirstOrDefault(u => u.id == userid);
                var Studentdata = db.Students.
                    Where(s => s.reg_no == userdata.registration_no).
                Select(s=>new { Reg_no = s.reg_no, Name = s.name, Gender=s.gender}).
                ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, Studentdata);
                
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