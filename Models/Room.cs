using System;
using System.Collections.Generic;

namespace ScrumPoker.Models
{
    public class Room
    {
        public HashSet<User> Users { get; set; }
        public List<Vote> Votes { get; set; }
        public string Admin { get; set; }
        public string Name { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
