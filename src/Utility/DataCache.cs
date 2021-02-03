using System;
using Microsoft.Extensions.Caching.Memory;

namespace Utility
{
    public class MemoryCacheHelper
    {
        public MemoryCacheHelper(/*MemoryCacheOptions options*/)//这里可以做成依赖注入，但没打算做成通用类库，所以直接把选项直接封在帮助类里边
        {
            //this._cache = new MemoryCache(options);
            this._cache = new MemoryCache(new MemoryCacheOptions());
        }

        private IMemoryCache _cache;

        public bool Exists(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return this._cache.TryGetValue<object>(key, out _);
        }

        public T GetCache<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this._cache.TryGetValue<T>(key, out var v);

            return v;
        }

        public void SetCache(string key, object value)
        {
            var cfg = new MemoryCacheEntryOptions()
            {
                // AbsoluteExpirationRelativeToNow = ts,
            };
            this.SetCache(key, value, null);
        }

        public void SetCache(string key, object value, double expirationMinute)
        {

            var now = DateTime.Now;
            var ts = now.AddMinutes(expirationMinute) - now;
            var cfg = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = ts,
            };
            this.SetCache(key, value, cfg);
        }

        public void SetCache(string key, object value, DateTimeOffset expirationTime)
        {
            var cfg = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = expirationTime,
            };
            this.SetCache(key, value, cfg);
        }
        public void SetCache(string key, object value, DateTime time)
        {
            var ts = time - DateTime.Now;
            if (ts.TotalSeconds < 0)
            {
                throw new ArgumentException(nameof(time) + "参数不能小于当前时间", nameof(time), null);
            }
            var cfg = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = ts,
            };
            this.SetCache(key, value, cfg);
        }
        public void SetCache(string key, object value, MemoryCacheEntryOptions options)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            object v = null;
            if (this._cache.TryGetValue(key, out v))
            {
                this._cache.Remove(key);
            }

            this._cache.Set<object>(key, value, options);
        }
        public void RemoveCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this._cache.Remove(key);
        }

        public void Dispose()
        {
            if (_cache != null)
            {
                _cache.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 缓存相关的操作类
    /// Copyright (C) Maticsoft
    /// </summary>
    public class DataCache
    {
        private static MemoryCacheHelper _localCacheHelper = new MemoryCacheHelper();
        /// <summary>
        /// 获取当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns></returns>
        public static object GetCache(string CacheKey)
        {


            //System.Web.Caching.Cache objCache = HttpRuntime;
            return _localCacheHelper.GetCache<object>(CacheKey);
        }


        /// <summary>
        /// 删除cache
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns></returns>
        public static void Remove(string key)
        {
            _localCacheHelper.RemoveCache(key);
        }

        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        public static void SetCache(string cacheKey, object objObject)
        {
            if (objObject == null)
            {
                return;
            }

            _localCacheHelper.SetCache(cacheKey, objObject);
        }

        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        public static void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            if (objObject == null)
            {
                return;
            }
            var ts = absoluteExpiration - DateTime.Now;
            if (ts.TotalSeconds < 0)
            {
                throw new ArgumentException(nameof(absoluteExpiration) + "参数不能小于当前时间", nameof(absoluteExpiration), null);
            }
            var cfg = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = ts,
                SlidingExpiration = slidingExpiration
            };
            _localCacheHelper.SetCache(cacheKey, objObject, cfg);
        }

        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <param name="objObject"></param>
        public static void SetCache(string cacheKey, object objObject, DateTime absoluteExpiration)
        {
            if (objObject == null)
            {
                return;
            }

            _localCacheHelper.SetCache(cacheKey, objObject, absoluteExpiration);
        }

       
    }
}