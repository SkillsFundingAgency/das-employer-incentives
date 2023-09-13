using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Exceptions
{
    [Serializable]
    public class ValidatePendingPaymentException : Exception
    {
        public Guid ApprenticeshipIncentiveId { get; private set; }
        public Guid PendingPaymentId { get; private set; }

        public ValidatePendingPaymentException(Guid apprenticeshipIncentiveId, Guid pendingPaymentId, Exception innerException)
            : base($"failed to validate ApprenticeshipIncentiveId : {apprenticeshipIncentiveId}, PendingPaymentId : {pendingPaymentId}, Message : {innerException.Message} ", innerException)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
        }

        protected ValidatePendingPaymentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ApprenticeshipIncentiveId = new Guid(info.GetString(nameof(ApprenticeshipIncentiveId)));
            PendingPaymentId = new Guid(info.GetString(nameof(PendingPaymentId)));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(ApprenticeshipIncentiveId), ApprenticeshipIncentiveId.ToString());
            info.AddValue(nameof(PendingPaymentId), PendingPaymentId.ToString());
            base.GetObjectData(info, context);
        }
    }
}
