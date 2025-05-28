using EmpregaNet.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EmpregaNet.Application.Common.Base
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _provider;

        public Mediator(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();
            var responseType = typeof(TResponse);
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handler = _provider.GetRequiredService(handlerType);

            if (handler == null)
                throw new InvalidOperationException($"Handler not found for {request.GetType().Name}");

            var handleMethod = handlerType.GetMethod("Handle");
            if (handleMethod == null)
            {
                throw new InvalidOperationException($"Method 'Handle' not found on handler for {requestType.Name}");
            }

            var pipelineBehaviors = _provider.GetServices(typeof(IPipelineBehavior<,>)
                                                  .MakeGenericType(requestType, responseType))
                                                  .Cast<IPipelineBehavior<IRequest<TResponse>, TResponse>>()
                                                  .ToList();

            RequestHandlerDelegate<TResponse> next = async () =>
            {
                return await (Task<TResponse>)handleMethod.Invoke(handler, new object[] { request, cancellationToken })!;
            };


            foreach (var behavior in pipelineBehaviors.AsEnumerable().Reverse())
            {
                var currentBehavior = behavior;
                next = async () => await currentBehavior.Handle(request, next, cancellationToken);
            }

            return await next();
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
            var handlers = _provider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                await (Task)handlerType
                    .GetMethod("Handle")!
                    .Invoke(handler, new object[] { notification, cancellationToken })!;
            }
        }
    }
}