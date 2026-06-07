using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class DoctorModel : AuditModel
{
    public int Id { get; set; }
    public int DoctorID { get => Id; set => Id = value; }

    [Required(ErrorMessage = "Please enter 'Name'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
    public string Name { get; set; }

    [MaxLength(150, ErrorMessage = "Maximum length of 'Specialization' is 150 characters.")]
    public string Specialization { get; set; }

    // Represent available slots as comma-separated time windows, e.g. "09:00-12:00,14:00-17:00"
    [MaxLength(500, ErrorMessage = "AvailableSlots value is too long.")]
    public string AvailableSlots { get; set; }
}
