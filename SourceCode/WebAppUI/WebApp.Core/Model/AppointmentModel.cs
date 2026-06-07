using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class AppointmentModel : AuditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please provide UserId.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Please provide DoctorId.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Please provide AppointmentDate.")]
    public DateTime AppointmentDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; }
}
