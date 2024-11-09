            using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SportsManagementSystemBE.Controllers
{
    public class ValuesController : ApiController
    {
        /*
         * ### Success Status Codes:
1. **200 OK**: The request was successful, and the server returned the requested data.
   - `HttpStatusCode.OK`
   
2. **201 Created**: A new resource has been successfully created on the server.
   - `HttpStatusCode.Created`
   
3. **204 No Content**: The request was successful, but there is no content to return (often used after an update or delete operation).
   - `HttpStatusCode.NoContent`

### Client Error Status Codes:
1. **400 Bad Request**: The server cannot process the request due to a client error (e.g., invalid input).
   - `HttpStatusCode.BadRequest`
   
2. **401 Unauthorized**: Authentication is required, but the request has not been authenticated.
   - `HttpStatusCode.Unauthorized`
   
3. **403 Forbidden**: The server understood the request but refuses to authorize it.
   - `HttpStatusCode.Forbidden`
   
4. **404 Not Found**: The requested resource could not be found on the server.
   - `HttpStatusCode.NotFound`

5. **409 Conflict**: The request could not be completed due to a conflict with the current state of the target resource (e.g., duplicate data).
   - `HttpStatusCode.Conflict`

### Server Error Status Codes:
1. **500 Internal Server Error**: A generic server error occurred, typically indicating that something went wrong on the server side.
   - `HttpStatusCode.InternalServerError`
   
2. **503 Service Unavailable**: The server is currently unable to handle the request (due to maintenance, overloading, etc.).
   - `HttpStatusCode.ServiceUnavailable`
*/


        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
