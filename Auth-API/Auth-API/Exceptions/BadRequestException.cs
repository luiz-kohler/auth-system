using System.Runtime.Serialization;

namespace Auth_API.Exceptions
{
    [Serializable]
    public class BadRequestException : Exception
    {
        public BadRequestException(string mensagem)
            : base(mensagem)
        {
        }

        protected BadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
