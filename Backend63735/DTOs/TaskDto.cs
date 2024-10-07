using static Backend63735.Models.Task;

namespace Backend63735.DTOs
{
    public class TaskDto
    {
        public int? Id { get; set; }
        public string? Description { get; set; }
        public int TaskStatus { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime DeadlineDay { get; set; }
        public int? PomodoroSessions { get; set; }
    }

}
