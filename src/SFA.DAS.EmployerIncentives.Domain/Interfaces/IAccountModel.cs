namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IAccountModel : IEntityModel<long>
    {
        long AccountLegalEntityId { get; set; }
        ILegalEntityModel LegalEntityModel { get; set; }
    }
}
