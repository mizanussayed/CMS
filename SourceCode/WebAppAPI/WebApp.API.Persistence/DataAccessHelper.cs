/*
// Using empty parameter
return await _dataAccess.QueryData<ProductDTO, dynamic>(sqlStatement, new { }, _connectionStringName);

// Using anonymous
return await _dataAccess.QueryData<ProductDTO, dynamic>(sqlStatement, new { CategoryId = categoryId }, _connectionStringName);

// Using Dynamic Parameters
DynamicParameters p = new DynamicParameters();
p.Add("CategoryId", productDTO.categoryId);
return await _dataAccess.QueryData<ProductDTO, dynamic>(sqlStatement, p, _connectionStringName);

// Using anonymous
return await _dataAccess.ExecuteData(sqlStatement, new { Id = id }, _connectionStringName);

// Using Dynamic Parameters
DynamicParameters p = new DynamicParameters();
p.Add("Title", productDTO.Title);
p.Add("CategoryId", productDTO.CategoryId);
return await _dataAccess.ExecuteData(sqlStatement, p, _connectionStringName);

// Using Dynamic Parameters with output parameter
DynamicParameters p = new DynamicParameters();
p.Add("Id", DbType.Int32, direction: ParameterDirection.Output);
p.Add("Name", vendorDTO.Name);
await _dataAccess.ExecuteData(sqlStatement, p, _connectionStringName);
return p.Get<int>("Id");
*/

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using System.Data;

namespace WebApp.API.Persistence;

public class DataAccessHelper : IDataAccessHelper
{
	private readonly IConfiguration _config;

	public DataAccessHelper(IConfiguration config)
	{
		this._config = config;
	}

	public async Task<List<T>> QueryData<T, U>(string storedProcedure, U parameters)
	{
		using (IDbConnection connection = new SqlConnection(_config.GetConnectionString("MSSQL")))
		{
			var rows = await connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
			return rows.ToList();
		}
	}

	public async Task<int> ExecuteData<T>(string storedProcedure, T parameters)
	{
		using (IDbConnection connection = new SqlConnection(_config.GetConnectionString("MSSQL")))
		{
			return await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
		}
	}
}