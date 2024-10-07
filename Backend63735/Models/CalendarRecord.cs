using MessagePack;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace Backend63735.Models
{
    public class CalendarRecord
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public ICollection<CalendarRecordPill> CalendarRecordPills { get; set; }
        public string? NoteDescription { get; set; }
        public Mood? MoodState { get; set; }
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual IdentityUser User { get; set; }
        public enum Mood
        {
            Sad = 0,

            Angry = 1,

            Happy = 2,

            Neutral = 3,

            Disappointed = 4
        }
    }
}
