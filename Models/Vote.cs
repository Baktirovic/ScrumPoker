using System;
using Newtonsoft.Json;

namespace ScrumPoker.Models
{
    public class Vote
    {
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("userid")]
        public string UserId { get; set; } 
    }
}
