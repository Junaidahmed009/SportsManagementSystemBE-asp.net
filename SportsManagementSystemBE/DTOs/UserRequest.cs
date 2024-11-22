using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SportsManagementSystemBE.DTOs
{
    public class UserRequest
    {
        public int UserId { get; set; }
        public int? TeamNo { get; set; }

    }
}