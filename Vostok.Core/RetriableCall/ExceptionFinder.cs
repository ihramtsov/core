using System;

namespace Vostok.RetriableCall
{
    // todo (andrew, 17.01.2018): test all this staff
    public static class ExceptionFinder
    {
        public static bool HasException<TException>(this Exception rootEx)
            where TException : Exception
        {
            return HasException(rootEx, e => e is TException);
        }

        public static bool HasException(this Exception rootEx, Func<Exception, bool> condition)
        {
            return FindFirstException(rootEx, condition) != null;
        }

        public static TException FindFirstException<TException>(Exception rootEx)
            where TException : Exception
        {
            return FindFirstException(rootEx, e => e is TException) as TException;
        }

        public static Exception FindFirstException(Exception rootEx, Func<Exception, bool> condition)
        {
            var ex = rootEx;
            while (ex != null && !condition(ex))
            {
                if (!(ex is AggregateException aggregateEx))
                    ex = ex.InnerException;
                else
                {
                    foreach (var innerException in aggregateEx.InnerExceptions)
                    {
                        var exInner = FindFirstException(innerException, condition);
                        if (exInner != null)
                            return exInner;
                    }
                    return null;
                }
            }
            return ex;
        }
    }
}