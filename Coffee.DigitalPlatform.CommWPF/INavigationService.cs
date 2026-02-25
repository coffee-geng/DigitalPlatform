using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.CommWPF
{
    public interface INavigationService
    {
        void OnNavigateTo(NavigationContext context = null);

        void OnNavigateFrom(NavigationContext context = null);
    }

    public class NavigationContext
    {
        public NavigationContext(Uri uri, INavigationService fromContext, INavigationService toContext) 
        {
            Uri = uri;
            FromContext = fromContext;
            ToContext = toContext;
        }

        public NavigationContext(Uri uri, NavigationParameters paramters, INavigationService fromContext, INavigationService toContext)
        {
            Uri = uri;
            Parameters = paramters;
            ToContext = toContext;
        }

        public Uri Uri { get; private set; }

        public NavigationParameters Parameters { get; private set; }

        public INavigationService FromContext { get; }

        public INavigationService ToContext { get; }
    }

    public class NavigationParameters : IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private readonly Dictionary<string, object> _parameters;

        public NavigationParameters()
        {
            _parameters = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get
            {
                if (_parameters.TryGetValue(key, out object value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Add(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (!_parameters.ContainsKey(key))
            {
                _parameters.Add(key, value);
            }
            else
            {
                _parameters[key] = value;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join("&", _parameters.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? string.Empty)}"));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }
    }
}
