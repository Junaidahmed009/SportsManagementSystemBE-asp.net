using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SportsManagementSystemBE.DTOs
{
    public class PlayersRequest
    {
        public List<string> RollNumbers { get; set; }
        public int TeamNo { get; set; }     
    }
}