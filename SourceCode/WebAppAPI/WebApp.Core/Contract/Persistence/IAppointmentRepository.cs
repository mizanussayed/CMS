using WebApp.Core.Model;

namespace WebApp.Core.Contract.Persistence;

public interface IAppointmentRepository
{
    Task<int> BookAppointment(AppointmentModel appointment, LogModel logModel);
    Task<List<AppointmentModel>> GetAppointmentsByUser(int userId);
    Task<List<AppointmentModel>> GetAllAppointments();
    Task UpdateAppointmentStatus(int appointmentId, string status, LogModel logModel);
    Task CancelAppointment(int appointmentId, string userName, LogModel logModel);
}
