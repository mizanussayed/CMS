using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System.Data;

namespace WebApp.API.Persistence;

public class CategoryRepository : ICategoryRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string CategoryCache = "CategoryData";
	private const string DistinctCategoryCache = "DistinctCategoryData";
	private const string CategoriesWithPiesCache = "CategoriesWithPiesData";

	public CategoryRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<CategoryModel>> GetCategories(int pageNumber)
	{
		PaginatedListModel<CategoryModel> output = _cache.Get<PaginatedListModel<CategoryModel>>(CategoryCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<CategoryModel, dynamic>("USP_Category_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<CategoryModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(CategoryCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(CategoryCache);
			if (keys is null)
				keys = new List<string> { CategoryCache + pageNumber };
			else
				keys.Add(CategoryCache + pageNumber);
			_cache.Set(CategoryCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<List<CategoryModel>> GetDistinctCategories()
	{
		var output = _cache.Get<List<CategoryModel>>(DistinctCategoryCache);

		if (output is null)
		{
			output = await _dataAccessHelper.QueryData<CategoryModel, dynamic>("USP_Category_GetDistinct", new { });
			_cache.Set(DistinctCategoryCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<CategoryModel> GetCategoryById(int categoryId)
	{
		return (await _dataAccessHelper.QueryData<CategoryModel, dynamic>("USP_Category_GetById", new { Id = categoryId })).FirstOrDefault();
	}

	public async Task<CategoryModel> GetCategoryByName(string categoryName)
	{
		return (await _dataAccessHelper.QueryData<CategoryModel, dynamic>("USP_Category_GetByName", new { Name = categoryName })).FirstOrDefault();
	}

	public async Task<int> InsertCategory(CategoryModel category, LogModel logModel)
	{
		ClearCache(CategoryCache);
		ClearCache(CategoriesWithPiesCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
		p.Add("Name", category.Name);
		p.Add("CreatedBy", category.CreatedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Category_Insert", p);
		return p.Get<int>("Id");
	}

	public async Task UpdateCategory(CategoryModel category, LogModel logModel)
	{
		ClearCache(CategoryCache);
		ClearCache(CategoriesWithPiesCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", category.Id);
		p.Add("Name", category.Name);
		p.Add("LastModifiedBy", category.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Category_Update", p);
	}

	public async Task DeleteCategory(int categoryId, LogModel logModel)
	{
		ClearCache(CategoryCache);
		ClearCache(CategoriesWithPiesCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", categoryId);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_Category_Delete", p);
	}

	public async Task<List<CategoryModel>> Export()
	{
		return await _dataAccessHelper.QueryData<CategoryModel, dynamic>("USP_Category_Export", new { });
	}
	#endregion

	#region "Customized Methods"
	public async Task<List<CategoryModel>> GetCategoriesWithPies()
	{
		var output = _cache.Get<List<CategoryModel>>(CategoriesWithPiesCache);

		if (output is null)
		{
			using (IDbConnection connection = new SqlConnection(_config.GetConnectionString("MSSQL")))
			{
				DynamicParameters p = new DynamicParameters();
				Dictionary<int, CategoryModel> dictionary = new Dictionary<int, CategoryModel>();

				var categories = await connection.QueryAsync<CategoryModel, PieModel, CategoryModel>("USP_Category_GetWithPies", (category, pie) =>
				{
					CategoryModel currentCategory;
					if (!dictionary.TryGetValue(category.Id, out currentCategory))
					{
						currentCategory = category;
						dictionary.Add(currentCategory.Id, currentCategory);
					}

					currentCategory.Pies.Add(pie);
					return currentCategory;
				}, p, commandType: CommandType.StoredProcedure);

				output = categories.Distinct().ToList();
				_cache.Set(CategoriesWithPiesCache, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
			}
		}

		return output;
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case CategoriesWithPiesCache:
				_cache.Remove(CategoriesWithPiesCache);
				break;
			case CategoryCache:
				var keys = _cache.Get<List<string>>(CategoryCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(CategoryCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}