//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SportsManagementSystemBE.Models
{
    using System;  using Newtonsoft.Json;
    using System.Collections.Generic;
    
    public partial class PointsBaseScore
    {
        public int id { get; set; }
        public int team_id { get; set; }
        public int fixture_id { get; set; }
        public int setsWon { get; set; }
    
        [JsonIgnore] public virtual Fixture Fixture { get; set; }
        [JsonIgnore] public virtual Team Team { get; set; }
    }
}
