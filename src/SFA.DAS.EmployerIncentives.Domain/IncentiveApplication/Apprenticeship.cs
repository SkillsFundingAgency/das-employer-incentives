﻿using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplication.Models;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplication
{
    public class Apprenticeship : Entity<Guid, ApprenticeshipModel>
    {
        public static Apprenticeship Create(ApprenticeshipModel model)
        {
            return new Apprenticeship(model.Id, model, false);
        }

        public Apprenticeship(Guid id, ApprenticeshipModel model, bool isNew) : base(id, model, isNew)
        {
        }
    }
}
