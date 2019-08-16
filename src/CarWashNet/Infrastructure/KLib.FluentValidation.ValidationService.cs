using FluentValidation;
using FL = FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLib.FluentValidation
{
	public class KlibValidationException : ValidationException
	{
		public KlibValidationException(IEnumerable<ValidationFailure> errors) : base(BuildErrorMesage(errors), errors)
		{

		}

		public string StringError => String.Join(Environment.NewLine, Errors.Select(p => p.ErrorMessage));

		private static string BuildErrorMesage(IEnumerable<ValidationFailure> errors)
		{
			return String.Join(Environment.NewLine, errors.Select(p => p.ErrorMessage));
		}
	}
	public static class ValidationService
	{
		#region Autofind validation
		static ConcurrentDictionary<RuntimeTypeHandle, IValidator> validators = new ConcurrentDictionary<RuntimeTypeHandle, IValidator>();
		static IValidator GetValidator(Type modelType)
		{
			if (!validators.TryGetValue(modelType.TypeHandle, out IValidator validator))
			{
				var typeName = string.Format("{0}.{1}Validator", modelType.Namespace, modelType.Name);
				var type = modelType.Assembly.GetType(typeName, true);
				validators[modelType.TypeHandle] = validator = (IValidator)Activator.CreateInstance(type);
			}
			return validator;
		}
		static IValidator<T> GetValidator<T>()
		{
			return (IValidator<T>)GetValidator(typeof(T));
		}
		public static ValidationResult Validate<T>(this T entity, string validationRuleSet)
		{
			var type = typeof(T);// entity.GetType();
			var validator = GetValidator(type);

			var ruleSetNames = validationRuleSet.Split(',', ';');
			var selector = ValidatorOptions.ValidatorSelectors.RulesetValidatorSelectorFactory(ruleSetNames);
			var context = new ValidationContext(entity, new FL.Internal.PropertyChain(), selector);
			var validationResult = validator.Validate(context);
			return validationResult;
		}
		public static void ValidateAndThrow<T>(this T entity, string validationRuleSet)
		{
			var validationResult = Validate(entity, validationRuleSet);
			if (!validationResult.IsValid)
			{
				throw new KlibValidationException(validationResult.Errors);
			}
		}
		public static ValidationResult Validate<T>(this T entity)
		{
			return entity.Validate("default");
		}
		public static void ValidateAndThrow<T>(this T entity)
		{
			ValidateAndThrow(entity, "default");
		}
		#endregion

		#region Strongtyped validation
		public static ValidationResult Validate<V, T>(this T entity, string validationRuleSet) where V : class, new() where T : class
		{
			var validator = new V() as AbstractValidator<T>;
			var validationResult = DefaultValidatorExtensions.Validate(validator, entity, ruleSet: validationRuleSet);
			return validationResult;
		}
		public static void ValidateAndThrow<V, T>(this T entity, string validationRuleSet) where V : class, new() where T : class
		{
			var validationResult = Validate<V, T>(entity, validationRuleSet);
			if (!validationResult.IsValid)
			{
				throw new KlibValidationException(validationResult.Errors);
			}
		}
		public static ValidationResult Validate<V, T>(this T entity) where V : class, new() where T : class
		{
			return entity.Validate<V, T>("default");
		}
		public static void ValidateAndThrow<V, T>(this T entity) where V : class, new() where T : class
		{
			entity.ValidateAndThrow<V, T>("default");
		}		
		#endregion        
	}
}
