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
        public HttpResponseMessage GetStudents(string course, string sections,int semno)
        {
            try
            {
                //// Check if course and sections parameters are provided
                //if (string.IsNullOrEmpty(course) || string.IsNullOrEmpty(sections))
                //{
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Course and section parameters are required.");
                //}

                // Query students who match both course and section
                var Studentslist = db.Students
                    .Where(s => s.final_course == course && s.section == sections && s.sem_no==semno)
                    .Select(s => new { s.reg_no, s.name })
                    .ToList();

                // Check if the list is empty
                if (!Studentslist.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No students found for the specified course and section.");
                }

                // Return the list if students are found
                return Request.CreateResponse(HttpStatusCode.OK, Studentslist);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }

    }
}