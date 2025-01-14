using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SportsManagementSystemBE.Controllers
{
    public class ScoringController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();
        [HttpPost]
        public HttpResponseMessage AddOrUpdateCricketScore(CricketScoringDTO cric)
        {
            try
            {
                // Fetch team based on teamName
                var team = db.Teams.FirstOrDefault(t => t.id == cric.Teamid);

                //if (team == null)
                //{
                //    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Team not found");
                //}

                // Check if the fixture_id exists and if the team_id is part of the fixture
                var fixture = db.Fixtures.FirstOrDefault(f => f.id == cric.FixtureId);

                if (fixture == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Fixture not found");
                }

                //if (fixture.team1_id != team.id && fixture.team2_id != team.id)
                //{
                //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The team is not part of the specified fixture");
                //}

                // Check if the fixture_id already exists for the given team_id in the CricketScore table
                var existingScore = db.CricketScores.FirstOrDefault(cs => cs.fixture_id == cric.FixtureId && cs.team_id == team.id);

                if (existingScore != null)
                {
                    // Update existing record
                    existingScore.score = cric.Score;
                    existingScore.overs = cric.Over;
                    existingScore.wickets = cric.Wickets;

                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);//, "Cricket Score updated successfully"
                }
                else
                {
                    // Insert new record
                    var newCricketScore = new CricketScore
                    {
                        team_id = team.id,
                        score = cric.Score,
                        overs = cric.Over,
                        wickets = cric.Wickets,
                        fixture_id = cric.FixtureId
                    };

                    db.CricketScores.Add(newCricketScore);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);//, "Cricket Score added with ID " + newCricketScore.id
                }
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
                    var uploadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "uploads", "CricketPics");

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
                    var relativePath = $"/uploads/CricketPics/{fileName}";
                    imagePaths.Add(relativePath);
                }

                // Check if any images were successfully uploaded
                if (imagePaths.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No valid images uploaded.");
                }

                // Return the image paths as a response
                return Request.CreateResponse(HttpStatusCode.OK,imagePaths);
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }
        //icacls "C:\Users\junai\source\repos\SportsManagementSystemBE\SportsManagementSystemBE\Resources\uploads\CricketPics" /grant Everyone:(F) /T
        //to Grant permossions to folder run it in command prompt as administrator .

        [HttpPost]
        public HttpResponseMessage PostHighScorer(List<ScoreCard> cards)
        {
            try
            {
                if(cards == null || !cards.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, " Not Found Data");
                }
                foreach (var card in cards)
                {
                    var data = new ScoreCard { 
                        fixture_id=card.fixture_id,
                        team_id=card.team_id,
                        player_id=card.player_id,
                        score=card.score,
                        ball_consumed=card.ball_consumed,
                        

                    };
                    db.ScoreCards.Add(data);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPut]
        public HttpResponseMessage UpdateCricketWinner(int fixtureId)
        {
            try
            {
                var fixture = db.Fixtures.FirstOrDefault(f => f.id == fixtureId);

                var team1Score = db.CricketScores.FirstOrDefault(s => s.fixture_id == fixtureId && s.team_id == fixture.team1_id);
                var team2Score = db.CricketScores.FirstOrDefault(s => s.fixture_id == fixtureId && s.team_id == fixture.team2_id);

                if (team1Score == null || team2Score == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Scores for one or both teams not found.");
                }

                if (team1Score.score > team2Score.score)
                {
                    fixture.winner_id = fixture.team1_id;
                }
                else if (team2Score.score > team1Score.score)
                {
                    fixture.winner_id = fixture.team2_id;
                }
                else if(team2Score.score == team1Score.score)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Score Are Level.");
                }

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Winner ID updated successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}