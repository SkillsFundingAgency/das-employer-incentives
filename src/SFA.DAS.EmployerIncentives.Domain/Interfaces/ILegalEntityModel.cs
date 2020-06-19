namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface ILegalEntityModel : IEntityModel<long>
    {
        long AccountLegalEntityId { get; set; }
        string Name { get; set; }
    }
}
