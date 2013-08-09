using System;
using System.Collections.Generic;
using System.Text;

namespace AndrewTweddle.ORToolkit.DynamicProgramming
{
    public class SolutionNodeStorageTypeException: Exception
    {
        public SolutionNodeStorageTypeException()
            : base()
        {
        }

        public SolutionNodeStorageTypeException(string message)
            : base(message)
        {
        }

        public SolutionNodeStorageTypeException(string message,
            Exception innerException)
            : base(message, innerException)
        {
        }

        public SolutionNodeStorageTypeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
