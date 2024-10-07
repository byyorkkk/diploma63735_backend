using System.ComponentModel.DataAnnotations;

namespace Backend63735.DTOs
{
    public class PillDto
    {

        public int? pill_ID { get; set; }

        [Required(ErrorMessage = "Pill Name is required")]
        public string? PillName { get; set; }
        public string? PillDose { get; set; }
    }
}
