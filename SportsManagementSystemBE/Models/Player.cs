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
    
    public partial class Player
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Player()
        {
            this.deliveries = new HashSet<delivery>();
            this.deliveries1 = new HashSet<delivery>();
            this.deliveries2 = new HashSet<delivery>();
            this.deliveries3 = new HashSet<delivery>();
            this.deliveries4 = new HashSet<delivery>();
            this.ManOfTheMatches = new HashSet<ManOfTheMatch>();
            this.ScoreCards = new HashSet<ScoreCard>();
        }
    
        public int id { get; set; }
        public string reg_no { get; set; }
        public int team_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<delivery> deliveries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<delivery> deliveries1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<delivery> deliveries2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<delivery> deliveries3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<delivery> deliveries4 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<ManOfTheMatch> ManOfTheMatches { get; set; }
        [JsonIgnore] public virtual Student Student { get; set; }
        [JsonIgnore] public virtual Team Team { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<ScoreCard> ScoreCards { get; set; }
    }
}
