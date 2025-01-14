using SportsManagementSystemBE.DTOs;
using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SportsManagementSystemBE.Controllers
{
    public class MatchEventsController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();
        [HttpPost]
        public HttpResponseMessage AddMatchEvents(Match_events matchEvent, string ImgPath)
        {
            try
            {
                var fixture = db.Fixtures.FirstOrDefault(f => f.id == matchEvent.fixture_id);
                var Eventtime = DateTime.Now;
                int sessionSport_id = (int)fixture.sessionSports_id;
                var FixtureImg = new FixturesImage
                {
                    fixtures_id = (int)matchEvent.fixture_id,
                    imagePath = ImgPath,
                    image_time = Eventtime,
                    event_id = matchEvent.id
                };
              
                var newMatchEvent = new Match_events
                {
                    fixture_id = matchEvent.fixture_id,
                    event_time = Eventtime,
                    event_type = matchEvent.event_type,
                    event_description = matchEvent.event_description,
                    sessionSports_id = sessionSport_id,
                    player_id = matchEvent.player_id,
                    secondary_player_id = matchEvent.secondary_player_id,
                    fielder_id = matchEvent.fielder_id,

                };
                db.Match_events.Add(newMatchEvent);
                db.FixturesImages.Add(FixtureImg);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);//, $"{matchEvent.id} matchEvents added successfully."
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}