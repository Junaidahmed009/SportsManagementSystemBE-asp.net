using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace SportsManagementSystemBE.Controllers
{
    public class RulesController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();


        [HttpGet]
        public HttpResponseMessage FetchRules(int Sportsid)
        {
            try
            {
                var sportidExists = db.Rules.Any(r => r.sportrs_id == Sportsid);
                if (!sportidExists)
                {
                    // If the Sportsid doesn't exist, insert a new record with null rules_of_game and sports id
                    var newRule = new Rule
                    {
                        sportrs_id = Sportsid,
                        rules_of_game = " "
                    };
                    db.Rules.Add(newRule);
                    db.SaveChanges();
                    var senruleddata = db.Rules.Where(r=>r.sportrs_id == Sportsid).Select(r => new { r.rules_of_game })
                                  .ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, senruleddata);
                }

                var rulesdata = db.Rules
                                  .Where(r => r.sportrs_id == Sportsid)
                                  .Select(r => new { r.rules_of_game })
                                  .ToList();
                if (rulesdata == null || !rulesdata.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No rules found for the given Sportsid.");
                }
                return Request.CreateResponse(HttpStatusCode.OK, rulesdata);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }


        [HttpPost]
        public HttpResponseMessage UpdateRules([FromBody]Rule data)
        {
            try
            {
                var existingRule = db.Rules.FirstOrDefault(r => r.sportrs_id == data.sportrs_id);

                if (existingRule != null)
                {
                    // Update the existing rule
                    existingRule.rules_of_game = data.rules_of_game;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error saving the rule: {ex.Message}");
            }
        }


    }
}