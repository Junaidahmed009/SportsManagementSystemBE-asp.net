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
    using System; using Newtonsoft.Json;
    using System.Collections.Generic;
    
    public partial class ManOfTheMatch
    {
        public int id { get; set; }
        public int fixture_id { get; set; }
        public int player_id { get; set; }
        public string image_path { get; set; }
    
        [JsonIgnore] public virtual Fixture Fixture { get; set; }
        [JsonIgnore] public virtual Player Player { get; set; }
    }
}
