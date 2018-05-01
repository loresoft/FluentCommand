using System;
using System.Threading;
using FluentAssertions;
using FluentCommand.Caching;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.Tests.Caching
{
    public class CacheManagerTest
    {
        private ITestOutputHelper _output;

        public CacheManagerTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AddTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            CachePolicy cachePolicy = null;
            bool isAdded = cacheManager.Add(key, value, cachePolicy);
            Assert.True(isAdded);
            bool doesContain = cacheManager.Contains(key);
            Assert.True(doesContain);
        }

        [Fact]
        public void AddValueNullTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = null;
            CachePolicy cachePolicy = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                bool isAdded = cacheManager.Add(key, value, cachePolicy);
            });

        }

        [Fact]
        public void AddKeyNullTest()
        {
            var cacheManager = new CacheManager();
            string key = null;
            object value = "value" + DateTime.Now.Ticks;
            CachePolicy cachePolicy = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                bool isAdded = cacheManager.Add(key, value, cachePolicy);
            });

        }

        [Fact]
        public void GetTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            CachePolicy cachePolicy = null;
            bool isAdded = cacheManager.Add(key, value, cachePolicy);
            Assert.True(isAdded);
            bool doesContain = cacheManager.Contains(key);
            Assert.True(doesContain);
            var result = cacheManager.Get(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void GetOrAddValueFactoryTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            bool valueCalled = false;

            Func<string, object> valueFactory = k => { valueCalled = true; return value; };
            object result = cacheManager.GetOrAdd(key, valueFactory);

            Assert.NotNull(result);
            Assert.True(valueCalled);
            Assert.Equal(value, result);

            // value factory 2 should not be called
            object value2 = "value2" + DateTime.Now.Ticks;
            bool value2Called = false;
            Func<string, object> valueFactory2 = k => { value2Called = true; return value2; };

            object result2 = cacheManager.GetOrAdd(key, valueFactory2);

            Assert.NotNull(result2);
            Assert.False(value2Called);
            // result should still = first call
            Assert.Equal(value, result2);
        }

        [Fact]
        public void RemoveTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            cacheManager.Add(key, value);
            Assert.Equal(1, cacheManager.Count);
            var result = cacheManager.Remove(key);
            Assert.Equal(value, result);
            Assert.Equal(0, cacheManager.Count);
        }

        [Fact]
        public void SetTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            cacheManager.Set(key, value);
            Assert.Equal(1, cacheManager.Count);
        }

        [Fact]
        public void ItemTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            cacheManager[key] = value;
            object actual = cacheManager[key];
            Assert.Equal(value, actual);
        }

        [Fact]
        public void ExpireItemTest()
        {
            var expiredHandle = new AutoResetEvent(false);
            var expirationTime = TimeSpan.FromSeconds(1);
            var cacheManager = new CacheManager(expirationTime);

            cacheManager.CacheItemExpired += (sender, args) =>
            {
                _output.WriteLine($"Item Expired: {args.CacheItem.Key}");
                expiredHandle.Set();
            };

            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            var expiration = TimeSpan.FromMilliseconds(800);

            var cachePolicy = new CachePolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration) };

            bool isAdded = cacheManager.Add(key, value, cachePolicy);
            isAdded.Should().BeTrue();
            bool contains = cacheManager.Contains(key);
            contains.Should().BeTrue();

            var cachedValue = cacheManager.Get(key);
            cachedValue.Should().NotBeNull();

            // waited for exiration timer
            var signalReceived = expiredHandle.WaitOne(TimeSpan.FromSeconds(5));
            signalReceived.Should().BeTrue();

            contains = cacheManager.Contains(key);
            contains.Should().BeFalse();
            cacheManager.Count.Should().Be(0);

            cachedValue = cacheManager.Get(key);
            cachedValue.Should().BeNull();
            
            expiredHandle.Reset();

            cachePolicy = new CachePolicy { SlidingExpiration = expiration };
            isAdded = cacheManager.Add(key, value, cachePolicy);
            isAdded.Should().BeTrue();
            contains = cacheManager.Contains(key);
            contains.Should().BeTrue();

            cachedValue = cacheManager.Get(key);
            cachedValue.Should().NotBeNull();


            // waited for exiration timer
            signalReceived = expiredHandle.WaitOne(TimeSpan.FromSeconds(5));
            signalReceived.Should().BeTrue();

            contains = cacheManager.Contains(key);
            contains.Should().BeFalse();
            cacheManager.Count.Should().Be(0);

            cachedValue = cacheManager.Get(key);
            cachedValue.Should().BeNull();
        }

        [Fact]
        public void ExpireGetTest()
        {
            var expiredHandle = new AutoResetEvent(false);
            var cacheManager = new CacheManager();

            cacheManager.CacheItemExpired += (sender, args) =>
            {
                _output.WriteLine($"Item Expired: {args.CacheItem.Key}");
                expiredHandle.Set();
            };

            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            var expiration = TimeSpan.FromMilliseconds(100);

            var cachePolicy = new CachePolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration) };

            bool isAdded = cacheManager.Add(key, value, cachePolicy);
            isAdded.Should().BeTrue();
            bool contains = cacheManager.Contains(key);
            contains.Should().BeTrue();

            var cachedValue = cacheManager.Get(key);
            cachedValue.Should().NotBeNull();

            Thread.Sleep(500);

            cachedValue = cacheManager.Get(key);
            cachedValue.Should().BeNull();
        }

        [Fact]
        public void UpdatePolicyTest()
        {
            var cacheManager = new CacheManager();
            string key = "key" + DateTime.Now.Ticks;
            object value = "value" + DateTime.Now.Ticks;
            TimeSpan expiration = TimeSpan.FromSeconds(2);

            DateTimeOffset absoluteExpiration = DateTimeOffset.Now.Add(expiration);

            var cachePolicy = new CachePolicy { AbsoluteExpiration = absoluteExpiration };

            bool isAdded = cacheManager.Add(key, value, cachePolicy);
            Assert.True(isAdded);
            bool contains = cacheManager.Contains(key);
            Assert.True(contains);

            var cacheItem = cacheManager.GetCacheItem(key);
            Assert.NotNull(cacheItem);
            Assert.Equal(absoluteExpiration, cacheItem.AbsoluteExpiration);

            DateTimeOffset newExpiration = DateTimeOffset.Now.AddMinutes(20);
            var newPolicy = new CachePolicy { AbsoluteExpiration = newExpiration };

            cacheManager.Set(key, value, newPolicy);

            var newItem = cacheManager.GetCacheItem(key);

            Assert.NotNull(newItem);
            Assert.Equal(newExpiration, newItem.AbsoluteExpiration);


        }

    }
}
