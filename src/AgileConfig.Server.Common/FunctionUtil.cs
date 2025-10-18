using System;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common
{
    public static class FunctionUtil
    {
        /// <summary>
        /// Try executing a function multiple times.
        /// </summary>
        /// <typeparam name="T">Return type of the function.</typeparam>
        /// <param name="func">Delegate to invoke.</param>
        /// <param name="tryCount">Number of attempts before giving up.</param>
        /// <returns>Result produced by the function.</returns>
        public static T TRY<T>(Func<T> func, int tryCount)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    result = func();
                    break;
                }
                catch
                {
                    if (i == (tryCount - 1))
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Try executing an asynchronous function multiple times.
        /// </summary>
        /// <typeparam name="T">Return type of the function.</typeparam>
        /// <param name="func">Asynchronous delegate to invoke.</param>
        /// <param name="tryCount">Number of attempts before rethrowing the exception.</param>
        /// <returns>Result produced by the asynchronous function.</returns>
        public static async Task<T> TRYAsync<T>(Func<Task<T>> func, int tryCount)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            T result = default(T);
            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    result = await func();
                    break;
                }
                catch 
                {
                    if (i == (tryCount - 1))
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Try executing an asynchronous function multiple times and swallow exceptions.
        /// </summary>
        /// <typeparam name="T">Return type of the function.</typeparam>
        /// <param name="func">Asynchronous delegate to invoke.</param>
        /// <param name="tryCount">Number of attempts to run the delegate.</param>
        /// <returns>Result produced by the asynchronous function, or default when all attempts fail.</returns>
        public static async Task<T> EATAsync<T>(Func<Task<T>> func, int tryCount)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            T result = default(T);
            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    result = await func();
                    break;
                }
                catch 
                {
                }
            }

            return result;
        }

        /// <summary>
        /// Try executing an action multiple times.
        /// </summary>
        /// <param name="act">Action to invoke.</param>
        /// <param name="tryCount">Number of attempts before rethrowing the exception.</param>
        /// <returns>Nothing.</returns>
        public static void TRY(Action act, int tryCount)
        {
            if (act == null)
            {
                throw new ArgumentNullException(nameof(act));
            }

            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    act();
                    break;
                }
                catch 
                {
                    if (i == (tryCount - 1))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
