using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common
{
    public class FunctionUtil
    {
        /// <summary>
        /// 尝试运行一个Func几次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        public static T TRY<T>(Func<T> func, int tryCount)
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
        /// 尝试执行一个异步方法多次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
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
        ///  尝试执行一个异步方法多次 , 直接吞掉异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        public static async Task<T> EATAsync<T>(Func<Task<T>> func, int tryCount)
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
                }
            }

            return result;
        }

        /// <summary>
        /// 尝试运行一个Action几次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        public static void TRY(Action act, int tryCount)
        {
            if (act == null)
            {
                throw new ArgumentNullException("act");
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
