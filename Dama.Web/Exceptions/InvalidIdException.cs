using System;

namespace Dama.Web.Exceptions
{
    public class InvalidIdException : Exception
    {
        public InvalidIdException(string message)
            :base(message)
        {
        }
    }
}