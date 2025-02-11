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
    
    public partial class Team
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Team()
        {
            this.CricketScores = new HashSet<CricketScore>();
            this.deliveries = new HashSet<delivery>();
            this.Fixtures = new HashSet<Fixture>();
            this.Fixtures1 = new HashSet<Fixture>();
            this.GoalBaseScores = new HashSet<GoalBaseScore>();
            this.Players = new HashSet<Player>();
            this.PointsBaseScores = new HashSet<PointsBaseScore>();
            this.ScoreCards = new HashSet<ScoreCard>();
            this.TurnBaseGames = new HashSet<TurnBaseGame>();
            this.TurnBaseGames1 = new HashSet<TurnBaseGame>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string className { get; set; }
        public int caption_id { get; set; }
        public int session_id { get; set; }
        public int sports_id { get; set; }
        public string image_path { get; set; }
        public bool teamStatus { get; set; }
        public string teamGender { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<CricketScore> CricketScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<delivery> deliveries { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<Fixture> Fixtures { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<Fixture> Fixtures1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<GoalBaseScore> GoalBaseScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<Player> Players { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<PointsBaseScore> PointsBaseScores { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<ScoreCard> ScoreCards { get; set; }
        [JsonIgnore] public virtual Session Session { get; set; }
        [JsonIgnore] public virtual Sport Sport { get; set; }
        [JsonIgnore] public virtual Team Teams1 { get; set; }
        [JsonIgnore] public virtual Team Team1 { get; set; }
        [JsonIgnore] public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<TurnBaseGame> TurnBaseGames { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore] public virtual ICollection<TurnBaseGame> TurnBaseGames1 { get; set; }
    }
}
