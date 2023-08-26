using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client
{
    class MessageCenter
    {
        public static event Action<string> Subscribe;
        public static void Receive(string message)
        {
            Subscribe?.Invoke(message);
        }
    }

}
