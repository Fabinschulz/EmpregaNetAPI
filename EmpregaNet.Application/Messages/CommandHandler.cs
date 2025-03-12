using EmpregaNet.Infra.Interface;
using FluentValidation.Results;

namespace EmpregaNet.Application.Messages
{
    public abstract class CommandHandler
    {
        protected ValidationResult ValidationResult;

        protected CommandHandler()
        {
            ValidationResult = new ValidationResult();
        }

        protected void AddError(string message)
        {
            ValidationResult.Errors.Add(new ValidationFailure(string.Empty, message));
        }

        protected async Task<ValidationResult> PersistData(IUnitOfWork uow)
        {
            if (!await uow.Commit()) AddError("Ops, ocorreu um erro ao salvar os dados.");

            return ValidationResult;
        }
    }
}