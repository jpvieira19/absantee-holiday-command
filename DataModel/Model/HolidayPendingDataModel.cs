using DataModel.Mapper;
using DataModel.Model;
using Domain.Model;

public class HolidayPendingDataModel
{
    public long Id { get; set; }
    public ColaboratorsIdDataModel colaboratorId { get; set; }
    public DateOnly _startDate { get; set; }
    public DateOnly _endDate { get; set; }

    public HolidayPendingDataModel() {}

    public HolidayPendingDataModel(Holiday holiday, ColaboratorsIdDataModel colaboratorsIdDataModel)
    {
        Id = holiday.Id;

        // Assuming you have a mapper that converts from the domain Colaborator model to the data model
        colaboratorId = colaboratorsIdDataModel;

        _startDate = holiday.HolidayPeriod.StartDate;

        _endDate = holiday.HolidayPeriod.EndDate;
    }
}
