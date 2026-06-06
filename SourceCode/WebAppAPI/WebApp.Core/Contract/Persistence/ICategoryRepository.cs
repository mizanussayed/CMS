using WebApp.Core.Model;

namespace WebApp.Core.Contract.Persistence;

public interface ICategoryRepository
{
	Task<PaginatedListModel<CategoryModel>> GetCategories(int pageNumber);
	Task<List<CategoryModel>> GetDistinctCategories();
	Task<CategoryModel> GetCategoryById(int categoryId);
	Task<CategoryModel> GetCategoryByName(string categoryName);
	Task<int> InsertCategory(CategoryModel category, LogModel logModel);
	Task UpdateCategory(CategoryModel category, LogModel logModel);
	Task DeleteCategory(int categoryId, LogModel logModel);
	Task<List<CategoryModel>> GetCategoriesWithPies();
	Task<List<CategoryModel>> Export();
}