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
                     let myWickets = g.Count(d => d.wicket_type == "Bowled" ||
                                  d.wicket_type == "Caught" ||
                                  d.wicket_type == "Stumped" ||
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

        [HttpGet]

        public HttpResponseMessage GetfootballScore(int Fixid)
        {
            try
            {
                if (Fixid == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                var data = db.GoalBaseScores.
                     Where(g => g.fixture_id == Fixid).
                     Select(g => new { g.team_id, g.goals }).
                     ToList();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }
        }



        [HttpGet]
        public HttpResponseMessage MatchScores(int fixtureid)
        {
            try
            {
                // Fetch the fixture details by matchId
                var fixture = db.Fixtures.FirstOrDefault(m => m.id == fixtureid);

                if (fixture != null)
                {
                    var team1 = db.Teams.FirstOrDefault(t => t.id == fixture.team1_id);
                    var team2 = db.Teams.FirstOrDefault(t => t.id == fixture.team2_id);
                    //var comments = db.Comments.FirstOrDefault(c => c.fixture_id == fixtureid);

                    // Initialize a response object to hold the match details
                    var matchDetails = new
                    {
                        Fixture = new
                        {
                            fixture.id,
                            fixture.team1_id,
                            fixture.team2_id,
                            fixture.matchDate,
                            fixture.venue,
                            winner = fixture.winner_id,
                            // Add team names to the response object
                            Team1Name = team1 != null ? team1.name : "Team 1 not found",
                            Team2Name = team2 != null ? team2.name : "Team 2 not found",
                            //Comments = comments != null ? comments.comments : "No commentary"
                        },
                        ScoreDetails = new List<object>()
                    };

                    // Check if there's goal-based scoring data for this fixture (e.g., football, soccer)
                    var goalScore = db.GoalBaseScores
                                      .Where(g => g.fixture_id == fixtureid)
                                      .Select(g => new
                                      {
                                          TeamId = g.team_id,
                                          g.goals
                                      }).ToList();

                    if (goalScore.Any())
                    {
                        matchDetails.ScoreDetails.Add(new
                        {
                            Type = "Goal-Based Scoring",
                            Score = goalScore
                        });
                    }

                    //// Check if there's cricket scoring data for this fixture (e.g., cricket)
                    //var cricketScore = db.CricketScores
                    //                     .Where(c => c.fixture_id == fixtureid)
                    //                     .Select(c => new
                    //                     {
                    //                         TeamId = c.team_id,
                    //                         c.score,
                    //                         c.overs,
                    //                         c.wickets
                    //                     }).ToList();

                    //if (cricketScore.Any())
                    //{
                    //    matchDetails.ScoreDetails.Add(new
                    //    {
                    //        Type = "Cricket Scoring",
                    //        Score = cricketScore
                    //    });
                    //}

                    // Check if there's point-based scoring data for this fixture (e.g., tennis, volleyball)
                    var pointScore = db.PointsBaseScores
                                       .Where(p => p.fixture_id == fixtureid)
                                       .Select(p => new
                                       {
                                           TeamId = p.team_id,
                                           p.setsWon
                                       }).ToList();

                    if (pointScore.Any())
                    {
                        matchDetails.ScoreDetails.Add(new
                        {
                            Type = "Point-Based Scoring",
                            Score = pointScore
                        });
                    }
                    var TurnScore = db.TurnBaseGames
                                       .Where(p => p.fixture_id == fixtureid)
                                       .Select(p => new
                                       {
                                           //WinnerId = p.winner_id,
                                           LoserId = p.loser_id,
                                       }).ToList();

                    if (TurnScore.Any())
                    {
                        matchDetails.ScoreDetails.Add(new
                        {
                            Type = "Turn-Based Scoring",
                            Score = pointScore
                        });
                    }

                    // Return the match details along with the score details
                    if (matchDetails.ScoreDetails.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, matchDetails);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "No scores available for this match.");
                    }
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


        //PointBasedScoring
        [HttpPost]
        public HttpResponseMessage AddOrUpdatePointBasedScore(PointsBaseScore points)
        {
            try
            {
                //var team = db.Teams.FirstOrDefault(t => t.name == teamName);
                //var fixture = db.Fixtures.FirstOrDefault(f => f.id == fixture_id);

                //if (points==null)
                //{
                //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No data found");
                //}

                var existingScore = db.PointsBaseScores.FirstOrDefault(ps => ps.fixture_id == points.fixture_id && ps.team_id == points.team_id);

                if (existingScore != null)
                {
                    // Update existing record
                    existingScore.setsWon = points.setsWon;

                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);//, "Score updated successfully"
                }
                else
                {
                    // Insert new record
                    var newPointBaseScore = new PointsBaseScore
                    {
                        team_id = points.team_id,
                        setsWon = points.setsWon,
                        fixture_id = points.fixture_id,
                    };

                    db.PointsBaseScores.Add(newPointBaseScore);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);//, "Score added with ID " + newPointBaseScore.id
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        public HttpResponseMessage UpdatePointBasedWinner(int fixtureId)
        {
            try
            {
                var fixture = db.Fixtures.FirstOrDefault(f => f.id == fixtureId);

                var team1Sets = db.PointsBaseScores.FirstOrDefault(s => s.fixture_id == fixtureId && s.team_id == fixture.team1_id);
                var team2Sets = db.PointsBaseScores.FirstOrDefault(s => s.fixture_id == fixtureId && s.team_id == fixture.team2_id);

                if (team1Sets == null || team2Sets == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Scores for one or both teams not found.");
                }

                if (team1Sets.setsWon > team2Sets.setsWon)
                {
                    fixture.winner_id = fixture.team1_id;
                }
                else if (team1Sets.setsWon < team2Sets.setsWon)
                {
                    fixture.winner_id = fixture.team2_id;
                }
                else if (team1Sets.setsWon == team2Sets.setsWon)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);
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

