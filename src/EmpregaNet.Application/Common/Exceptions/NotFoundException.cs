namespace Common.Exceptions
{
    /// <summary>
    /// Exception thrown when an entity is not found.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
