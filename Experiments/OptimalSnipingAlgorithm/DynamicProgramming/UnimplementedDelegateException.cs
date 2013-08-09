using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public class UnimplementedDelegateException: Exception
    {
        public UnimplementedDelegateException()
            : base()
        {
        }

        public UnimplementedDelegateException(string message)
            : base(message)
        {
        }

        public UnimplementedDelegateException(string message, 
            Exception innerException)
            : base(message, innerException)
        {
        }

        public UnimplementedDelegateException(
            SerializationInfo info, StreamingContext context
            )
            : base(info, context )
        {
        }
    }
}
