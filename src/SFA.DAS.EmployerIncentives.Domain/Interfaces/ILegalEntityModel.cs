namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface ILegalEntityModel : IEntityModel<long>
    {
        string Name { get; set; }
    }
}
