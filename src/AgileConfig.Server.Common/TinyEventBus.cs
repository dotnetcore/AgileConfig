using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common
{
    public interface ITinyEventBus
    {
        /// <summary>
        /// 注册一个action到对应的eventkey的队列里
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="action"></param>
        void Register(string eventKey, Action<object> action);
        /// <summary>
        /// 移除这个eventkey对应的所有事件
        /// </summary>
        /// <param name="eventKey"></param>
        void Clear(string eventKey);
        /// <summary>
        /// 从eventkey的事件队列里移除1个action
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="action"></param>
        void Remove(string eventKey, Action<object> action);
        /// <summary>
        /// 根据eventkey找出事件队列，执行队列里所有的事件，事件会被异步执行
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="param"></param>
        void Fire(string eventKey, object param = null);
    }

    public class TinyEventBus : ITinyEventBus
    {
        private static ConcurrentDictionary<string, List<Action<object>>> _events;

        private ILogger _logger = null;
        private TinyEventBus()
        {
            _logger = Global.LoggerFactory.CreateLogger<TinyEventBus>();
            _events = new ConcurrentDictionary<string, List<Action<object>>>();
        }

        public void Register(string eventKey, Action<object> action)
        {
            if (!_events.TryGetValue(eventKey, out List<Action<object>> actions))
            {
                _events.TryAdd(eventKey, new List<Action<object>>());
            }
            if (actions == null)
            {
                _events.TryGetValue(eventKey, out actions);
            }
            ICollection list = actions;
            lock (list.SyncRoot)
            {
                actions.Add(action);
            }
        }


        public void Clear(string eventKey)
        {
            _events.TryRemove(eventKey, out List<Action<object>> actions);
        }


        public void Remove(string eventKey, Action<object> action)
        {
            _events.TryGetValue(eventKey, out List<Action<object>> actions);
            if (actions != null)
            {
                ICollection list = actions;
                lock (list.SyncRoot)
                {
                    actions.Remove(action);
                }
            }
        }


        public void Fire(string eventKey, object param = null)
        {
            _events.TryGetValue(eventKey, out List<Action<object>> actions);
            if (actions != null)
            {
                ICollection list = actions;
                lock (list.SyncRoot)
                {
                    foreach (var act in actions)
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                act(param);
                            }
                            catch (Exception e)
                            {
                                _logger?.LogError(e, $"fire event {eventKey} error");
                            }
                        });
                    }
                }
            }
        }

        private static ITinyEventBus _instance;
        public static ITinyEventBus Instance
        {
            get
            {
                return _instance ?? (_instance = new TinyEventBus());
            }
        }
    }
}
