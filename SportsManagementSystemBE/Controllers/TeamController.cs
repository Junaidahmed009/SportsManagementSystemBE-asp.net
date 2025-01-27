using Microsoft.Ajax.Utilities;
using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Http;
using System.Web.WebPages;

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
                if (checkname)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Team is already registered");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }
        [HttpGet]
        public HttpResponseMessage AllApprovedTeams(int id)
        {
            try
            {
                var eventManager = db.Users.FirstOrDefault(u => u.id == id);
                var latestSession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();
                var sessionsport = db.SessionSports.FirstOrDefault(ss => ss.managed_by == eventManager.id && ss.session_id == latestSession.id);
                var approvedTeams = db.Teams
                                      .Where(t => t.teamStatus == true &&
                                      t.session_id == latestSession.id &&
                                      t.sports_id == sessionsport.sports_id)
                                      .Select(t => new { teamid = t.id, teamname = t.name })
                                      .ToList();

                if (!approvedTeams.Any())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No approved teams found.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, approvedTeams);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetAllLatestTeams(int userId)
        {
            try
            {
                // Get the latest session
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                if (latestSession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No sessions found.");
                }

                // Get user sports data for the latest session
                var userData = db.SessionSports.FirstOrDefault(s => s.managed_by == userId && s.session_id == latestSession.id);
                if (userData == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User does not manage any sports for the latest session.");
                }

                // Get the list of teams for the user's sport in the latest session
                var latestTeamsList = db.Teams
                    .Where(t => t.sports_id == userData.sports_id && t.session_id == latestSession.id)
                    .Select(t => new { t.id, t.name })
                    .ToList();

                if (!latestTeamsList.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No teams found for the specified sport and session.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, latestTeamsList);
            }
            catch (Exception ex)
            {
                // Log exception (e.g., using a logging framework)
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddTeam(TeamDTOs teamlist)
        {
            try
            {
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                if (latestSession == null || latestSession.startDate < DateTime.Now)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorcode = 1 });//, message = "No active session found." 
                }
                // Check if the team name already exists in the session
                if (db.Teams.Any(t => t.name == teamlist.Name && t.session_id == latestSession.id))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, new { errorcode = 3 });//, message = "Team with the same name already exists in this session."
                }
                //check if Caption exists in user table.
                var user = db.Users.FirstOrDefault(u => u.id == teamlist.Caption_id);
                if (user == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorcode = 5 });//, message = "User not found." 
                }
                //check if the caption exists in student table.
                var student = db.Students.FirstOrDefault(s => s.reg_no == user.registration_no);
                if (student == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorcode = 6 });//, message = "Student not found."
                }

                var sport = db.Sports.FirstOrDefault(s => s.id == teamlist.Sports_id);
                if (sport == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { errorcode = 7 });//, message = "Sport not found." 
                }

                // Check if the user is already a captain in the latest session for the same sport
                if (db.Teams.Any(t => t.caption_id == user.id && t.session_id == latestSession.id && t.sports_id == teamlist.Sports_id))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, new { errorcode = 4 });//, message = "User is already a captain of a team for the same sport in this session."
                }

                // Create team
                var newTeam = new Team
                {
                    name = teamlist.Name,
                    className = teamlist.ClassName,
                    caption_id = teamlist.Caption_id,
                    session_id = latestSession.id,
                    sports_id = teamlist.Sports_id,
                    image_path = teamlist.Image_path,
                    teamStatus = teamlist.TeamStatus,
                    teamGender = student.gender,
                };

                db.Teams.Add(newTeam);
                db.SaveChanges();


                // Add player data if SingleUser
                if (teamlist.TeamType == "SingleUser")
                {
                    var player = new Player
                    {
                        reg_no = student.reg_no,
                        team_id = newTeam.id
                    };

                    db.Players.Add(player);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                var response = new
                {
                    newTeam.id
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                // Log the error (logging mechanism not shown here)
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage PlayingTeams(int fixtureId)
        {
            try
            {
                // Fetch fixture details
                var fixture = db.Fixtures.FirstOrDefault(f => f.id == fixtureId);
                if (fixture == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Fixture not found.");
                }

                // Fetch team details and players in a single step for both teams
                var team1Details = db.Teams
                    .Where(t => t.id == fixture.team1_id)
                    .Select(t => new
                    {
                        TeamId = t.id,
                        TeamName = t.name ?? "Team undecided",
                        Players = db.Players
                            .Where(p => p.team_id == t.id)
                            .Join(
                                db.Students,
                                p => p.reg_no,
                                s => s.reg_no,
                                (p, s) => new { p.id, PlayerName = s.name }
                            ).ToList()
                    })
                    .FirstOrDefault();

                var team2Details = db.Teams
                    .Where(t => t.id == fixture.team2_id)
                    .Select(t => new
                    {
                        TeamId = t.id,
                        TeamName = t.name ?? "Team undecided",
                        Players = db.Players
                            .Where(p => p.team_id == t.id)
                            .Join(
                                db.Students,
                                p => p.reg_no,
                                s => s.reg_no,
                                (p, s) => new { p.id, PlayerName = s.name }
                            ).ToList()
                    })
                    .FirstOrDefault();

                // Combine results into a response object
                var result = new
                {
                    Team1 = team1Details,
                    Team2 = team2Details
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetunapprovedTeams(int id)
        {
            try
            {
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                var userdata = db.SessionSports.FirstOrDefault(s => s.managed_by == id && s.session_id == latestSession.id);
                if (userdata == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                var latestteams = db.Teams
                    .Where(t => t.sports_id==userdata.sports_id)
                    .Join(db.Users,
                    t => t.caption_id,
                    u => u.id,
                    (t, u) => new
                    { t.id, t.name, t.image_path, t.teamStatus, regno = u.registration_no, username = u.name })
                    .ToList();
                if (userdata == null && latestteams==null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK, latestteams);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        //AllTeamsbyem
        [HttpGet]
        public HttpResponseMessage GetCricketTeams()
        {
            try
            {
                var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                var CricketId = db.Sports.
                    Where(s => s.games == "Cricket").
                    Select(s => s.id).SingleOrDefault();
                var latestCricketteams = db.Teams
                    .Where(t => t.session_id == latestSession.id && t.sports_id == CricketId)
                    .Join(db.Users,
                    t => t.caption_id,
                    u => u.id,
                    (t, u) => new
                    { t.id, t.name, t.image_path, t.teamStatus, regno = u.registration_no, username = u.name })
                    .ToList();



                //.Select(t => new { t.id, t.name,t.image_path,t.teamStatus })
                //.ToList();
                if (latestCricketteams == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, latestCricketteams);


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //allteambyid
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

        [HttpPost]
        public HttpResponseMessage UploadImage()
        {
            try
            {
                // Get the files from the request (multiple files support)
                var httpRequest = HttpContext.Current.Request;//accepts full request
                var uploadedFiles = httpRequest.Files;//handle file  from object coming from Frontend.

                //// Check if no files are uploaded
                //if (uploadedFiles.Count == 0)
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "No images uploaded.");
                //}

                // Define valid image extensions
                var validExtensions = new List<string> { ".jpeg", ".jpg", ".png", ".webp", ".gif" };

                // List to hold the image paths to return in the response
                var imagePaths = new List<string>();

                for (int i = 0; i < uploadedFiles.Count; i++)
                {
                    var uploadedFile = uploadedFiles[i];

                    // Check if the file is valid
                    if (uploadedFile == null || uploadedFile.ContentLength == 0)
                    {
                        continue; // Skip invalid files
                    }

                    // Check if the file extension is valid
                    var extension = Path.GetExtension(uploadedFile.FileName).ToLower();
                    if (!validExtensions.Contains(extension))
                    {
                        continue; // Skip invalid files
                    }

                    // Generate a unique file name for each image
                    var fileName = Guid.NewGuid() + extension;

                    // Define the upload path for the image inside Resources/uploads/teamPics
                    var uploadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "uploads", "teamPics");

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Full file path to save the image
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Save the image to the file system
                    uploadedFile.SaveAs(filePath);

                    // Add the relative image path to the list (corrected to /uploads/teamPics/)
                    var relativePath = $"/uploads/teamPics/{fileName}";
                    imagePaths.Add(relativePath);
                }

                // Check if any images were successfully uploaded
                if (imagePaths.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No valid images uploaded.");
                }

                // Return the image paths as a response
                return Request.CreateResponse(HttpStatusCode.OK, imagePaths);
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUserAppliedTeams(int userId)
        {
            try
            { 
                var result = db.Teams
                    .Where(p => p.caption_id == userId)
                    .Join(db.Sports,
                        team => team.sports_id,
                        sport => sport.id,
                        (team, sport) => new {
                            Tname = team.name,
                            teamStatus = team.teamStatus,
                            image_path = team.image_path,
                            className = team.className,
                            sport = sport.games // Include sport name here
                        })
                    .ToList();

                //// Fetch the latest session
                //var latestSession = db.Sessions.OrderByDescending(s => s.startDate).FirstOrDefault();
                //if (latestSession == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.NotFound, "No active session found.");
                //}

                //// Fetch teams associated with the current user
                //var teams = db.Teams
                //    .Where(t => t.caption_id == userId)
                //    .Select(t => new { t.id, t.sports_id })
                //    .ToList();

                //if (!teams.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.NotFound, "No teams associated with the user.");
                //}

                //// Check if the latest session is valid for any of the user's teams
                //var teamSportIds = teams.Select(t => t.sports_id).ToList();
                //var latestSessionSport = db.SessionSports
                //    .FirstOrDefault(s => s.session_id == latestSession.id && teamSportIds.Contains(s.sports_id));

                //if (latestSessionSport == null)
                //{
                //    return Request.CreateResponse(HttpStatusCode.NotFound, "No sports associated with the latest session for the user's teams.");
                //}

                //// Query for the relevant fixtures based on the latest session and sports
                //var fixturesQuery =
                //    from f in db.Fixtures
                //    join t1 in db.Teams on f.team1_id equals t1.id into t1Teams
                //    from t1 in t1Teams.DefaultIfEmpty()
                //    join t2 in db.Teams on f.team2_id equals t2.id into t2Teams
                //    from t2 in t2Teams.DefaultIfEmpty()
                //    join w in db.Teams on f.winner_id equals w.id into winnerTeams
                //    from w in winnerTeams.DefaultIfEmpty()
                //    join ss in db.SessionSports on f.sessionSports_id equals ss.id
                //    join s in db.Sports on ss.sports_id equals s.id
                //    where ss.session_id == latestSession.id && teamSportIds.Contains(ss.sports_id)
                //    select new
                //    {
                //        fixture_id = f.id,
                //        team1_name = f.team1_id == null ? "Yet to Decide" : (t1 != null ? t1.name : "Yet to Decide"),
                //        team2_name = f.team2_id == null ? "Yet to Decide" : (t2 != null ? t2.name : "Yet to Decide"),
                //        matchdate = f.matchDate,
                //        venuee = f.venue,
                //        winner_name = f.winner_id == null ? "Match Not Started" : (w != null ? w.name : "Match Not Started"),
                //        winnerId = f.winner_id,
                //        matchType = f.match_type,
                //        sport_name = s.games,
                //        sport_type = s.game_type
                //    };

                //// Execute the query and get the results
                //var results = fixturesQuery.ToList();
                //if (!results.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.NotFound, "No fixtures found for the latest session.");
                //}

                //// Return the combined data (results and teams)
                //var allData = new { results, teams };
                return Request.CreateResponse(HttpStatusCode.OK, result);

               
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }






    }
}
