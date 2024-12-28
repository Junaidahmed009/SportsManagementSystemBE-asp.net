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
    using System;
    using System.Collections.Generic;
    
    public partial class Match_events
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Match_events()
        {
            this.FixturesImages = new HashSet<FixturesImage>();
        }
    
        public int id { get; set; }
        public Nullable<int> fixture_id { get; set; }
        public Nullable<System.DateTime> event_time { get; set; }
        public string event_type { get; set; }
        public string event_description { get; set; }
        public Nullable<int> sessionSports_id { get; set; }
        public Nullable<int> player_id { get; set; }
        public Nullable<int> secondary_player_id { get; set; }
        public Nullable<int> fielder_id { get; set; }
    
        public virtual Fixture Fixture { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FixturesImage> FixturesImages { get; set; }
        public virtual Player Player { get; set; }
        public virtual Player Player1 { get; set; }
        public virtual SessionSport SessionSport { get; set; }
        public virtual Player Player2 { get; set; }
    }
}
