using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.Redis
{
    public class RedisApiOutputCache : IApiOutputCache
    {
        private readonly IDatabase _database;
        private readonly IServer _server;
        private readonly JsonSerializerSettings _settings;

        public RedisApiOutputCache(IServer server) : this(server, -1)
        {}

        public RedisApiOutputCache(IServer server, int databaseId)
        {
            _server = server;
            _database = _server.Multiplexer.GetDatabase(databaseId);
            _settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public void RemoveStartsWith(string key)
        {
            _database.KeyDelete(string.Concat(key, "*"));
        }

        public T Get<T>(string key) where T : class
        {
            var item = _database.StringGet(key);

            return item.HasValue ? JsonConvert.DeserializeObject<T>(item, _settings) : null;
        }

        public object Get(string key)
        {
            var item = _database.StringGet(key);

            return item.HasValue ? JsonConvert.DeserializeObject(item, _settings) : null;
        }

        public void Remove(string key)
        {
            _database.KeyDelete(key);
        }

        public bool Contains(string key)
        {
            return _database.KeyExists(key);
        }

        public void Add(string key, object o, DateTimeOffset expiration, string dependsOnKey = null)
        {
            _database.StringSet(key, JsonConvert.SerializeObject(o, _settings), 
                expiration.UtcDateTime - DateTime.UtcNow);
        }

        public IEnumerable<string> AllKeys
        {
            get
            {
                return _server.Keys().Select(x => x.ToString());
            }
        }
    }
}
