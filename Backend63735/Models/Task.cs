using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend63735.Models
{
    public partial class Task
    {
        [Key]
        public int Id { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime DeadlineDay { get; set; }
        public string Description { get; set; }

        public Status TaskStatus { get; set; }
        public int? PomodoroSessions { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual IdentityUser User { get; set; }

        public int? CalendarRecordId { get; set; }
        public virtual CalendarRecord CalendarRecord { get; set; }

        public enum Status
        {
            NotStarted = 0,
            InProgress = 1,
            Done = 2
        }

    }
}

