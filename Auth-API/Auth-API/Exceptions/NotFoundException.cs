using System.Runtime.Serialization;

namespace Auth_API.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException(string mensagem)
            : base(mensagem)
        {
        }

        protected NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
