using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FluentAssertions;
using FluentCommand.Caching;
using Xunit;

namespace FluentCommand.Tests.Caching
{
    public class CacheItemTest
    {
        [Fact]
        public void CacheItemConstructorTest()
        {
            string key = "key";
            object value = "value";
            var cachePolicy = new CachePolicy();
            var cacheItem = new CacheItem(key, value, cachePolicy);

            cacheItem.Should().NotBeNull();
            cacheItem.Key.Should().Be("key");
            cacheItem.Value.Should().Be("value");

            cacheItem.CachePolicy.SlidingExpiration.Should().Be(CacheManager.NoSlidingExpiration);
            cacheItem.CachePolicy.AbsoluteExpiration.Should().Be(CacheManager.InfiniteAbsoluteExpiration);
        }

        [Fact]
        public void CacheItemConstructorNullKeyTest()
        {
            string key = null;
            object value = "value";

            Assert.Throws<ArgumentNullException>(() =>
            {
                var cacheItem = new CacheItem(key, value);
            });
        }

        [Fact]
        public void CacheItemConstructorNullValueTest()
        {
            string key = "key";
            object value = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                var cacheItem = new CacheItem(key, value);
            });
        }


        [Fact]
        public void HasExpirationTest()
        {
            string key = "key";
            object value = "value";
            var cachePolicy = new CachePolicy { SlidingExpiration = TimeSpan.FromSeconds(30) };
            var cacheItem = new CacheItem(key, value, cachePolicy);
            bool hasExpiration = cacheItem.CanExpire();
            Assert.True(hasExpiration);

            cachePolicy = new CachePolicy();
            cacheItem = new CacheItem(key, value, cachePolicy);
            hasExpiration = cacheItem.CanExpire();
            Assert.False(hasExpiration);

            cachePolicy = new CachePolicy { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) };
            cacheItem = new CacheItem(key, value, cachePolicy);
            hasExpiration = cacheItem.CanExpire();
            Assert.True(hasExpiration);
        }

        [Fact]
        public void IsExpiredTest()
        {
            string key = "key";
            object value = "value";
            var cachePolicy = new CachePolicy { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) };
            var cacheItem = new CacheItem(key, value, cachePolicy);
            bool hasExpiration = cacheItem.CanExpire();
            Assert.True(hasExpiration);
            bool isExpired = cacheItem.IsExpired();
            Assert.False(isExpired);

            TimeSpan expiration = TimeSpan.FromSeconds(2);

            cachePolicy = new CachePolicy { SlidingExpiration = expiration };
            cacheItem = new CacheItem(key, value, cachePolicy);
            hasExpiration = cacheItem.CanExpire();
            Assert.True(hasExpiration);
            isExpired = cacheItem.IsExpired();
            Assert.False(isExpired);

            Thread.Sleep(expiration);
            Thread.Sleep(100);
            isExpired = cacheItem.IsExpired();
            Assert.True(isExpired);

            cachePolicy = new CachePolicy { AbsoluteExpiration = DateTimeOffset.Now.Add(expiration) };
            cacheItem = new CacheItem(key, value, cachePolicy);
            hasExpiration = cacheItem.CanExpire();
            Assert.True(hasExpiration);
            isExpired = cacheItem.IsExpired();
            Assert.False(isExpired);

            Thread.Sleep(expiration);
            Thread.Sleep(100);
            isExpired = cacheItem.IsExpired();
            Assert.True(isExpired);
        }

        [Fact]
        public void RaiseExpiredCallbackTest()
        {
            string key = "key";
            object value = "value";
            TimeSpan expiration = TimeSpan.FromSeconds(2);
            bool expireCalled = false;

            var cachePolicy = new CachePolicy
            {
                SlidingExpiration = expiration,
                ExpiredCallback = e => { expireCalled = true; }
            };
            var cacheItem = new CacheItem(key, value, cachePolicy);
            bool hasExpiration = cacheItem.CanExpire();
            Assert.True(hasExpiration);
            bool isExpired = cacheItem.IsExpired();
            Assert.False(isExpired);

            Thread.Sleep(expiration);
            isExpired = cacheItem.IsExpired();
            Assert.True(isExpired);

            cacheItem.RaiseExpiredCallback();
            Thread.Sleep(expiration);

            Assert.True(expireCalled);

        }

        [Fact]
        public void UpdateUsageTest()
        {
            string key = "key";
            object value = "value";
            TimeSpan expiration = TimeSpan.FromSeconds(2);

            var cachePolicy = new CachePolicy { SlidingExpiration = expiration };
            var cacheItem = new CacheItem(key, value, cachePolicy);

            bool hasExpiration = cacheItem.CanExpire();
            Assert.True(hasExpiration);
            bool isExpired = cacheItem.IsExpired();
            Assert.False(isExpired);

            Thread.Sleep(TimeSpan.FromSeconds(1));
            cacheItem.UpdateUsage();

            isExpired = cacheItem.IsExpired();
            Assert.False(isExpired);

            Thread.Sleep(expiration);
            isExpired = cacheItem.IsExpired();
            Assert.True(isExpired);

            cacheItem.UpdateUsage();
            isExpired = cacheItem.IsExpired();
            Assert.False(isExpired);
        }
    }
}
