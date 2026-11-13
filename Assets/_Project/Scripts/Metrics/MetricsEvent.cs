using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameOff.Metrics
{
    /// <summary>
    /// P129: one play event as a flat JSON object ({"e": name, "t": seconds since start, ...}). Built by hand because JsonUtility
    /// can't write fields that differ per event. Keys are short lower-case names; deploy/reports drops anything else.
    /// </summary>
    public class MetricsEvent
    {
        private readonly StringBuilder _json = new StringBuilder(128);

        public MetricsEvent(string name, float time)
        {
            _json.Append("{\"e\":");
            AppendString(_json, name);
            _json.Append(",\"t\":").Append(Number(time));
        }

        public MetricsEvent Add(string key, string value)
        {
            Key(key);
            AppendString(_json, value ?? "");
            return this;
        }

        public MetricsEvent Add(string key, int value)
        {
            Key(key);
            _json.Append(value.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        public MetricsEvent Add(string key, float value)
        {
            Key(key);
            _json.Append(Number(value));
            return this;
        }

        public MetricsEvent Add(string key, bool value)
        {
            Key(key);
            _json.Append(value ? "true" : "false");
            return this;
        }

        public MetricsEvent Add(string key, IReadOnlyList<int> values)
        {
            Key(key);
            _json.Append('[');
            for (int i = 0; i < values.Count; i++)
            {
                if (i > 0) _json.Append(',');
                _json.Append(values[i].ToString(CultureInfo.InvariantCulture));
            }
            _json.Append(']');
            return this;
        }

        public string ToJson() => _json + "}";

        private void Key(string key)
        {
            _json.Append(',');
            AppendString(_json, key);
            _json.Append(':');
        }

        /// <summary>Two decimals, invariant culture (a German browser would write 1,5); NaN and infinity become 0.</summary>
        public static string Number(float value) =>
            float.IsNaN(value) || float.IsInfinity(value) ? "0" : System.Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);

        public static void AppendString(StringBuilder json, string text)
        {
            json.Append('"');
            foreach (char c in text)
            {
                switch (c)
                {
                    case '"': json.Append("\\\""); break;
                    case '\\': json.Append("\\\\"); break;
                    case '\n': json.Append("\\n"); break;
                    case '\r': json.Append("\\r"); break;
                    case '\t': json.Append("\\t"); break;
                    default:
                        if (c < ' ') json.Append("\\u").Append(((int)c).ToString("x4"));
                        else json.Append(c);
                        break;
                }
            }
            json.Append('"');
        }
    }
}
