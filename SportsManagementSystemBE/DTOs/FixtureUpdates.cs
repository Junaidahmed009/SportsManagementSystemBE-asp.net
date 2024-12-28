using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SportsManagementSystemBE.DTOs
{
    public class FixtureUpdates
    {
        public List<FixtureUpdate> UpdatedFixtures { get; set; }

    }
    public class FixtureUpdate
    {
        public int Fixtureid { get; set; }
        public int Team1id { get; set; }
        public int Team2id { get; set; }
    }
}