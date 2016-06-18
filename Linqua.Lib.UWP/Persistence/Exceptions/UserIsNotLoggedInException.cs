using System;

namespace Linqua.Persistence.Exceptions
{
    public class UserIsNotLoggedInException : Exception
    {
        public UserIsNotLoggedInException()
        {
        }

        public UserIsNotLoggedInException(string message) : base(message)
        {
        }

        public UserIsNotLoggedInException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}