using WebApp.Core.Model;

namespace WebApp.Core.Contract.Persistence;

public interface IDoctorRepository
{
    Task<PaginatedListModel<DoctorModel>> GetDoctors(int pageNumber);
    Task<List<DoctorModel>> GetDistinctSpecializations();
    Task<DoctorModel> GetDoctorById(int doctorId);
	Task<DoctorModel> GetDoctorByName(string doctorName);
    Task<int> InsertDoctor(DoctorModel doctor, LogModel logModel);
    Task UpdateDoctor(DoctorModel doctor, LogModel logModel);
    Task DeleteDoctor(int doctorId, LogModel logModel);
    Task<List<DoctorModel>> Export();
}
