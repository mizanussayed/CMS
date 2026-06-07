using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System.Data;

namespace WebApp.API.Persistence;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string AppointmentAllCache = "AppointmentAllData";
    private const string AppointmentByUserCache = "AppointmentByUserData";
    private const string AppointmentByDoctorCache = "AppointmentByDoctorData";
    private const string AppointmentByIdCache = "AppointmentByIdData";

    public AppointmentRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    public async Task<int> BookAppointment(AppointmentModel appointment, LogModel logModel)
    {
        ClearCache(AppointmentAllCache, AppointmentByUserCache, AppointmentByDoctorCache, AppointmentByIdCache);

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
        string cacheKey = AppointmentByUserCache + userId;
        var output = _cache.Get<List<AppointmentModel>>(cacheKey);

        if (output is null)
        {
            output = await _dataAccessHelper.QueryData<AppointmentModel, dynamic>("USP_Appointment_GetByUser", new { UserId = userId });
            SetCache(AppointmentByUserCache, cacheKey, output);
        }

        return output;
    }

    public async Task<List<AppointmentModel>> GetAllAppointments(int pageNumber)
    {
        var output = _cache.Get<List<AppointmentModel>>(AppointmentAllCache);

        if (output is null)
        {
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			output = await _dataAccessHelper.QueryData<AppointmentModel, dynamic>("USP_Appointment_GetAll", p);
            SetCache(AppointmentAllCache, AppointmentAllCache, output);
        }

        return output;
    }

    public async Task UpdateAppointmentStatus(int appointmentId, string status, LogModel logModel)
    {
        ClearCache(AppointmentAllCache, AppointmentByUserCache, AppointmentByDoctorCache, AppointmentByIdCache);

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
        ClearCache(AppointmentAllCache, AppointmentByUserCache, AppointmentByDoctorCache, AppointmentByIdCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", appointmentId);
        p.Add("UserName", userName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Appointment_Cancel", p);
    }


    public Task<AppointmentModel> GetAppointmentById(int appointmentId)
    {
        return GetAppointmentByIdInternal(appointmentId, AppointmentByIdCache + appointmentId);
    }

	public async Task DeleteAppointment(int appointmentId, string userName, LogModel logModel)
	{
		ClearCache(AppointmentAllCache, AppointmentByUserCache, AppointmentByDoctorCache, AppointmentByIdCache);
		DynamicParameters p = new DynamicParameters();
		p.Add("Id", appointmentId);
		p.Add("UserName", userName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);
		await _dataAccessHelper.ExecuteData("USP_Appointment_Delete", p);
	}


	private async Task<AppointmentModel> GetAppointmentByIdInternal(int appointmentId, string cacheKey)
    {
        var output = _cache.Get<AppointmentModel>(cacheKey);

        if (output is null)
        {
            output = (await _dataAccessHelper.QueryData<AppointmentModel, dynamic>("USP_Appointment_GetById", new { Id = appointmentId })).FirstOrDefault();
            SetCache(AppointmentByIdCache, cacheKey, output);
        }

        return output;
    }

    private void SetCache<T>(string rootKey, string cacheKey, T value)
    {
        _cache.Set(cacheKey, value, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

        List<string> keys = _cache.Get<List<string>>(rootKey);
        if (keys is null)
            keys = new List<string> { cacheKey };
        else if (!keys.Contains(cacheKey))
            keys.Add(cacheKey);

        _cache.Set(rootKey, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
    }

    private void ClearCache(params string[] rootKeys)
    {
        foreach (var rootKey in rootKeys)
        {
            var keys = _cache.Get<List<string>>(rootKey);
            if (keys is not null)
            {
                foreach (var key in keys)
                    _cache.Remove(key);
            }

            _cache.Remove(rootKey);
        }
    }
}
