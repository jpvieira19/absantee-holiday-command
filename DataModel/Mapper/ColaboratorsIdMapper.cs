namespace DataModel.Mapper;

using DataModel.Model;

using Domain.Model;
using Domain.Factory;

public class ColaboratorsIdMapper
{
    private IColaboratorIdFactory _colaboratorIdFactory;

    public ColaboratorsIdMapper(IColaboratorIdFactory colaboratorIdFactory)
    {
        _colaboratorIdFactory = colaboratorIdFactory;
    }

    public ColaboratorId ToDomain(ColaboratorsIdDataModel colabsIdDM)
    {
        ColaboratorId colaboratorIdDomain = _colaboratorIdFactory.NewColaboratorId(colabsIdDM.Id);

        colaboratorIdDomain.colabId = colabsIdDM.Id;

        return colaboratorIdDomain;
    }

    public IEnumerable<ColaboratorId> ToDomain(IEnumerable<ColaboratorsIdDataModel> colaboratorsIdDataModel)
    {

        List<ColaboratorId> colaboratorsIdDomain = new List<ColaboratorId>();

        foreach(ColaboratorsIdDataModel colaboratorIdDataModel in colaboratorsIdDataModel)
        {
            ColaboratorId colaboratorIdDomain = ToDomain(colaboratorIdDataModel);

            colaboratorsIdDomain.Add(colaboratorIdDomain);
        }

        return colaboratorsIdDomain.AsEnumerable();
    }

    public ColaboratorsIdDataModel ToDataModel(ColaboratorId colaboratorId)
    {
        ColaboratorsIdDataModel colaboratorsIdDataModel = new ColaboratorsIdDataModel(colaboratorId);

        return colaboratorsIdDataModel;
    }

}