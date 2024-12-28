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
using System.Web;
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
        [HttpGet]
        public HttpResponseMessage AllApprovedTeams(int id)
        {
            try
            {
                var eventManager = db.Users.FirstOrDefault(u => u.id == id);
                var latestSession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();
                var sessionsport = db.SessionSports.FirstOrDefault(ss => ss.managed_by == eventManager.id && ss.session_id==latestSession.id);
                var approvedTeams = db.Teams
                                      .Where(t => t.teamStatus == true &&
                                      t.session_id == latestSession.id &&
                                      t.sports_id == sessionsport.sports_id)
                                      .Select(t => new { teamid=t.id, teamname=t.name })
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
                var latestSession = db.Sessions.OrderByDescending(s => s.endDate).FirstOrDefault();
                if (latestSession == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict,new {errorcode=1});//"not found latest session"
                }
                if (DateTime.Now > latestSession.endDate)
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
                var httpRequest = HttpContext.Current.Request;
                var uploadedFiles = httpRequest.Files;

                // Check if no files are uploaded
                if (uploadedFiles.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No images uploaded.");
                }

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
        public HttpResponseMessage GetImage(string imagePath)
        {
            try
            {
                // Map the relative path to the physical file path on the server
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "uploads", imagePath.TrimStart('/'));

                // Check if the file exists
                if (!File.Exists(fullPath))
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Image not found.");
                }

                // Read the image file as bytes
                var imageBytes = File.ReadAllBytes(fullPath);

                // Get the file extension to set the correct content type
                var fileExtension = Path.GetExtension(imagePath).ToLower();
                string contentType;

                switch (fileExtension)
                {
                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    case ".webp":
                        contentType = "image/webp";
                        break;
                    default:
                        contentType = "application/octet-stream"; // Default for unknown file types
                        break;
                }

                // Create the response with the image content
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(imageBytes);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }





    }
}
