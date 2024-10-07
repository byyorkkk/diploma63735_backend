using System.ComponentModel.DataAnnotations;
using static Backend63735.Models.CalendarRecord;

namespace Backend63735.DTOs
{
    public class CalendarRecordDto
    {

        public int? Id { get; set; }
        public int? MoodStatus { get; set; }
        public DateTime Date { get; set; }
        public string? NoteDescription { get; set; }
        public List<int>? PillIds { get; set; }
    }
}
