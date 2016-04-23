using System;

namespace Linqua.Persistence.Exceptions
{
    public class NoInternetConnectionException : Exception
    {
        public NoInternetConnectionException()
            : this("Operation cannot complete because there is no connection to the internet.")
        {
        }

        public NoInternetConnectionException(string message) : base(message)
        {
        }

        public NoInternetConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}