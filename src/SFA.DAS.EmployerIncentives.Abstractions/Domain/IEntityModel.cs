namespace SFA.DAS.EmployerIncentives.Abstractions.Domain
{
    public interface IEntityModel<IdType>
    {
        IdType Id { get; set; }
    }
}
