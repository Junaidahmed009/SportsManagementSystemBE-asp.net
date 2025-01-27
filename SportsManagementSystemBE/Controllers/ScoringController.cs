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
        //[HttpPost]
        //public HttpResponseMessage AddOrUpdateCricketScore(CricketScoringDTO cric)
        //{
        //    try
        //    {
        //        // Fetch team based on teamName
        //        var team = db.Teams.FirstOrDefault(t => t.id == cric.Teamid);

        //        //if (team == null)
        //        //{
        //        //    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Team not found");
        //        //}

        //        // Check if the fixture_id exists and if the team_id is part of the fixture
        //        var fixture = db.Fixtures.FirstOrDefault(f => f.id == cric.FixtureId);

        //        if (fixture == null)
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Fixture not found");
        //        }

        //        //if (fixture.team1_id != team.id && fixture.team2_id != team.id)
        //        //{
        //        //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The team is not part of the specified fixture");
        //        //}

        //        // Check if the fixture_id already exists for the given team_id in the CricketScore table
        //        var existingScore = db.CricketScores.FirstOrDefault(cs => cs.fixture_id == cric.FixtureId && cs.team_id == team.id);

        //        if (existingScore != null)
        //        {
        //            // Update existing record
        //            existingScore.score = cric.Score;
        //            existingScore.overs = cric.Over;
        //            existingScore.wickets = cric.Wickets;

        //            db.SaveChanges();
        //            return Request.CreateResponse(HttpStatusCode.OK);//, "Cricket Score updated successfully"
        //        }
        //        else
        //        {
        //            // Insert new record
        //            var newCricketScore = new CricketScore
        //            {
        //                team_id = team.id,
        //                score = cric.Score,
        //                overs = cric.Over,
        //                wickets = cric.Wickets,
        //                fixture_id = cric.FixtureId
        //            };

        //            db.CricketScores.Add(newCricketScore);
        //            db.SaveChanges();

        //            return Request.CreateResponse(HttpStatusCode.OK);//, "Cricket Score added with ID " + newCricketScore.id
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        [HttpPost]
        public HttpResponseMessage AddCricketScore(delivery Score)
        {
            try
            {
                if (Score == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                var data = new delivery
                {
                    fixture_id = Score.fixture_id,
                    team_id = Score.team_id,
                    over_number = Score.over_number,
                    ball_number = Score.ball_number,
                    runs_scored = Score.runs_scored,
                    striker_id = Score.striker_id,
                    non_striker_id = Score.non_striker_id,
                    bowler_id = Score.bowler_id,
                    extras = Score.extras,
                    extra_runs = Score.extra_runs,
                    wicket_type = Score.wicket_type,
                    dismissed_player_id = Score.dismissed_player_id,
                    fielder_id = Score.fielder_id,
                };
                
                db.deliveries.Add(data);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,data.id);

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
                //if(cards == null || !cards.Any())
                //{
                //    return Request.CreateResponse(HttpStatusCode.NotFound);//, " Not Found Data"
                //}
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
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, "Scores for one or both teams not found."
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
                    return Request.CreateResponse(HttpStatusCode.Conflict);//, "Score Are Level."
                }

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Winner ID updated successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage MatchScores(int matchId)
        {
            try
            {
                // Fetch the fixture details by matchId
                var fixture = db.Fixtures.FirstOrDefault(m => m.id == matchId);

                if (fixture != null)
                {
                    var team1 = db.Teams.FirstOrDefault(t => t.id == fixture.team1_id);
                    var team2 = db.Teams.FirstOrDefault(t => t.id == fixture.team2_id);

                    // Initialize a list to hold all match details and scores
                    var matchDetails = new List<object>();

                    // Add fixture details as the first item
                    matchDetails.Add(new
                    {
                        FixtureId = fixture.id,
                        Team1Id = fixture.team1_id,
                        Team2Id = fixture.team2_id,
                        MatchDate = fixture.matchDate,
                        Venue = fixture.venue,
                        Team1Name = team1 != null ? team1.name : "Team 1 not found",
                        Team2Name = team2 != null ? team2.name : "Team 2 not found",
                    });

                    // Goal-Based Scoring
                    var goalScore = db.GoalBaseScores
                                      .Where(g => g.fixture_id == matchId)
                                      .Select(g => new
                                      {
                                          TeamId = g.team_id,
                                          g.goals
                                      }).ToList();

                    if (goalScore.Any())
                    {
                        matchDetails.Add(new
                        {
                            Type = "Goal-Based Scoring",
                            Score = goalScore
                        });
                    }

                    // Cricket Scoring
                    var cricketScore = db.CricketScores
                                         .Where(c => c.fixture_id == matchId)
                                         .Select(c => new
                                         {
                                             TeamId = c.team_id,
                                             c.score,
                                             c.overs,
                                             c.wickets
                                         }).ToList();

                    if (cricketScore.Any())
                    {
                        matchDetails.Add(new
                        {
                            Type = "Cricket Scoring",
                            Score = cricketScore
                        });
                    }

                    // Point-Based Scoring
                    var pointScore = db.PointsBaseScores
                                       .Where(p => p.fixture_id == matchId)
                                       .Select(p => new
                                       {
                                           TeamId = p.team_id,
                                           p.setsWon
                                       }).ToList();

                    if (pointScore.Any())
                    {
                        matchDetails.Add(new
                        {
                            Type = "Point-Based Scoring",
                            Score = pointScore
                        });
                    }

                    // Turn-Based Scoring
                    var turnScore = db.TurnBaseGames
                                      .Where(t => t.fixture_id == matchId)
                                      .Select(t => new
                                      {
                                          WinnerId = t.winnner_id,
                                          LoserId = t.loser_id,
                                      }).ToList();

                    if (turnScore.Any())
                    {
                        matchDetails.Add(new
                        {
                            Type = "Turn-Based Scoring",
                            Score = turnScore
                        });
                    }

                    // Return the match details and scores
                    return Request.CreateResponse(HttpStatusCode.OK, matchDetails);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Fixture not found.");
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage AddOrUpdateGoalBasedScore(GoalBaseDTO gbd)
        {
            try
            {
                var team = db.Teams.FirstOrDefault(t => t.id == gbd.Teamid);
                var fixture = db.Fixtures.FirstOrDefault(f => f.id == gbd.Fixture_id);

                if (fixture.team1_id != team.id && fixture.team2_id != team.id)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);//, "The team is not part of the specified fixture"
                }

                var existingScore = db.GoalBaseScores.FirstOrDefault(cs => cs.fixture_id == gbd.Fixture_id && cs.team_id == team.id);

                if (existingScore != null)
                {
                    // Update existing record
                    existingScore.goals = gbd.Goals;

                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);//, "Score updated successfully"
                }
                else
                {
                    // Insert new record
                    var newGoalBaseScore = new GoalBaseScore
                    {
                        team_id = team.id,
                        goals = gbd.Goals,
                        fixture_id = gbd.Fixture_id
                    };

                    db.GoalBaseScores.Add(newGoalBaseScore);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);//,"Score Updated" + newGoalBaseScore.id
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        public HttpResponseMessage UpdateGoalBasedWinner(int fixtureId)
        {
            try
            {
                var fixture = db.Fixtures.FirstOrDefault(f => f.id == fixtureId);

                var team1Goals = db.GoalBaseScores.FirstOrDefault(s => s.fixture_id == fixtureId && s.team_id == fixture.team1_id);
                var team2Goals = db.GoalBaseScores.FirstOrDefault(s => s.fixture_id == fixtureId && s.team_id == fixture.team2_id);

                if (team1Goals == null || team2Goals == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, "Scores for one or both teams not found."
                }

                if (team1Goals.goals > team2Goals.goals)
                {
                    fixture.winner_id = fixture.team1_id;
                }
                else if (team2Goals.goals > team1Goals.goals)
                {
                    fixture.winner_id = fixture.team2_id;
                }
                else if (team2Goals.goals == team1Goals.goals)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);
                }

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);//, "Winner ID updated successfully."
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message);
            }
        }


    }
}

