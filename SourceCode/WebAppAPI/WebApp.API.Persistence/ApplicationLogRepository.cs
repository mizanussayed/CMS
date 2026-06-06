using Dapper;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System.Data;

namespace WebApp.API.Persistence;

public class ApplicationLogRepository : IApplicationLogRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;

	public ApplicationLogRepository(IDataAccessHelper dataAccessHelper, IConfiguration config)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
	}

	public async Task<PaginatedListModel<ApplicationLogModel>> GetApplicationLogs(int pageNumber)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("PageNumber", pageNumber);
		p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
		p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

		var result = await _dataAccessHelper.QueryData<ApplicationLogModel, dynamic>("USP_ApplicationLog_GetAll", p);
		int TotalRecords = p.Get<int>("TotalRecords");
		int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

		PaginatedListModel<ApplicationLogModel> output = new PaginatedListModel<ApplicationLogModel>
		{
			PageIndex = pageNumber,
			TotalRecords = TotalRecords,
			TotalPages = totalPages,
			HasPreviousPage = pageNumber > 1,
			HasNextPage = pageNumber < totalPages,
			Items = result.ToList()
		};

		return output;
	}

	public async Task<ApplicationLogModel> GetApplicationLogById(int applicationLogId)
	{
		return (await _dataAccessHelper.QueryData<ApplicationLogModel, dynamic>("USP_ApplicationLog_GetById", new { Id = applicationLogId })).FirstOrDefault();
	}

	public async Task DeleteApplicationLog(int applicationLogId)
	{
		await _dataAccessHelper.ExecuteData("USP_ApplicationLog_Delete", new { Id = applicationLogId });
	}

	public async Task<List<ApplicationLogModel>> Export()
	{
		return await _dataAccessHelper.QueryData<ApplicationLogModel, dynamic>("USP_ApplicationLog_Export", new { });
	}
}