using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System.Data;

namespace WebApp.API.Persistence;

public class DoctorRepository : IDoctorRepository
{
    private readonly IDataAccessHelper _dataAccessHelper;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private const string DoctorCache = "DoctorData";

    public DoctorRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
    {
        this._dataAccessHelper = dataAccessHelper;
        this._config = config;
        this._cache = cache;
    }

    public async Task<PaginatedListModel<DoctorModel>> GetDoctors(int pageNumber)
    {
        PaginatedListModel<DoctorModel> output = _cache.Get<PaginatedListModel<DoctorModel>>(DoctorCache + pageNumber);

        if (output is null)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("PageNumber", pageNumber);
            p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
            p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _dataAccessHelper.QueryData<DoctorModel, dynamic>("USP_Doctor_GetAll", p);
            int TotalRecords = p.Get<int>("TotalRecords");
            int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

            output = new PaginatedListModel<DoctorModel>
            {
                PageIndex = pageNumber,
                TotalRecords = TotalRecords,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages,
                Items = result.ToList()
            };

            _cache.Set(DoctorCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

            List<string> keys = _cache.Get<List<string>>(DoctorCache);
            if (keys is null)
                keys = new List<string> { DoctorCache + pageNumber };
            else
                keys.Add(DoctorCache + pageNumber);
            _cache.Set(DoctorCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
        }

        return output;
    }

    public async Task<List<DoctorModel>> GetDistinctSpecializations()
    {
        return await _dataAccessHelper.QueryData<DoctorModel, dynamic>("USP_Doctor_GetDistinctSpecializations", new { });
    }

    public async Task<DoctorModel> GetDoctorById(int doctorId)
    {
        return (await _dataAccessHelper.QueryData<DoctorModel, dynamic>("USP_Doctor_GetById", new { Id = doctorId })).FirstOrDefault();
    }

    public async Task<int> InsertDoctor(DoctorModel doctor, LogModel logModel)
    {
        ClearCache(DoctorCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
        p.Add("Name", doctor.Name);
        p.Add("Specialization", doctor.Specialization);
        p.Add("AvailableSlots", doctor.AvailableSlots);
        p.Add("CreatedBy", doctor.CreatedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Doctor_Insert", p);
        return p.Get<int>("Id");
    }

    public async Task UpdateDoctor(DoctorModel doctor, LogModel logModel)
    {
        ClearCache(DoctorCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", doctor.Id);
        p.Add("Name", doctor.Name);
        p.Add("Specialization", doctor.Specialization);
        p.Add("AvailableSlots", doctor.AvailableSlots);
        p.Add("LastModifiedBy", doctor.LastModifiedBy);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Doctor_Update", p);
    }

    public async Task DeleteDoctor(int doctorId, LogModel logModel)
    {
        ClearCache(DoctorCache);

        DynamicParameters p = new DynamicParameters();
        p.Add("Id", doctorId);
        p.Add("UserName", logModel.UserName);
        p.Add("UserRole", logModel.UserRole);
        p.Add("IP", logModel.IP);

        await _dataAccessHelper.ExecuteData("USP_Doctor_Delete", p);
    }

    public async Task<List<DoctorModel>> Export()
    {
        return await _dataAccessHelper.QueryData<DoctorModel, dynamic>("USP_Doctor_Export", new { });
    }

    private void ClearCache(string key)
    {
        var keys = _cache.Get<List<string>>(key);
        if (keys is not null)
        {
            foreach (var item in keys)
                _cache.Remove(item);
            _cache.Remove(key);
        }
    }
}
