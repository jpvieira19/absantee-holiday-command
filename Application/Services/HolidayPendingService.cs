namespace Application.Services;

using Domain.Model;
using Application.DTO;

using Microsoft.EntityFrameworkCore;
using DataModel.Repository;
using Domain.IRepository;
using Domain.Factory;
using DataModel.Model;
using Gateway;

public class HolidayPendingService {

    private readonly AbsanteeContext _context;
    private readonly IHolidayPendingRepository _holidayPendingRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IColaboratorsIdRepository _colaboratorsIdRepository;
    private readonly IHolidayPeriodFactory _holidayPeriodFactory;
    private readonly HolidayPendentAmqpGateway _holidayPendentAmqpGateway;


    
    public HolidayPendingService(IHolidayPendingRepository holidayPendingRepository, IHolidayPeriodFactory holidayPeriodFactory, HolidayPendentAmqpGateway holidayPendentAmqpGateway,IColaboratorsIdRepository colaboratorsIdRepository, IHolidayRepository holidayRepository) {
        _holidayPendingRepository = holidayPendingRepository;
        _holidayPeriodFactory = holidayPeriodFactory;
        _holidayPendentAmqpGateway=holidayPendentAmqpGateway;
        _colaboratorsIdRepository = colaboratorsIdRepository;
        _holidayRepository = holidayRepository;
    }    

    public async Task<HolidayDTO> Add(HolidayDTO holidayDto, List<string> errorMessages)
    {
        bool bExists = await _holidayPendingRepository.HolidayExists(holidayDto.Id);
        bool colabExists = await _colaboratorsIdRepository.ColaboratorExists(holidayDto._colabId);
        bool doesColabHasHolidayInPeriod = await _holidayRepository.DoesColabHasHolidayInPeriod(holidayDto._colabId,holidayDto._holidayPeriod.StartDate,holidayDto._holidayPeriod.EndDate);
        if(bExists) {
            errorMessages.Add("Holiday already exists.");
            return null;
        }
        if(!colabExists) {
            errorMessages.Add("Colab doesn't exist.");
            return null;
        }
        if(doesColabHasHolidayInPeriod) {
            errorMessages.Add("Colab already has a holiday in that Period.");
            return null;
        }
        try{
        Holiday holiday = HolidayDTO.ToDomain(holidayDto);

        holiday = await _holidayPendingRepository.AddHoliday(holiday);

        HolidayDTO holidayDTO = HolidayDTO.ToDTO(holiday);

        string holidayAmqpDTO = HolidayGatewayDTO.Serialize(holidayDTO);	
        _holidayPendentAmqpGateway.PublishNewHolidayPending(holidayAmqpDTO);

        return holidayDTO;
        }catch(ArgumentException ex){
               errorMessages.Add(ex.Message);
            return null;
        }
    }

    public async Task<bool> Remove(long holidayId)
        {
            // Verifica se a Holiday Pendente existe
            bool exists = await _holidayPendingRepository.HolidayExists(holidayId);
            if (!exists)
            {
                // Se não existir, retorna false indicando falha na remoção
                return false;
            }

            // Remove a Holiday Pendente pelo seu ID
            await _holidayPendingRepository.RemoveHoliday(holidayId);

            // Retorna true indicando que a remoção foi bem-sucedida
            return true;
        }

    

    
}