﻿using SportsManagementSystemBE.DTOs;
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
        public HttpResponseMessage AddCricketScore(delivery Score, string image_path)
        {
            try
            {
                if (Score == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Score cannot be null.");
                }

                // Create a new delivery object
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
                var totalRunsScored = db.deliveries
                  .Where(d => d.fixture_id == Score.fixture_id && d.team_id == Score.team_id)
                   .Sum(d => d.runs_scored);

                var totalExtraRuns = db.deliveries
                    .Where(d => d.fixture_id == Score.fixture_id && d.team_id == Score.team_id)
                    .Sum(d => d.extra_runs ?? 0);

                var totalScore = totalRunsScored + totalExtraRuns; // Combine both sums


                // Handle image if provided
                if (!string.IsNullOrEmpty(image_path))
                {
                    var imagedata = new Delivery_Images
                    {
                        image_path = image_path,
                        deliveries_id = data.id // Use the saved delivery ID
                    };

                    db.Delivery_Images.Add(imagedata);
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK,totalScore);//s, "Cricket score added successfully."
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetMatchScorers(int fixtureid)
        {
            try
            {
                // sab sa phala mai na sara table join kia phir un ko group by mai kiaa taaka woh data mai get kar sakou
                // phir into g mai store kia ar naya object bna ka key.g use kar ka get kar liaa
                //my score aur wicket pa logic b lagai ka agar null hon ga tou data ka object nai bana ga
                //key use hoti hai data get karna ka lia jo groupby ki waja sa save huaa haai.
                var BatsmanData=
                    (from d in db.deliveries
                     join f in db.Fixtures on d.fixture_id equals f.id
                     join p in db.Players on d.striker_id equals p.id
                     join s in db.Students on p.reg_no equals s.reg_no
                     join t in db.Teams on d.team_id equals t.id
                     where f.id == fixtureid

                     group d by new{ f.id,d.striker_id,d.team_id,teamname=t.name,p.reg_no,strikername=s.name } into g
                      let myScore = g.Sum(s => s.runs_scored)
                     where myScore > 0 && myScore != null
                     select new { 
                         //fixtureid=g.Key.id,
                         teamid=g.Key.team_id,
                         teamname =g.Key.teamname,
                         //strikerid=g.Key.striker_id,
                         strikername=g.Key.strikername,
                         myScore=g.Sum(s=>s.runs_scored),
                     } ).ToList();

                var BowlerData =
                    (from d in db.deliveries
                     join p in db.Players on d.bowler_id equals p.id
                     join s in db.Students on p.reg_no equals s.reg_no
                     join f in db.Fixtures on d.fixture_id equals f.id
                     //join t in db.Teams on d.team_id equals t.id// d.team_id
                     join bowlerTeam in db.Teams on p.team_id equals bowlerTeam.id
                     where f.id == fixtureid

                     group d by new { f.id,bowlerTeamid=bowlerTeam.id, bowlerTeam.name,bowlername = s.name } into g // p.reg_no,
                     let myWickets = g.Count(d => d.wicket_type == "Bold" ||
                                  d.wicket_type == "Caught" ||
                                  d.wicket_type == "Stumps" ||
                                  d.wicket_type == "Hit Wicket")
                     where myWickets > 0  // Exclude bowlers with 0 or null wickets
                     select new
                     {
                         //fixtureid=g.Key.id,
                         teamid =g.Key.bowlerTeamid,
                         teamname = g.Key.name,
                         //strikerid=g.Key.striker_id,
                          bowlername = g.Key.bowlername,
                          myWickets =myWickets
                     }).ToList();
                var result = new {
                    BatsmanData = BatsmanData,
                    BowlerData = BowlerData,
                };
                return Request.CreateResponse(HttpStatusCode.OK,result);//,result
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message);
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
        public HttpResponseMessage PostHighScorer(ManOfTheMatch userdata)
        {
            try
            {
                if (userdata == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, " Not Found Data"
                }
                var data = new ManOfTheMatch
                {
                    fixture_id = userdata.fixture_id,
                    player_id = userdata.player_id,
                    image_path = userdata.image_path,
                };

                db.ManOfTheMatches.Add(data);
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
                // Fetching data from the deliveries table (d)
                // Joining Fixtures (f) to filter by a specific fixture (match)
                // Joining Teams (t) to get team names
                // Applying a filter to get only records for the given fixtureId
                var result =
                    (from d in db.deliveries
                     join f in db.Fixtures on d.fixture_id equals f.id  // Match deliveries to their respective fixture
                     join t in db.Teams on d.team_id equals t.id  // Match deliveries to their respective team
                     where f.id == fixtureId  // Filter for the specific fixture (match)

                     // Grouping deliveries by fixture_id, team_id, and team name
                     group d by new { d.fixture_id, d.team_id, t.name } into g

                     // Creating a new object to store results
                     select new
                     {
                         fixtureid = g.FirstOrDefault().fixture_id, // Get fixture ID from the first delivery in the group
                         teamid = g.FirstOrDefault().team_id, // Get team ID from the first delivery in the group
                         teamname = g.FirstOrDefault().Team.name, // Get team name from the first delivery in the group
                         runswithextras = g.Sum(s => s.runs_scored) + g.Sum(s => s.extra_runs), // Calculate total runs including extras
                         totalwickets = g.Count(s => s.wicket_type == "Bold" || s.wicket_type == "catch_out" || s.wicket_type == "Stumps" || s.wicket_type == "runout" || s.wicket_type == "Hit Wicket")
                     }).ToList(); // Convert result to a list

                // Check if there are no records or less than two teams
                if (result.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, "No records found for the given fixture."
                }
                else if (result.Count < 2)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, "Only one team has records for the given fixture."
                }

                var team1 = result[0];
                var team2 = result[1];

                // Check if either team has null runs (indicating no data)
                if (team1.runswithextras == null || team2.runswithextras == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);//, "One or both teams have no runs data."
                }

                int? winner = 0;

                if (team1.runswithextras > team2.runswithextras)
                {
                    winner = team1.teamid;
                }
                else if (team2.runswithextras > team1.runswithextras)
                {
                    winner = team2.teamid;
                }
                else if (team2.runswithextras == team1.runswithextras)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);//, "The match ended in a tie."
                }

                var fixture = db.Fixtures.FirstOrDefault(f => f.id == fixtureId);

                if (fixture.winner_id != null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);//, "A winner has already been set for this fixture."
                }

                if (winner != null)
                {
                    fixture.winner_id = winner;
                }

                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK); //, result
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

