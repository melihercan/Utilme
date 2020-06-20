using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Utilme
{
    public class Result<T>
    {
        public T Value { get; }
        public bool IsOk { get; } = true;
        public ResultStatus Status { get; } = ResultStatus.Ok;
        public string Message { get; }

        public Result(T value)
        {
            Value = value;
        }

        private Result(ResultStatus status, string message)
        {
            Status = status;
            if(status != ResultStatus.Ok)
            {
                IsOk = false;
            }
            Message = message ?? string.Empty;
        }

        public static Result<T> Ok(T value)
        {
            return new Result<T>(value);
        }

        public static Result<T> Error(string message = null)
        {
            return new Result<T>(ResultStatus.Error, message);
        }

        public static Result<T> NotFound(string message = null)
        {
            return new Result<T>(ResultStatus.NotFound, message);
        }

        public static Result<T> Timeout(string message = null)
        {
            return new Result<T>(ResultStatus.Timeout, message);
        }

        public static Result<T> Cancelled(string message = null)
        {
            return new Result<T>(ResultStatus.Cancelled, message);
        }
        
        public static Result<T> NotSupported(string message = null)
        {
            return new Result<T>(ResultStatus.NotSupported, message);
        }
        
        public static Result<T> InvalidData(string message = null)
        {
            return new Result<T>(ResultStatus.InvalidData, message);
        }

    }
}
