using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using StackExchange.Redis;

namespace WebApi.OutputCache.Redis.Tests
{
    [TestFixture]
    public class RedisApiOutputCacheTests
    {
        private IConnectionMultiplexer _connection;
        private RedisApiOutputCache _outputCache;
        private IDatabase _database;
        private const string Key1 = "key1";
        private const string Key2 = "key2";
        private const string Key3 = "otherkey";
        private const string Value1 = "{id:1, listOfStuff:[1,2]}";
        private const string Value2 = "value2";
        private const string Value3 = "othervalue";

        [SetUp]
        public void SetUp()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["redis"].ConnectionString;
            _connection = ConnectionMultiplexer.Connect(connectionString);
            _database = _connection.GetDatabase();
            _outputCache = new RedisApiOutputCache(_connection);

            var server = _connection.GetServer(_connection.GetEndPoints().First());
            server.FlushDatabase();
        }

        [Test]
        public void ShouldRemoveKeysStartingWithString()
        {
            _database.StringSet(Key1, Value1);
            _database.StringSet(Key2, Value2);
            _database.StringSet(Key3, Value3);

            _outputCache.RemoveStartsWith("key");

            Assert.That(string.IsNullOrWhiteSpace(_database.StringGet(Key1)), Is.True);
            Assert.That(string.IsNullOrWhiteSpace(_database.StringGet(Key2)), Is.True);
            Assert.That(_database.StringGet(Key3), Is.Not.Null);

        }

        [Test]
        public void ShouldGetGenericItemFromCache()
        {
            _database.StringSet(Key1, Value1);

            var item = _outputCache.Get<Fixture1>(Key1);

            Assert.That(item, Is.Not.Null);
        }

        [Test]
        public void ShouldGetObjectFromCache()
        {
            _database.StringSet(Key1, Value1);

            var item = _outputCache.Get(Key1);

            Assert.That(item, Is.Not.Null);
        }

        [Test]
        public void ShouldRemoveItemFromCache()
        {
            _database.StringSet(Key1, Value1);

            _outputCache.Remove(Key1);

            Assert.That(string.IsNullOrWhiteSpace(_database.StringGet(Key1)), Is.True);
        }

        [Test]
        public void ShouldReturnTrueWhenItemExistsInCache()
        {
            _database.StringSet(Key1, Value1);

            var exists = _outputCache.Contains(Key1);

            Assert.IsTrue(exists);
        }

        [Test]
        public void ShouldReturnFalseWhenItemDoesNotExistInCache()
        {
            _database.StringSet(Key1, Value1);

            var exists = _outputCache.Contains(Key2);

            Assert.IsFalse(exists);
        }

        [Test]
        public void ShouldAddItemWithExpiry()
        {
            _outputCache.Add(Key1, Value1, DateTimeOffset.UtcNow.AddSeconds(2));
            Thread.Sleep(2100);
            var exists = _outputCache.Contains(Key1);

            Assert.IsFalse(exists);
        }

        [Test]
        public void ShouldGetAllKeys()
        {
            _database.StringSet(Key1, Value1);
            _database.StringSet(Key2, Value2);
            _database.StringSet(Key3, Value3);

            var keys = _outputCache.AllKeys;

            var enumerable = keys as string[] ?? keys.ToArray();
            Assert.IsTrue(enumerable.Contains(Key1));
            Assert.IsTrue(enumerable.Contains(Key2));
            Assert.IsTrue(enumerable.Contains(Key3));
        }
    }
}
