using Application.DTO;
using DataModel.Repository;
using Domain.IRepository;
using Domain.Model;

namespace Application.Services
{
    public class AssociationService
    {

        private readonly IHolidayRepository _holidayRepository;
        private AssociationVerificationAmqpGateway _associationAmqpGateway;

        public AssociationService(IHolidayRepository holidayRepository, AssociationVerificationAmqpGateway associationVerificationAmqpGateway)
        {
            _holidayRepository = holidayRepository;
            _associationAmqpGateway = associationVerificationAmqpGateway;
        }

        public async Task<AssociationAmqpDTO> Validations(AssociationAmqpDTO associationAmqpDTO)
        {
            bool doesColabHasHolidaysInPeriod = await DoesColabHasHolidaysInPeriod(associationAmqpDTO.ColaboratorId, associationAmqpDTO.StartDate, associationAmqpDTO.EndDate);
           
                    if (doesColabHasHolidaysInPeriod)
                    {
                        associationAmqpDTO.Status = "Not Ok";
                        string stringholidayAmqpDTO = AssociationAmqpDTO.Serialize(associationAmqpDTO);
                        _associationAmqpGateway.Publish(stringholidayAmqpDTO);
                    }
                    else
                    {
                        associationAmqpDTO.Status = "Ok";
                        string stringholidayAmqpDTO = AssociationAmqpDTO.Serialize(associationAmqpDTO);
                        _associationAmqpGateway.Publish(stringholidayAmqpDTO);
                    }
                    return null;
        }

        

        public async Task<bool> DoesColabHasHolidaysInPeriod(long colabId, DateOnly startDate, DateOnly endDate)
        {
            IEnumerable<Holiday> holidays = await _holidayRepository.getHolidaysByColabIdInPeriod(colabId, startDate, endDate);
            if (holidays.Count() > 0)
            {
                return true;
            }
            else{
                return false;
            }
        }
    }
}