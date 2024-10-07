using AutoMapper;
using Backend63735.DTOs;
using Backend63735.Models;
using Task = Backend63735.Models.Task;

namespace Backend63735
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // PillDto to Pill Mapping
            CreateMap<PillDto, Pill>()
                .ForMember(dest => dest.PillId, opt => opt.MapFrom(src => src.pill_ID))
                .ForMember(dest => dest.PillDose, opt => opt.MapFrom(src => src.PillDose))
                .ReverseMap()
                .ForMember(dest => dest.pill_ID, opt => opt.MapFrom(src => src.PillId))
                .ForMember(dest => dest.PillDose, opt => opt.MapFrom(src => src.PillDose));

            // Task to TaskDto Mapping
            CreateMap<Task, TaskDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TaskStatus, opt => opt.MapFrom(src => (int)src.TaskStatus))
                .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.StartDay))
                .ForMember(dest => dest.DeadlineDay, opt => opt.MapFrom(src => src.DeadlineDay))
                .ForMember(dest => dest.PomodoroSessions, opt => opt.MapFrom(src => src.PomodoroSessions));

            // TaskDto to Task Mapping
            CreateMap<TaskDto, Task>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.TaskStatus, opt => opt.MapFrom(src => (Task.Status)src.TaskStatus))
                .ForMember(dest => dest.StartDay, opt => opt.MapFrom(src => src.StartDay))
                .ForMember(dest => dest.DeadlineDay, opt => opt.MapFrom(src => src.DeadlineDay))
                .ForMember(dest => dest.PomodoroSessions, opt => opt.MapFrom(src => src.PomodoroSessions));

            // CalendarRecord to CalendarRecordDto Mapping
            CreateMap<CalendarRecord, CalendarRecordDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.MoodStatus, opt => opt.MapFrom(src => (int?)src.MoodState))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.NoteDescription, opt => opt.MapFrom(src => src.NoteDescription))
                .ForMember(dest => dest.PillIds, opt => opt.MapFrom(src => src.CalendarRecordPills.Select(crp => crp.PillId).ToList()));

            CreateMap<CalendarRecordDto, CalendarRecord>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.MoodState, opt => opt.MapFrom(src => src.MoodStatus.HasValue ? (CalendarRecord.Mood?)src.MoodStatus.Value : null))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.NoteDescription, opt => opt.MapFrom(src => src.NoteDescription))
                .ForMember(dest => dest.CalendarRecordPills,
                    opt => opt.MapFrom(src =>
                        src.PillIds != null ? src.PillIds.Select(id => new CalendarRecordPill { PillId = id }).ToList() : null));
        }
    }
}
