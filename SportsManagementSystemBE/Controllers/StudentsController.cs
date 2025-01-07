using SportsManagementSystemBE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SportsManagementSystemBE.Controllers
{
    public class StudentsController : ApiController
    {
        private SportsManagementSystemEntities db = new SportsManagementSystemEntities();

        [HttpGet]
        public HttpResponseMessage GetStudents(string course, string sections,int semno,string gender)
        {
            try
            {
                var Studentslist = db.Students
                    .Where(s => s.discipline == course && s.section == sections && s.semNo==semno && s.gender == gender)
                    .Select(s => new { s.reg_no, s.name })
                    .ToList();
                if (!Studentslist.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No students found for the specified course and section.");
                }
                return Request.CreateResponse(HttpStatusCode.OK, Studentslist);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

    }
}