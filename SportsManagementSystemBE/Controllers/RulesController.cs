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
        public HttpResponseMessage GetSports()
        {
            try
            {
                var sports = db.Sports
                               .Select(s => new { s.id, s.games })
                               .ToList();
                if (sports == null || !sports.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, sports);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage FetchRules(Rule data)
        {
            try
            {
                var rulesdata = db.Rules
                              .Where(r => r.sportrs_id == data.sportrs_id)
                              .Select(r => new { r.rules_of_game })
                              .ToList();
                if (rulesdata == null || !rulesdata.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                return Request.CreateResponse(HttpStatusCode.OK, rulesdata);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage UpdateRules([FromBody] Rule data)
        {
            try
            {
                var existingRule = db.Rules.FirstOrDefault(r => r.sportrs_id == data.sportrs_id);

                if (existingRule != null)
                {
                    // Update the existing rule
                    existingRule.rules_of_game = data.rules_of_game;
                }
                else
                {
                    // Add the new rule
                    db.Rules.Add(data);
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