namespace Domain.IRepository;

using Domain.Model;

public interface IHolidayRepository : IGenericRepository<Holiday>
{
    Task<bool> HolidayExists(long id);
    //Task<Holiday> AddHolidayPeriod(Holiday holiday, List<string> errorMessages);
    Task<Holiday> AddHoliday(Holiday holiday);
    Task<Holiday> GetHolidayByIdAsync(long id);
    Task<IEnumerable<Holiday>> getHolidaysByColabIdInPeriod(long colabId, DateOnly startDate, DateOnly endDate);

    Task<bool> DoesColabHasHolidayInPeriod(long id, DateOnly startDate, DateOnly endDate);
    
}
