using EmpregaNet.Application.Messages;
using FluentValidation.Results;

namespace EmpregaNet.Application.Mediator
{
    public interface IMediatorHandler
    {
        Task PublishEvent<T>(T evento) where T : Event;
        Task<ValidationResult> SendCommand<T>(T comando) where T : Command;
    }
}