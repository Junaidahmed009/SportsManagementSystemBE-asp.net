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
            this.CricketScores = new HashSet<CricketScore>();
            this.deliveries = new HashSet<delivery>();
            this.FixturesImages = new HashSet<FixturesImage>();
            this.GoalBaseScores = new HashSet<GoalBaseScore>();
            this.ManOfTheMatches = new HashSet<ManOfTheMatch>();
            this.Notifications = new HashSet<Notification>();
            this.PointsBaseScores = new HashSet<PointsBaseScore>();
            this.ScoreCards = new HashSet<ScoreCard>();
            this.TurnBaseGames = new HashSet<TurnBaseGame>();
        }
    
        public int id { get; set; }
        public Nullable<int> team1_id { get; set; }
        public Nullable<int> team2_id { get; set; }
        public System.DateTime matchDate { get; set; }
        public string venue { get; set; }
        public string match_type { get; set; }
        public Nullable<int> winner_id { get; set; }
        public Nullable<int> sessionSports_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<CricketScore> CricketScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<delivery> deliveries { get; set; }
        [JsonIgnore] public virtual SessionSport SessionSport { get; set; }
        [JsonIgnore] public virtual Team Team { get; set; }
        [JsonIgnore] public virtual Team Team1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<FixturesImage> FixturesImages { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<GoalBaseScore> GoalBaseScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<ManOfTheMatch> ManOfTheMatches { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<Notification> Notifications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<PointsBaseScore> PointsBaseScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<ScoreCard> ScoreCards { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<TurnBaseGame> TurnBaseGames { get; set; }
    }
}
