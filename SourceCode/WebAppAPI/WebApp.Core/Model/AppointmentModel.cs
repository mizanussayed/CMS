using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class AppointmentModel : AuditModel
{
    public int AppointmentID { get; set; }

    [Required(ErrorMessage = "Please provide UserId.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Please provide DoctorId.")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Please provide AppointmentDate.")]
    public DateTime AppointmentDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; }
	public string UserName { get; set; } = string.Empty;
	public string DoctorName { get; set; } = string.Empty;
}
