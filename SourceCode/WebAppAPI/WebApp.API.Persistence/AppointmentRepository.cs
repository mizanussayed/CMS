using Dapper;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System.Data;

namespace WebApp.API.Persistence;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;

    public AppointmentRepository(IDataAccessHelper dataAccessHelper, IConfiguration config)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
    }

    public async Task<int> BookAppointment(AppointmentModel appointment, LogModel logModel)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
        p.Add("UserId", appointment.UserId);
        p.Add("DoctorId", appointment.DoctorId);
        p.Add("AppointmentDate", appointment.AppointmentDate);
        p.Add("Status", appointment.Status ?? "Pending");
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Appointment_Book", p);
        return p.Get<int>("Id");
    }

    public async Task<List<AppointmentModel>> GetAppointmentsByUser(int userId)
    {
        return await _dataAccessHelper.QueryData<AppointmentModel, dynamic>("USP_Appointment_GetByUser", new { UserId = userId });
    }

    public async Task<List<AppointmentModel>> GetAllAppointments()
    {
        return await _dataAccessHelper.QueryData<AppointmentModel, dynamic>("USP_Appointment_GetAll", new { });
    }

    public async Task UpdateAppointmentStatus(int appointmentId, string status, LogModel logModel)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("Id", appointmentId);
        p.Add("Status", status);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Appointment_UpdateStatus", p);
    }

    public async Task CancelAppointment(int appointmentId, string userName, LogModel logModel)
    {
        DynamicParameters p = new DynamicParameters();
        p.Add("Id", appointmentId);
        p.Add("UserName", userName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Appointment_Cancel", p);
    }
}
