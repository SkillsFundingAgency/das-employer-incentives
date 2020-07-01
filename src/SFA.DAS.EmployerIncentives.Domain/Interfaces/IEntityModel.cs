namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IEntityModel<IdType>
    {
        IdType Id { get; set; }
    }
}
