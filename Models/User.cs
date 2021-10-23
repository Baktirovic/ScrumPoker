using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ScrumPoker.Models
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [StringLength(10, MinimumLength = 3)]
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; } 
        public DateTime TimeStamp { get; set; }
    }
}
