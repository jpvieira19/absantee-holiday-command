namespace DataModel.Mapper;

using DataModel.Model;
using Domain.Model;
using Domain.Factory;
using System.Linq;
using System;

public class HolidayPendingMapper
{
    private IHolidayFactory _holidayFactory;

    public HolidayPendingMapper(
        IHolidayFactory holidayFactory)
    {
        _holidayFactory = holidayFactory;
    }


    public Holiday ToDomain(HolidayPendingDataModel holidayDM)
    {
        long id = holidayDM.Id;
        ColaboratorsIdDataModel colabId = holidayDM.colaboratorId;

        long cId = colabId.Id;

        IHolidayPeriodFactory _holidayPeriodFactory = new HolidayPeriodFactory();
        HolidayPeriod holidayPeriod = _holidayPeriodFactory.NewHolidayPeriod(holidayDM._startDate, holidayDM._endDate);
        

        Holiday holidayDomain = _holidayFactory.NewHoliday(id,cId,holidayPeriod);
        
        return holidayDomain;
    }
 
    public IEnumerable<Holiday> ToDomain(IEnumerable<HolidayPendingDataModel> holidaysDM)
    {
        List<Holiday> holidaysDomain = new List<Holiday>();

        foreach(HolidayPendingDataModel holidayDataModel in holidaysDM)
        {
            Holiday holidayDomain = ToDomain(holidayDataModel);

            holidaysDomain.Add(holidayDomain);
        }

        return holidaysDomain.AsEnumerable();
    }

    

    public HolidayPendingDataModel ToDataModel(Holiday holiday, ColaboratorsIdDataModel colaboratorsIdDataModel)
    {
        var holidayDataModel = new HolidayPendingDataModel
        {
            Id = holiday.Id,
            colaboratorId = colaboratorsIdDataModel,
            _startDate = holiday.HolidayPeriod.StartDate,
            _endDate = holiday.HolidayPeriod.EndDate
        };

        return holidayDataModel;
    }
   
}