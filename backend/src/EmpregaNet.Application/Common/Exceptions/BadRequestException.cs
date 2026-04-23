namespace EmpregaNet.Application.Common.Exceptions
{
    /// <summary>
    /// Represents errors that occur during application execution due to a bad request.
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
            Errors = new string[] { message };
        }

        public BadRequestException(string[] errors) : base("Multiple errors ocurred. See the Errors property for details.")
        {
            Errors = errors;
        }

        public string[] Errors { get; }
    }
}
