using CarWashNet.Domain.Repository;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarWashNet.Domain.Validation
{   
    public class CarWashValidationException : ValidationException
    {
        public CarWashValidationException(string message) : base(message) {  }
        public CarWashValidationException(string message, IEnumerable<ValidationFailure> errors) : base(message, errors) { }
        public CarWashValidationException(IEnumerable<ValidationFailure> errors) : base(BuildErrorMessage(errors), errors) { }

        private static string BuildErrorMessage(IEnumerable<ValidationFailure> errors)
        {
            var arr = errors.Select(x => $"-- {x.ErrorMessage} {Environment.NewLine}");
            return string.Join(string.Empty, arr);
        }
    }

    public static class ValidationService
    {
        #region Strongtyped validation
        public static ValidationResult Validate<V, T>(this T entity, string validationRuleSet, CarWashDb db) where V : class, new() where T : class
        {
            var validator = new V() as DbValidator<T>;
            validator.SetDb(db);
            var validationResult = DefaultValidatorExtensions.Validate(validator, entity, ruleSet: validationRuleSet);
            return validationResult;
        }
        public static void ValidateAndThrow<V, T>(this T entity, string validationRuleSet, CarWashDb db) where V : class, new() where T : class
        {
            var validationResult = Validate<V, T>(entity, validationRuleSet, db);
            if (!validationResult.IsValid)
            {
                throw new CarWashValidationException(validationResult.Errors);
            }
        }
        public static ValidationResult Validate<V, T>(this T entity, CarWashDb db) where V : class, new() where T : class
        {
            return entity.Validate<V, T>("default", db);
        }
        public static void ValidateAndThrow<V, T>(this T entity, CarWashDb db) where V : class, new() where T : class
        {
            entity.ValidateAndThrow<V, T>("default", db);
        }
        #endregion
    }
}
