namespace WebApp.Core.Contract.Persistence;

public interface IDataAccessHelper
{
	Task<int> ExecuteData<T>(string storedProcedure, T parameters);
	Task<List<T>> QueryData<T, U>(string storedProcedure, U parameters);
}