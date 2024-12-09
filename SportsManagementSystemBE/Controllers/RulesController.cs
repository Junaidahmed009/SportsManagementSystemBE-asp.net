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
                var rulesdata = db.Rules
                                  .Where(r => r.sports_id == Sportsid)
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
        public HttpResponseMessage UpdateRules(Rule data)
        {
            try
            {
                var existingRule = db.Rules.FirstOrDefault(r => r.sports_id == data.sports_id);

                if (existingRule == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Rule not found for the given sports ID.");
                }

                // Update the existing rule
                existingRule.rules_of_game = data.rules_of_game;

                // Save changes to the database
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Rule updated successfully."); // Changed to 200 OK
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error saving the rule: {ex.Message}");
            }
        }
        [HttpGet]
        public HttpResponseMessage FetchCricketRules()
        {
            try
            {
                var CricketRule=db.Rules.
                   Where(r=>r.id==1).
                   Select(r => new {r.rules_of_game}).
                   ToList();
                if (CricketRule == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Rule not found.");
                }
                return Request.CreateResponse(HttpStatusCode.OK, CricketRule);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error Fetching the rule: {ex.Message}");
            }
        }


    }
}