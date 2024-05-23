namespace Domain.IRepository;

using Domain.Model;

public interface IHolidayPendingRepository : IGenericRepository<Holiday>
{
    Task<bool> HolidayExists(long id);
    Task<Holiday> GetHolidayByIdAsync(long id);
    Task<IEnumerable<Holiday>> GetHolidaysAsync();

    Task<Holiday> AddHoliday(Holiday holiday);
    Task<IEnumerable<Holiday>> GetHolidaysByColabIdAsync(long colabId);

    Task<bool> RemoveHoliday(long holidayId);
    

    

}
