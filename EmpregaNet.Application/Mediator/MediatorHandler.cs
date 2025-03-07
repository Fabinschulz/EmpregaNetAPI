
using EmpregaNet.Application.Messages;
using FluentValidation.Results;
using MediatR;

namespace EmpregaNet.Application.Mediator
{
    public class MediatorHandler : IMediatorHandler
    {
        private readonly IMediator _mediator;

        public MediatorHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task PublishEvent<T>(T evento) where T : Event
        {
            await _mediator.Publish(evento);
        }

        public async Task<ValidationResult> SendCommand<T>(T request) where T : Command
        {
            return await _mediator.Send(request);
        }

    }
}