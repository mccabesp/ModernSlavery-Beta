﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;
using Newtonsoft.Json;

namespace ModernSlavery.WebUI.Shared.Classes
{
    public class HttpSession : IHttpSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool Dirty;

        public HttpSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string SessionID => _httpContextAccessor.HttpContext.Session.Id;

        public async Task LoadAsync()
        {
            //Load data from distributed data store asynchronously
            if (!_httpContextAccessor.HttpContext.Session.IsAvailable)
            {
                await _httpContextAccessor.HttpContext.Session.LoadAsync();
                Dirty = false;
            }
        }

        public async Task SaveAsync()
        {
            if (Dirty) await _httpContextAccessor.HttpContext.Session.CommitAsync();

            Dirty = false;
        }

        public object this[string key]
        {
            get => Get<string>(key);
            set
            {
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

                if (value == null)
                    Remove(key);
                else
                    Add(key, value);
            }
        }

        public IEnumerable<string> Keys => _httpContextAccessor.HttpContext.Session.Keys;

        public T Get<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            //Get value from session
            var bytes = _httpContextAccessor.HttpContext.Session.Get(key);
            if (bytes == null || bytes.Length == 0) return default;

            bytes = Encryption.Decompress(bytes);

            if (bytes == null || bytes.Length == 0) return default;

            var value = Encoding.UTF8.GetString(bytes);

            if (string.IsNullOrWhiteSpace(value)) return default;

            if (typeof(T).IsSimpleType()) return (T) Convert.ChangeType(value, typeof(T));

            return JsonConvert.DeserializeObject<T>(value);
        }

        public void Add(string key, object value)
        {
            string str = null;
            if (value.GetType().IsSimpleType())
            {
                str = value.ToString();
            }
            else
            {
                str = Json.SerializeObject(value);
                if (str.Length > 250)
                {
                    var bytes = Encoding.UTF8.GetBytes(str);
                    bytes = Encryption.Compress(bytes);
                    _httpContextAccessor.HttpContext.Session.Set(key, bytes);
                    Dirty = true;
                    return;
                }
            }

            _httpContextAccessor.HttpContext.Session.SetString(key, str);
            Dirty = true;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            _httpContextAccessor.HttpContext.Session.Remove(key);
            Dirty = true;
        }

        public void Clear()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }
    }
}