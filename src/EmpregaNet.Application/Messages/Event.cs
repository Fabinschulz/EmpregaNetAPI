using EmpregaNet.Domain.Common;

namespace EmpregaNet.Application.Messages
{
    public class Event : Message, INotification
    {
        public DateTimeOffset Timestamp { get; private set; }

        protected Event()
        {
            Timestamp = DateTimeOffset.Now;
        }
    }

    public class EntityEvent<TEntity> : Event
       where TEntity : BaseEntity
    {
        public TEntity Entity { get; protected set; }
        public long EntityId => Entity.Id;

        public EntityEvent(TEntity entity)
        {
            Entity = entity;
            AggregateId = entity.Id;
        }
    }
}