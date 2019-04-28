using System;

namespace SharpBCH.SLP
{
    public class InvalidSLPScriptException : Exception
    {
        public InvalidSLPScriptException(string message) : base(message)
        {
        }
    }
}