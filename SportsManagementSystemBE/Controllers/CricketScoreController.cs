using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Management;

namespace SportsManagementSystemBE.Controllers
{
    public class CricketScoreController : ApiController
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
                var dismissalTypes = new List<string>
                                        {
                                            "Bowled",
                                            "Caught",
                                            "runout",
                                            "Stumped",
                                            "Hit Wicket"
                                        };
                // Check existing dismissals for striker and non-striker in this fixture
                bool isStrikerDismissed = db.deliveries
                    .Any(d =>
                        d.fixture_id == Score.fixture_id &&
                        d.dismissed_player_id == Score.striker_id &&
                        dismissalTypes.Contains(d.wicket_type)
                    );
                bool isNonStrikerDismissed = db.deliveries
                    .Any(d =>
                        d.fixture_id == Score.fixture_id &&
                        d.dismissed_player_id == Score.non_striker_id &&
                        dismissalTypes.Contains(d.wicket_type)
                    );
                if (isStrikerDismissed || isNonStrikerDismissed)
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict);//$\"Striker/Non-striker already dismissed in fixture, {Score.fixture_id}
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
                return Request.CreateResponse(HttpStatusCode.OK, totalScore);//s, "Cricket score added successfully."
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
                         runswithextras = g.Sum(s => s.runs_scored) + (g.Sum(s => s.extra_runs) ?? 0), // Calculate total runs including extras
                         totalwickets = g.Count(s => s.wicket_type == "Bowled" || s.wicket_type == "Caught" || s.wicket_type == "Stumped" || s.wicket_type == "runout" || s.wicket_type == "Hit Wicket")
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
                return Request.CreateResponse(HttpStatusCode.OK,result); //, result
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage PostManOfMatch(ManOfTheMatch userdata)
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

        [HttpGet]
        public HttpResponseMessage GetCricketScores(int fixtureid)
        {
            try
            {
                // sab sa phala mai na sara table join kia phir un ko group by mai kiaa taaka woh data mai get kar sakou
                // phir into g mai store kia ar naya object bna ka key.g use kar ka get kar liaa
                //my score aur wicket pa logic b lagai ka agar null hon ga tou data ka object nai bana ga
                //key use hoti hai data get karna ka lia jo groupby ki waja sa save huaa haai.
                var BatsmanData =
                    (from d in db.deliveries
                     join f in db.Fixtures on d.fixture_id equals f.id
                     join p in db.Players on d.striker_id equals p.id
                     join s in db.Students on p.reg_no equals s.reg_no
                     join t in db.Teams on d.team_id equals t.id
                     where f.id == fixtureid

                     group d by new { f.id, d.striker_id, d.team_id, teamname = t.name, p.reg_no, strikername = s.name } into g
                     let myScore = g.Sum(s => s.runs_scored)
                     where myScore > 0 && myScore != null
                     select new
                     {
                         //fixtureid=g.Key.id,
                         teamid = g.Key.team_id,
                         teamname = g.Key.teamname,
                         //strikerid=g.Key.striker_id,
                         strikername = g.Key.strikername,
                         myScore = g.Sum(s => s.runs_scored),
                     }).ToList();

                var BowlerData =
                    (from d in db.deliveries
                     join p in db.Players on d.bowler_id equals p.id
                     join s in db.Students on p.reg_no equals s.reg_no
                     join f in db.Fixtures on d.fixture_id equals f.id
                     //join t in db.Teams on d.team_id equals t.id// d.team_id
                     join bowlerTeam in db.Teams on p.team_id equals bowlerTeam.id
                     where f.id == fixtureid

                     group d by new { f.id, bowlerTeamid = bowlerTeam.id, bowlerTeam.name, bowlername = s.name } into g // p.reg_no,
                     let myWickets = g.Count(d => d.wicket_type == "Bowled" ||
                                  d.wicket_type == "Caught" ||
                                  d.wicket_type == "Stumped" ||
                                  d.wicket_type == "Hit Wicket")
                     where myWickets > 0  // Exclude bowlers with 0 or null wickets
                     select new
                     {
                         //fixtureid=g.Key.id,
                         teamid = g.Key.bowlerTeamid,
                         teamname = g.Key.name,
                         //strikerid=g.Key.striker_id,
                         bowlername = g.Key.bowlername,
                         myWickets = myWickets
                     }).ToList();
                var result = new
                {
                    BatsmanData = BatsmanData,
                    BowlerData = BowlerData,
                };
                return Request.CreateResponse(HttpStatusCode.OK, result);//,result
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred: " + ex.Message);
            }

        }

        [HttpGet]
        public HttpResponseMessage GetMatchScore(int Fixid)
        {
            try
            {
                var individualScore = (from d in db.deliveries
                                       join f in db.Fixtures on d.fixture_id equals f.id
                                       join p in db.Players on d.striker_id equals p.id
                                       join t in db.Teams on d.team_id equals t.id
                                       join s in db.Students on p.reg_no equals s.reg_no
                                       where f.id == Fixid
                                       group d by new { f.id, d.striker_id, d.team_id, t.name, p.reg_no,studentname=s.name } into grouped
                                       select new
                                       {
                                           id = grouped.Key.id,
                                           teamid = grouped.Key.team_id,
                                           teamName = grouped.Key.name,
                                           striker_id = grouped.Key.striker_id,
                                           Batsman = grouped.Key.studentname,
                                           runs = grouped.Sum(d => d.runs_scored)
                                       }).ToList();
                var fixture = db.Fixtures.FirstOrDefault(d => d.id ==Fixid);
                var team1 = individualScore.Where(score => score.teamid == fixture.team1_id).ToList();
                var team2 = individualScore.Where(score => score.teamid == fixture.team2_id).ToList();

                var EachTeamIndividualScore = new
                {
                    team1Score = team1,
                    team2Score = team2
                };

                var TeamRunswithExtra = (from d in db.deliveries
                                         join f in db.Fixtures on d.fixture_id equals f.id
                                         join t in db.Teams on d.team_id equals t.id
                                         where f.id == Fixid
                                         group d by new { f.id, d.team_id, t.name } into grouped
                                         select new
                                         {
                                             id = grouped.Key.id,
                                             teamid = grouped.Key.team_id,
                                             teamName = grouped.Key.name,
                                             run = grouped.Sum(s => s.runs_scored) + (grouped.Sum(s => s.extra_runs) ?? 0)
                                         }).ToList();

                var team1Runs = TeamRunswithExtra.Where(score => score.teamid == fixture.team1_id).ToList();
                var team2Runs = TeamRunswithExtra.Where(score => score.teamid == fixture.team2_id).ToList();

                var EachTeamScore = new
                {
                    team1Total = team1Runs,
                    team2Total = team2Runs
                };

                var bowlingStatsResponse = bowlingStatsPerMatch(Fixid);
                var bowlingStatsResult = bowlingStatsResponse.Content.ReadAsAsync<object>().Result;

                var results = new
                {
                    PlayersScore = EachTeamIndividualScore,
                    RunwithExtra = EachTeamScore,
                    bowlingStats = bowlingStatsResult,
                };

                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage bowlingStatsPerMatch(int fixtureId)
        {
            try
            {
                // Step 1: Calculate bowling stats (database-friendly operations only)
                var bowStat = (
                    from d in db.deliveries
                    where d.fixture_id == fixtureId
                    join p in db.Players on d.bowler_id equals p.id // Get bowler's team
                    group d by new { d.fixture_id, p.team_id, d.bowler_id } into grouped
                    let validDeliveries = grouped.Count(d =>
                        d.extras != "Wide" && d.extras != "No Ball")
                    let extraDeliveries = grouped.Count(d =>
                        d.extras == "Wide" || d.extras == "No Ball")
                    select new
                    {
                        fixture_id = grouped.Key.fixture_id,
                        team_id = grouped.Key.team_id,
                        bowler_id = grouped.Key.bowler_id,
                        runsConceeded = grouped.Sum(s => s.runs_scored) + (grouped.Sum(s => s.extra_runs) ?? 0),
                        validDeliveries = validDeliveries, // Store for later formatting
                        extradelivery=extraDeliveries,
                        wickets_taken = grouped.Count(d =>
                            d.wicket_type == "Bowled" ||
                            d.wicket_type == "Caught" ||
                            d.wicket_type == "Stumped" ||
                            d.wicket_type == "Hit Wicket")
                    }).ToList(); // Materialize here to switch to LINQ-to-Objects

                // Step 2: Format overs as a string in memory (after ToList)
                var formattedBowStat = bowStat.Select(bs =>
                {
                    int completedOvers = bs.validDeliveries / 6;
                    int balls = bs.validDeliveries % 6;
                    int extras = bs.extradelivery;
                    string overs = balls == 0
                        ? $"{completedOvers}"
                        : $"{completedOvers}.{balls}";

                    return new
                    {
                        bs.fixture_id,
                        bs.team_id,
                        bs.bowler_id,
                        bs.runsConceeded,
                        overs,
                        bs.wickets_taken,
                        bs.extradelivery,
                        
                    };
                }).ToList();

                // Step 2: Create temporary data for #bowInfo
                var bowInfo = (from t in db.Teams
                               join p in db.Players on t.id equals p.team_id
                               join s in db.Students on p.reg_no equals s.reg_no
                               select new
                               {
                                   teamid = t.id,
                                   Tname = t.name,
                                   reg_no = s.reg_no,
                                   player_id = p.id,
                                   player_name = s.name
                               }).ToList();

                // Step 3: Join #bowStat and #bowInfo
                var result = (from bi in bowInfo
                              join bs in formattedBowStat on bi.player_id equals bs.bowler_id
                              select new
                              {
                                  team_id = bi.teamid,
                                  team_name = bi.Tname,
                                  player_id = bi.player_id,
                                  player_name = bi.player_name,
                                  runs_conceeded = bs.runsConceeded,
                                  overs = bs.overs,
                                  wickets_taken = bs.wickets_taken,
                                  Extras=bs.extradelivery,
                              }).ToList();
                var fixture = db.Fixtures.FirstOrDefault(d => d.id == fixtureId);
                var team1 = result.Where(score => score.team_id == fixture.team1_id).ToList();
                var team2 = result.Where(score => score.team_id == fixture.team2_id).ToList();
                var EachTeamIndividualStats = new
                {
                    team1Stats = team1,
                    team2Stats = team2
                };
                if (result.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No bowling data found for the given fixture");
                }

                return Request.CreateResponse(HttpStatusCode.OK, EachTeamIndividualStats);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage ballByballData(int specificFixtureId)
        {
            try
            {
                var fixtureData = db.Fixtures
                    .Where(f => f.id == specificFixtureId)
                    .Select(f => new
                    {
                        Team1Id = f.team1_id,
                        Team2Id = f.team2_id
                    }).FirstOrDefault();

                if (fixtureData == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No fixtures found");
                }

                var allDeliveries = db.deliveries
                    .Where(d => d.fixture_id == specificFixtureId)
                    .OrderBy(d => d.over_number)
                    .ThenBy(d => d.ball_number)
                    .AsEnumerable()
                    .Select(d => new
                    {
                        Delivery = d,
                        Striker = db.Players.Where(p => p.id == d.striker_id)
                                    .Join(db.Students, p => p.reg_no, s => s.reg_no, (p, s) => s.name)
                                    .FirstOrDefault(),
                        NonStriker = db.Players.Where(p => p.id == d.non_striker_id)
                                    .Join(db.Students, p => p.reg_no, s => s.reg_no, (p, s) => s.name)
                                    .FirstOrDefault(),
                        Bowler = db.Players.Where(p => p.id == d.bowler_id)
                                    .Join(db.Students, p => p.reg_no, s => s.reg_no, (p, s) => s.name)
                                    .FirstOrDefault(),
                        DismissedPlayer = db.Players.Where(p => p.id == d.dismissed_player_id)
                                    .Join(db.Students, p => p.reg_no, s => s.reg_no, (p, s) => s.name)
                                    .FirstOrDefault(),
                        Fielder = db.Players.Where(p => p.id == d.fielder_id)
                                    .Join(db.Students, p => p.reg_no, s => s.reg_no, (p, s) => s.name)
                                    .FirstOrDefault()
                    })
                    .Select(x => new BallByBallDto
                    {
                        Over = x.Delivery.over_number ??0,
                        Ball = x.Delivery.ball_number ??0,
                        Striker = x.Striker ?? "Unknown",
                        NonStriker = x.NonStriker ?? "Unknown",
                        Bowler = x.Bowler ?? "Unknown",
                        BatsmanRuns = x.Delivery.runs_scored ??0,
                        ExtraRuns = x.Delivery.extra_runs ??0,
                        ExtraType = x.Delivery.extras ?? "None",
                        IsWicket = !string.IsNullOrEmpty(x.Delivery.wicket_type),
                        WicketType = x.Delivery.wicket_type ?? "None",
                        DismissedPlayer = x.DismissedPlayer ?? "None",
                        Fielder = x.Fielder ?? "None",
                        TeamId = x.Delivery.team_id ??0
                    }).ToList();
                var team1data = allDeliveries
                    .Where(d => d.TeamId == fixtureData.Team1Id)
                    .ToList();

                var team2data = allDeliveries
                    .Where(d => d.TeamId == fixtureData.Team2Id)
                    .ToList();
                var team1name = db.Teams
                   .Where(t => t.id == fixtureData.Team1Id).
                   Select(t => t.name)
                   .FirstOrDefault();

                var team2name = db.Teams
                  .Where(t => t.id == fixtureData.Team2Id).
                  Select(t => t.name)
                  .FirstOrDefault();

                var result = new
                {
                    team1 = team1data,
                    team2 = team2data,
                    team1name=team1name,
                    team2name=team2name
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetImagePath(int fixid)
        {
            try
            {
                if (fixid < 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                var MOMimagepath = db.ManOfTheMatches.
                    Where(m => m.fixture_id == fixid)
                    .Select(m => m.image_path)
                    .FirstOrDefault();

                var Deliveryimages =
                   (from d in db.deliveries
                    join dm in db.Delivery_Images on d.id equals dm.deliveries_id
                    where d.fixture_id == fixid
                    group d by new { dm.image_path, dm.deliveries_id, d.runs_scored, d.wicket_type } into grouped
                    select new
                    {
                        imagepath = grouped.Key.image_path,
                        deliveriesid = grouped.Key.deliveries_id,
                        socre = grouped.Key.runs_scored.ToString(),
                        wicket = grouped.Key.wicket_type
                    }).ToList();

                var playerDetails =
                    (from p in db.Players
                     join mm in db.ManOfTheMatches on p.id equals mm.player_id
                     join s in db.Students on p.reg_no equals s.reg_no
                     join d in db.deliveries on mm.fixture_id equals d.fixture_id 
                     where mm.fixture_id == fixid
                     group new {p, d} by new { p.reg_no, studentregno = s.reg_no, s.name, s.section, s.semNo, s.discipline} into grouped
                     select new
                     {
                         studentreg = grouped.Key.studentregno,
                         name = grouped.Key.name,
                         section = grouped.Key.section,
                         semno = grouped.Key.semNo,
                         discipline = grouped.Key.discipline,
                         runsscored = grouped.Where(g=>g.d.striker_id==g.p.id).Sum(g => g.d.runs_scored),
                         wickets_taken = grouped.Count(g =>g.d.bowler_id==g.p.id &&
                          ( g.d.wicket_type == "Bowled" ||
                           g.d.wicket_type == "Caught" ||
                           g.d.wicket_type == "Stumped" ||
                           g.d.wicket_type == "Hit Wicket"))
                     }).ToList();
                if (playerDetails == null || Deliveryimages == null || MOMimagepath ==null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                var data = new { MOMimagepath,playerDetails,Deliveryimages };

                return Request.CreateResponse(HttpStatusCode.OK,data);
            }
            catch (Exception ex) {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage topWicketTaker(int sessionId)
        {
            try
            {
                var sessionSportIds = db.SessionSports
                    .Where(ss => ss.session_id == sessionId)
                    .Select(ss => ss.id)
                    .ToList();

                if (!sessionSportIds.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No fixtures found");
                }

                var validDeliveries = db.deliveries
                    .Where(d => d.fixture_id.HasValue) // Ensure fixture_id is not null
                    .Join(db.Fixtures,
                        d => d.fixture_id.Value,       // Now safe to use .Value
                        f => f.id,
                        (d, f) => new { Delivery = d, Fixture = f })
                    .Where(x =>
                        x.Fixture.sessionSports_id.HasValue &&
                        sessionSportIds.Contains(x.Fixture.sessionSports_id.Value) // Fix here
                    )
                    .Where(x =>
                        x.Delivery.extras != "Wide" &&
                        x.Delivery.extras != "No ball"
                    )
                    .ToList();

                var result = validDeliveries
                    .GroupBy(x => new { x.Delivery.bowler_id })
                    .Select(g =>
                    {
                        var bowler = db.Players
                            .Where(p => p.id == g.Key.bowler_id)
                            .Select(p => p.Student.name)
                            .FirstOrDefault();

                        int validBalls = g.Count();
                        int overs = validBalls / 6;
                        int balls = validBalls % 6;
                        int runsConceded = (int)g.Sum(x => x.Delivery.runs_scored);// + x.Delivery.extra_runs
                        int wickets = g.Count(x =>
                            x.Delivery.wicket_type == "Bowled" ||
                            x.Delivery.wicket_type == "Caught" ||
                            x.Delivery.wicket_type == "Stumped" ||
                            //x.Delivery.wicket_type == "lbw" ||
                            x.Delivery.wicket_type == "hit wicket" ||
                            x.Delivery.wicket_type == "Caught and Bowled"
                        );

                        decimal economyRate = (validBalls == 0)
                            ? 0
                            : Math.Round(runsConceded / (decimal)(validBalls / 6.0m), 2);

                        return new
                        {
                            bowlerId = g.Key.bowler_id,
                            bowlerName = bowler ?? "Unknown",
                            wickets,
                            oversFormatted = $"{overs}.{balls}",
                            runsConceded,
                            economyRate
                        };
                    })
                    .OrderByDescending(b => b.wickets)
                    .ThenBy(b => b.economyRate)
                    .Take(3)
                    .ToList();
                if (!result.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No wickets taken in this session");
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage topScorer(int sessionId)
        {
            try
            {
                // Fetch sessionSportIds for the given sessionId
                var sessionSportIds = db.SessionSports
                                        .Where(ss => ss.session_id == sessionId)
                                        .Select(ss => ss.id)
                                        .ToList();

                if (!sessionSportIds.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No fixtures found for this session");
                }

                // Fetch valid deliveries (excluding wides, no-balls, and null runs_scored)
                var validDeliveries = db.deliveries
                                    .Join(db.Fixtures,
                                          d => d.fixture_id,
                                          f => f.id,
                                          (d, f) => new { Delivery = d, Fixture = f })
                                    .Where(x => x.Fixture.sessionSports_id.HasValue && sessionSportIds.Contains(x.Fixture.sessionSports_id.Value))
                                    .Where(x => !(x.Delivery.extras == "Wide" || x.Delivery.extras == "No Ball")) // Exclude wides & no-balls
                                    .Where(x => x.Delivery.striker_id != null) // Ensure striker_id is not null
                                    .Where(x => x.Delivery.runs_scored.HasValue) // Exclude deliveries where runs_scored is NULL
                                    .Join(db.Players, // Join with Players table to get player details
                                          d => d.Delivery.striker_id,
                                          p => p.id,
                                          (d, p) => new { Delivery = d.Delivery, Fixture = d.Fixture, Player = p })
                                    .Join(db.Students, // Join with Students table to get student name
                                          dp => dp.Player.reg_no,
                                          s => s.reg_no,
                                          (dp, s) => new { Delivery = dp.Delivery, Fixture = dp.Fixture, Player = dp.Player, Student = s })
                                    .ToList();

                // Group by striker_id and calculate total runs, balls faced, strike rate, and overs played
                var result = validDeliveries
                     .GroupBy(x => x.Delivery.striker_id) // Group by striker_id
                     .Select(g => new
                     {
                         batsmanId = g.Key,
                         batsmanName = g.First().Student.name ?? "Unknown", // Fetch batsman name from the joined Student table
                         totalRuns = g.Sum(x => x.Delivery.runs_scored ?? 0), // Sum only runs_scored
                         ballsFaced = g.Count(),
                         strikeRate = g.Count() == 0
                             ? 0
                             : Math.Round((double)g.Sum(x => x.Delivery.runs_scored ?? 0) / g.Count() * 100, 2), // Calculate strike rate
                         oversPlayed = $"{g.Count() / 6}.{g.Count() % 6}" // Format overs played
                     })
                     .OrderByDescending(p => p.totalRuns) // Order by total runs descending
                     .ThenByDescending(p => p.strikeRate) // Then by strike rate descending
                     .Take(3) // Take top 3
                     .ToList();

                if (!result.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No batting records found");
                }

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        //return best player of tournament in session based search accoring to new way
        [HttpGet]
        public HttpResponseMessage BestPlayer(int sessionId)
        {
            try
            {
                // 1. Get sessionSport IDs
                var sessionSportIds = db.SessionSports
                    .Where(ss => ss.session_id == sessionId)
                    .Select(ss => ss.id)
                    .ToList();

                if (!sessionSportIds.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Session not found");
                }

                // 2. Get all fixture IDs in the session (primitive int)
                var fixtureIds = db.Fixtures
                    .Where(f => f.sessionSports_id.HasValue &&
                          sessionSportIds.Contains(f.sessionSports_id.Value))
                    .Select(f => f.id)
                    .ToList();

                if (!fixtureIds.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No fixtures in this session");
                }

                // 3. Get batting stats (using primitive fixture IDs)
                var battingStats = db.deliveries
                    .Where(d => d.fixture_id.HasValue &&
                          fixtureIds.Contains(d.fixture_id.Value) &&
                          d.extras != "Wide" &&
                          d.extras != "No ball")
                    .GroupBy(d => d.striker_id)
                    .Select(g => new
                    {
                        PlayerId = g.Key,
                        TotalRuns = g.Sum(d => d.runs_scored ?? 0), // Handle null runs_scored
                        BallsFaced = g.Count(),
                        Innings = g.Select(d => d.fixture_id).Distinct().Count()
                    })
                    .ToList();

                // 4. Get bowling stats (using primitive fixture IDs)
                var bowlingStats = db.deliveries
                    .Where(d => d.fixture_id.HasValue &&
                          fixtureIds.Contains(d.fixture_id.Value) &&
                          d.extras != "Wide" &&
                          d.extras != "No-ball")
                    .GroupBy(d => d.bowler_id)
                    .Select(g => new
                    {
                        PlayerId = g.Key,
                        Wickets = g.Count(d =>
                            d.wicket_type == "Bowled" ||
                            d.wicket_type == "Caught" ||
                            d.wicket_type == "Stumped" ||
                            d.wicket_type == "hit wicket" ||
                            d.wicket_type == "Caught and Bowled"),
                        RunsConceded = g.Sum(d => (d.runs_scored ?? 0) + (d.extra_runs ?? 0)), // Handle null runs_scored and extra_runs
                        BallsBowled = g.Count(),
                        Innings = g.Select(d => d.fixture_id).Distinct().Count()
                    })
                    .ToList();

                // 5. Combine stats and calculate scores
                var allPlayerIds = battingStats.Select(b => b.PlayerId)
                                              .Union(bowlingStats.Select(b => b.PlayerId))
                                              .Distinct()
                                              .ToList();

                var players = db.Players
                    .Where(p => allPlayerIds.Contains(p.id))
                    .Select(p => new { p.id, p.Student.name })
                    .ToList();

                var result = allPlayerIds
                    .Select(playerId =>
                    {
                        var bat = battingStats.FirstOrDefault(b => b.PlayerId == playerId);
                        var bowl = bowlingStats.FirstOrDefault(b => b.PlayerId == playerId);
                        var player = players.FirstOrDefault(p => p.id == playerId);

                        return new
                        {
                            PlayerId = playerId,
                            PlayerName = player?.name ?? "Unknown",
                            BattingRuns = bat?.TotalRuns ?? 0,
                            BattingStrikeRate = bat != null && bat.BallsFaced > 0 ?
                                Math.Round((double)bat.TotalRuns / bat.BallsFaced * 100, 2) : 0,
                            WicketsTaken = bowl?.Wickets ?? 0,
                            BowlingEconomy = bowl != null && bowl.BallsBowled > 0 ?
                                Math.Round((double)bowl.RunsConceded / (bowl.BallsBowled / 6.0), 2) : 0,
                            MatchesPlayed = (bat?.Innings ?? 0) + (bowl?.Innings ?? 0)
                        };
                    })
                    .OrderByDescending(p => (p.BattingRuns * 2) + (p.WicketsTaken * 25)) // Simple scoring logic
                    .Take(3)
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        ////fetch bowlers and overs with wickets of a match accoring to new way
        //[HttpGet]
        //public HttpResponseMessage bowlingStatsPerMatch(int fixtureId)
        //{
        //    try
        //    {
        //        // Step 1: Calculate bowling stats (database-friendly operations only)
        //        var bowStat = (
        //            from d in db.deliveries
        //            where d.fixture_id == fixtureId
        //            join p in db.Players on d.bowler_id equals p.id // Get bowler's team
        //            group d by new { d.fixture_id, p.team_id, d.bowler_id } into grouped
        //            let validDeliveries = grouped.Count(d =>
        //                d.extras != "Wide" && d.extras != "No-ball") // Case-sensitive check
        //            select new
        //            {
        //                fixture_id = grouped.Key.fixture_id,
        //                team_id = grouped.Key.team_id,
        //                bowler_id = grouped.Key.bowler_id,
        //                runsConceeded = grouped.Sum(d => d.runs_scored + d.extra_runs),
        //                validDeliveries = validDeliveries, // Store for later formatting
        //                wickets_taken = grouped.Count(d =>
        //                    d.wicket_type == "Bowled" ||
        //                    d.wicket_type == "Caught" ||
        //                    d.wicket_type == "Stumped")
        //            }).ToList(); // Materialize here to switch to LINQ-to-Objects

        //        // Step 2: Format overs as a string in memory (after ToList)
        //        var formattedBowStat = bowStat.Select(bs =>
        //        {
        //            int completedOvers = bs.validDeliveries / 6;
        //            int balls = bs.validDeliveries % 6;
        //            string overs = balls == 0
        //                ? $"{completedOvers}"
        //                : $"{completedOvers}.{balls}";

        //            return new
        //            {
        //                bs.fixture_id,
        //                bs.team_id,
        //                bs.bowler_id,
        //                bs.runsConceeded,
        //                overs,
        //                bs.wickets_taken
        //            };
        //        }).ToList();

        //        // Step 2: Create temporary data for #bowInfo
        //        var bowInfo = (from t in db.Teams
        //                       join p in db.Players on t.teamid equals p.team_id
        //                       join s in db.Students on p.reg_no equals s.reg_no
        //                       select new
        //                       {
        //                           teamid = t.teamid,
        //                           Tname = t.Tname,
        //                           reg_no = s.reg_no,
        //                           player_id = p.id,
        //                           player_name = s.name
        //                       }).ToList();

        //        // Step 3: Join #bowStat and #bowInfo
        //        var result = (from bi in bowInfo
        //                      join bs in formattedBowStat on bi.player_id equals bs.bowler_id
        //                      select new
        //                      {
        //                          team_id = bi.teamid,
        //                          team_name = bi.Tname,
        //                          player_id = bi.player_id,
        //                          player_name = bi.player_name,
        //                          runs_conceeded = bs.runsConceeded,
        //                          overs = bs.overs,
        //                          wickets_taken = bs.wickets_taken
        //                      }).ToList();
        //        var fixture = db.Fixtures.FirstOrDefault(d => d.id == fixtureId);
        //        var team1 = result.Where(score => score.team_id == fixture.team1_id).ToList();
        //        var team2 = result.Where(score => score.team_id == fixture.team2_id).ToList();
        //        var EachTeamIndividualStats = new
        //        {
        //            team1Stats = team1,
        //            team2Stats = team2
        //        };
        //        if (result.Count == 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotFound, "No bowling data found for the given fixture");
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, EachTeamIndividualStats);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}
    }
}