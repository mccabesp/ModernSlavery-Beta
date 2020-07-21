using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernSlavery.Core.Classes.ErrorMessages
{
    [Serializable]
    public class Outcome<TFail> 
    {
        public Outcome()
        {
        }

        public Outcome(TFail error, string message=null)
        {
            Errors.Add((error, message));
        }

        public Outcome(List<(TFail Error, string Message)> errors)
        {
            Errors = errors;
        }

        public List<(TFail Error, string Message)> Errors { get; set; } = new List<(TFail, string)>();
        public bool Success => !Fail;
        public bool Fail => Errors != null && Errors.Any();
    }

    [Serializable]
    public class Outcome<TFail, TSuccess>:Outcome<TFail>
    {
        public Outcome(TSuccess result):base()
        {
            Result = result;
        }

        public Outcome(TFail error, string message = null):base(error,message)
        {
        }

        public Outcome(List<(TFail Error, string Message)> errors):base(errors)
        {
            
        }

        public TSuccess Result { get; }
    }
}