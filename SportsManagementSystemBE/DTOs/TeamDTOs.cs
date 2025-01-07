using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SportsManagementSystemBE.DTOs
{
    public class TeamDTOs
    {
        
        public string Name { get; set; }
        public string  ClassName{ get; set; }
        //public int Session_id { get; set; }
        public int Caption_id { get; set; }
        public int Sports_id { get; set; }
        public string Image_path { get; set; }
        public bool TeamStatus { get; set; }
        public string TeamType { get; set; }

    }
}
