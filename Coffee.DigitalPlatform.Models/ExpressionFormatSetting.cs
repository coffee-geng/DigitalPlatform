using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Coffee.DigitalPlatform.Models
{
    public class ExpressionFormatSetting
    {
        private Dictionary<string, string> _fontFamilyDict = new Dictionary<string, string>();

        private Dictionary<string, FontWeight> _fontWeightDict = new Dictionary<string, FontWeight>();

        private Dictionary<string, FontStyle> _fontStyleDict = new Dictionary<string, FontStyle>();

        private Dictionary<string, double> _fontSiaeDict = new Dictionary<string, double>();

        private Dictionary<string, Color> _fontForegroundDict = new Dictionary<string, Color>();

        public void AddFontFamily(string key, string fontFamily)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontFamilyDict[key] = fontFamily;
        }

        public void AddFontWeight(string key, FontWeight fontWeight)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontWeightDict[key] = fontWeight;
        }

        public void AddFontStyle(string key, FontStyle fontStyle)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontStyleDict[key] = fontStyle;
        }

        public void AddFontSize(string key, double fontSize)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontSiaeDict[key] = fontSize;
        }

        public void AddFontForeground(string key, Color fontForeground)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontForegroundDict[key] = fontForeground;
        }

        public void RemoveFontFamily(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontFamilyDict.Remove(key);
        }

        public void RemoveFontWeight(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontWeightDict.Remove(key);
        }

        public void RemoveFontStyle(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontStyleDict.Remove(key);
        }

        public void RemoveFontSize(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontSiaeDict.Remove(key);
        }

        public void RemoveFontForeground(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            _fontForegroundDict.Remove(key);
        }

        public string? GetFontFamily(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            return _fontFamilyDict.TryGetValue(key, out string? fontFamily) ? fontFamily : null;
        }

        public FontStyle GetFontStyle(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            return _fontStyleDict.TryGetValue(key, out FontStyle fontStyle) ? fontStyle : FontStyles.Normal;
        }

        public FontWeight GetFontWeight(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            return _fontWeightDict.TryGetValue(key, out FontWeight fontWeight) ? fontWeight : FontWeights.Normal;
        }

        public double GetFontSize(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            return _fontSiaeDict.TryGetValue(key, out double fontSize) ? fontSize : 12.0;
        }

        public Color GetFontForeground(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(nameof(key));
            return _fontForegroundDict.TryGetValue(key, out Color fontForeground) ? fontForeground : Colors.Black;
        }
    }
}
