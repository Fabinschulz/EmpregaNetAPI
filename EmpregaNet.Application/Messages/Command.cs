using FluentValidation.Results;
using MediatR;

namespace EmpregaNet.Application.Messages
{
    public abstract class Command : Message, IRequest<ValidationResult>
    {
        public DateTime Timestamp { get; private set; }
        public required ValidationResult ValidationResult { get; set; }

        protected Command()
        {
            Timestamp = DateTime.Now;
        }

        public virtual bool IsValid()
        {
            throw new NotImplementedException();
        }
    }
}