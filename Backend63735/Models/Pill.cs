using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend63735.Models
{

    public partial class Pill
    {
        [Key]
        public int PillId { get; set; }

        [Required(ErrorMessage = "Pill Name is required")]
        public string PillName { get; set; }
        public string? PillDose { get; set; }
        public ICollection<CalendarRecordPill> CalendarRecordPills { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual IdentityUser User { get; set; }

        public int? CalendarRecordId { get; set; }
        public virtual CalendarRecord CalendarRecord { get; set; }
    }
}