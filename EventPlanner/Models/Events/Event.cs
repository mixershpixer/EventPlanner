using EventPlanner.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventPlanner.Models
{
    public class Event
    {
        public int Id { get; set; }
        [Display(Name = "Event title")]
        public string Title { get; set; }
        public ICollection<Field> Fields { get; set; }
        [Display(Name = "Max users")]
        public int MaxUsers { get; set; }
        [Display(Name = "Subscribed users")]
        public int SignedUsersCount { get; set; }

        public string UserId { get; set; }
        public ApplicationUser UserCreator { get; set; }

        public ICollection<UserEvent> EventUsersSigned { get; set; }
        public Event()
        {
            this.Fields = new List<Field>();
            this.EventUsersSigned = new List<UserEvent>();
        }
    }
}