﻿using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Abstractions.Commands
{
    public class ValidationResult
    {
        public Dictionary<string, string> ValidationDictionary { get; set; }

        public ValidationResult()
        {
            ValidationDictionary = new Dictionary<string, string>();
        }

        public void AddError(string propertyName)
        {
            ValidationDictionary.Add(propertyName, $"{propertyName} has not been supplied");
        }

        public void AddError(string propertyName, string validationError)
        {
            ValidationDictionary.Add(propertyName, validationError);
        }

        public void AddIsNotSetError(string propertyName)
        {
            ValidationDictionary.Add(propertyName, "Is not set");
        }

        public bool IsValid()
        {
            if (ValidationDictionary == null)
            {
                return false;
            }

            return !ValidationDictionary.Any();
        }
    }
}
