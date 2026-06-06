using WebApp.Core.Model;

namespace WebApp.Core.Contract.Persistence;

public interface IPieRepository
{
	Task<PaginatedListModel<PieModel>> GetPies(int pageNumber);
	Task<PieModel> GetPieById(int pieId);
	Task<List<PieModel>> GetPieByCategoryId(int categoryId);
	Task<PieModel> GetPieByName(string pieName);
	Task<int> InsertPie(PieModel pie, LogModel logModel);
	Task UpdatePie(PieModel pie, LogModel logModel);
	Task DeletePie(int pieId, LogModel logModel);
	Task<List<PieModel>> Export();
}