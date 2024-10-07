namespace Backend63735.Models
{
    public class CalendarRecordPill
    {
        public int CalendarRecordId { get; set; }
        public CalendarRecord CalendarRecord { get; set; }

        public int PillId { get; set; }
        public Pill Pill { get; set; }
    }
}
