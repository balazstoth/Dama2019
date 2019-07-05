namespace Dama.Web.Exception
{
    public class InvalidIdException : System.Exception
    {
        public InvalidIdException(string message)
            :base(message)
        {
        }
    }
}