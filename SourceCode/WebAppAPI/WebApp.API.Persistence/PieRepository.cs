using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System.Data;

namespace WebApp.API.Persistence;

public class PieRepository : IPieRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string PieCache = "PieData";

	public PieRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<PieModel>> GetPies(int pageNumber)
	{
		PaginatedListModel<PieModel> output = _cache.Get<PaginatedListModel<PieModel>>(PieCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<PieModel, dynamic>("USP_Pie_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<PieModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(PieCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(PieCache);
			if (keys is null)
				keys = new List<string> { PieCache + pageNumber };
			else
				keys.Add(PieCache + pageNumber);
			_cache.Set(PieCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<PieModel> GetPieById(int pieId)
	{
		return (await _dataAccessHelper.QueryData<PieModel, dynamic>("USP_Pie_GetById", new { Id = pieId })).FirstOrDefault();
	}

	public async Task<List<PieModel>> GetPieByCategoryId(int categoryId)
	{
		return (await _dataAccessHelper.QueryData<PieModel, dynamic>("USP_Pie_GetByCategoryId", new { CategoryId = categoryId }));
	}

	public async Task<PieModel> GetPieByName(string pieName)
	{
		return (await _dataAccessHelper.QueryData<PieModel, dynamic>("USP_Pie_GetByName", new { Name = pieName })).FirstOrDefault();
	}

	public async Task<int> InsertPie(PieModel pie, LogModel logModel)
	{
		ClearCache(PieCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("Name", pie.Name);
		p.Add("Description", pie.Description);
		p.Add("Price", pie.Price);
		p.Add("ImageUrl", pie.ImageUrl);
		p.Add("ExpiryDate", pie.ExpiryDate);
		p.Add("InStock", pie.InStock);
		p.Add("CategoryId", pie.CategoryId);
		p.Add("CreatedBy", pie.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Pie_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdatePie(PieModel pie, LogModel logModel)
	{
		ClearCache(PieCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", pie.Id);
		p.Add("Name", pie.Name);
		p.Add("Description", pie.Description);
		p.Add("Price", pie.Price);
		p.Add("ImageUrl", pie.ImageUrl);
		p.Add("ExpiryDate", pie.ExpiryDate);
		p.Add("InStock", pie.InStock);
		p.Add("CategoryId", pie.CategoryId);
		p.Add("LastModifiedBy", pie.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Pie_Update", p);
	}

	public async Task DeletePie(int pieId, LogModel logModel)
	{
		ClearCache(PieCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", pieId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Pie_Delete", p);
	}

	public async Task<List<PieModel>> Export()
	{
		return await _dataAccessHelper.QueryData<PieModel, dynamic>("USP_Pie_Export", new { });
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case PieCache:
				var keys = _cache.Get<List<string>>(PieCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(PieCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}