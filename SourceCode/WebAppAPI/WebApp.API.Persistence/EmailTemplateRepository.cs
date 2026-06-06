using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System.Data;

namespace WebApp.API.Persistence;

public class EmailTemplateRepository : IEmailTemplateRepository
{
	private readonly IDataAccessHelper _dataAccessHelper;
	private readonly IConfiguration _config;
	private readonly IMemoryCache _cache;
	private const string EmailTemplateCache = "EmailTemplateData";

	public EmailTemplateRepository(IDataAccessHelper dataAccessHelper, IConfiguration config, IMemoryCache cache)
	{
		this._dataAccessHelper = dataAccessHelper;
		this._config = config;
		this._cache = cache;
	}

	#region "DataAccessHelper Methods"
	public async Task<PaginatedListModel<EmailTemplateModel>> GetEmailTemplates(int pageNumber)
	{
		PaginatedListModel<EmailTemplateModel> output = _cache.Get<PaginatedListModel<EmailTemplateModel>>(EmailTemplateCache + pageNumber);

		if (output is null)
		{
			DynamicParameters p = new DynamicParameters();
			p.Add("PageNumber", pageNumber);
			p.Add("PageSize", Convert.ToInt32(_config["SiteSettings:PageSize"]));
			p.Add("TotalRecords", DbType.Int32, direction: ParameterDirection.Output);

			var result = await _dataAccessHelper.QueryData<EmailTemplateModel, dynamic>("USP_EmailTemplate_GetAll", p);
			int TotalRecords = p.Get<int>("TotalRecords");
			int totalPages = (int)Math.Ceiling(TotalRecords / Convert.ToDouble(_config["SiteSettings:PageSize"]));

			output = new PaginatedListModel<EmailTemplateModel>
			{
				PageIndex = pageNumber,
				TotalRecords = TotalRecords,
				TotalPages = totalPages,
				HasPreviousPage = pageNumber > 1,
				HasNextPage = pageNumber < totalPages,
				Items = result.ToList()
			};

			_cache.Set(EmailTemplateCache + pageNumber, output, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));

			List<string> keys = _cache.Get<List<string>>(EmailTemplateCache);
			if (keys is null)
				keys = new List<string> { EmailTemplateCache + pageNumber };
			else
				keys.Add(EmailTemplateCache + pageNumber);
			_cache.Set(EmailTemplateCache, keys, TimeSpan.FromMinutes(Convert.ToInt32(_config["SiteSettings:ExpirationTime"])));
		}

		return output;
	}

	public async Task<EmailTemplateModel> GetEmailTemplateById(int emailTemplateId)
	{
		return (await _dataAccessHelper.QueryData<EmailTemplateModel, dynamic>("USP_EmailTemplate_GetById", new { Id = emailTemplateId })).FirstOrDefault();
	}

	public async Task<EmailTemplateModel> GetEmailTemplateByName(string name)
	{
		return (await _dataAccessHelper.QueryData<EmailTemplateModel, dynamic>("USP_EmailTemplate_GetByName", new { Name = name })).FirstOrDefault();
	}

	public async Task UpdateEmailTemplate(EmailTemplateModel emailTemplate, LogModel logModel)
	{
		ClearCache(EmailTemplateCache);

		DynamicParameters p = new DynamicParameters();
		p.Add("Id", emailTemplate.Id);
		p.Add("Subject", emailTemplate.Subject);
		p.Add("Template", emailTemplate.Template);
		p.Add("LastModifiedBy", emailTemplate.LastModifiedBy);
		p.Add("UserName", logModel.UserName);
		p.Add("UserRole", logModel.UserRole);
		p.Add("IP", logModel.IP);

		await _dataAccessHelper.ExecuteData("USP_EmailTemplate_Update", p);
	}
	#endregion

	#region "Helper Methods"
	private void ClearCache(string key)
	{
		switch (key)
		{
			case EmailTemplateCache:
				var keys = _cache.Get<List<string>>(EmailTemplateCache);
				if (keys is not null)
				{
					foreach (var item in keys)
						_cache.Remove(item);
					_cache.Remove(EmailTemplateCache);
				}
				break;
			default:
				break;
		}
	}
	#endregion
}