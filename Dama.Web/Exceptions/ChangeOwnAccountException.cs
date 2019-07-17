using System;

namespace Dama.Web.Exceptions
{
    public class ChangeOwnAccountException : Exception
    {
        public ChangeOwnAccountException(string message)
            : base(message)
        {
        }
    }
}