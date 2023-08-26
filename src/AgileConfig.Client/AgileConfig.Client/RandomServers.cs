using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgileConfig.Client
{
    public class RandomServers
    {
        private List<string> _serverUrls;
        private List<int> _serverIndexs;
        private int StartIndex = -1;
        public RandomServers(string serverUrls)
        {
            _serverUrls = new List<string>();
            _serverIndexs = new List<int>();
            if (!string.IsNullOrEmpty(serverUrls))
            {
                _serverUrls = serverUrls.Split(',').ToList();
            }
            else
            {
                throw new ArgumentNullException("serverUrls");
            }

            var randomIndex = new Random().Next(0, _serverUrls.Count);
            _serverIndexs.Add(randomIndex);

            int index = randomIndex + 1;
            while (true)
            {
                if (index == randomIndex)
                {
                    break;
                }
                if (index >= _serverUrls.Count)
                {
                    index = index - _serverUrls.Count;
                    continue;
                }
                _serverIndexs.Add(index);
                index++;
            }
        }

        public int ServerCount
        {
            get
            {
                return _serverUrls.Count;
            }
        }

        private int NextIndex
        {
            get
            {
                if (IsComplete)
                {
                    throw new Exception("No next index .");
                }
                StartIndex++;

                return StartIndex;
            }
        }

        public bool IsComplete
        {
            get
            {
                return StartIndex + 1 >= _serverIndexs.Count;
            }
        }

        public string Next()
        {
            return _serverUrls[_serverIndexs[NextIndex]];
        }
    }
}
