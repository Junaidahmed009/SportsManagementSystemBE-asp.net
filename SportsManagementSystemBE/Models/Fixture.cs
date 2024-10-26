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
    
    public partial class Fixture
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Fixture()
        {
            this.Comments = new HashSet<Comment>();
            this.CricketScores = new HashSet<CricketScore>();
            this.FixturesImages = new HashSet<FixturesImage>();
            this.GoalBaseScores = new HashSet<GoalBaseScore>();
            this.PointsBaseScores = new HashSet<PointsBaseScore>();
            this.TurnBaseGames = new HashSet<TurnBaseGame>();
        }
    
        public int id { get; set; }
        public int team1_id { get; set; }
        public int team2_id { get; set; }
        public System.DateTime matchDate { get; set; }
        public string venue { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<Comment> Comments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<CricketScore> CricketScores { get; set; }
        [JsonIgnore] public virtual Team Team { get; set; }
        [JsonIgnore] public virtual Team Team1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<FixturesImage> FixturesImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<GoalBaseScore> GoalBaseScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<PointsBaseScore> PointsBaseScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<TurnBaseGame> TurnBaseGames { get; set; }
    }
}
