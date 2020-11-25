﻿using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Interfaces;
using Newtonsoft.Json;

namespace ModernSlavery.WebUI.Viewing.Classes
{
    public class SessionList<T> : IHashSet<T>
    {
        private readonly IHttpSession _session;
        private readonly string _sessionName;

        private HashSet<T> _items;

        public SessionList(IHttpSession session, string contextName, string variableName)
        {
            _session = session;
            _sessionName = $"{contextName}:{variableName}";
            LoadItems();
        }

        public int Count => _items.Count;

        public void Add(params T[] items)
        {
            _items.AddRange(items);
            SaveItems();
        }

        public IEnumerable<T> AsEnumerable()
        {
            return _items.AsEnumerable();
        }

        public void Clear()
        {
            _items.Clear();
            SaveItems();
        }

        public bool Contains(params T[] items)
        {
            return items.All(i => _items.Contains(i));
        }

        public void Remove(params T[] items)
        {
            _items.RemoveWhere(i => items.Any(i2 => i2.Equals(i)));
            SaveItems();
        }

        public IList<T> ToList()
        {
            return _items.ToList();
        }

        private void LoadItems()
        {
            var json = _session[_sessionName].ToStringOrNull();

            if (!string.IsNullOrWhiteSpace(json)) _items = JsonConvert.DeserializeObject<HashSet<T>>(json);

            if (_items == null) _items = new HashSet<T>();
        }

        private void SaveItems()
        {
            if (_items == null || _items.Count == 0)
            {
                _session.Remove(_sessionName);
            }
            else
            {
                var json = Json.SerializeObject(_items);

                _session[_sessionName] = json;
            }
        }
    }
}