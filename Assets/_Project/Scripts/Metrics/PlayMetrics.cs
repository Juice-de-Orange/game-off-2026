using System;
using System.Collections.Generic;
using System.Text;

namespace GameOff.Metrics
{
    /// <summary>
    /// P129: the queue of play events waiting to be sent (PlayMetricsRunner sends them to deploy/reports, /api/metrics).
    /// The session id is random and lives only in memory: nothing is stored in the browser, a reload is a new session.
    /// </summary>
    public static class PlayMetrics
    {
        /// <summary>Events kept while the server is unreachable; the oldest go first.</summary>
        public const int MaxQueued = 500;
        /// <summary>Events per request (the backend refuses more than 100).</summary>
        public const int MaxPerBatch = 100;

        private static readonly List<string> Queue = new List<string>();

        /// <summary>Off until the runner switches it on (WebGL build only, never in the editor or the tests).</summary>
        public static bool Enabled { get; set; }
        public static string SessionId { get; private set; } = NewSessionId();
        public static int Queued => Queue.Count;

        public static void Track(MetricsEvent e)
        {
            if (!Enabled || e == null) return;
            if (Queue.Count >= MaxQueued) Queue.RemoveAt(0);
            Queue.Add(e.ToJson());
        }

        /// <summary>The next request body with up to MaxPerBatch events (they leave the queue), or null when there is nothing.</summary>
        public static string TakeBatch(string version)
        {
            if (Queue.Count == 0) return null;
            int n = Math.Min(MaxPerBatch, Queue.Count);
            var json = new StringBuilder(n * 160);
            json.Append("{\"session\":");
            MetricsEvent.AppendString(json, SessionId);
            json.Append(",\"version\":");
            MetricsEvent.AppendString(json, version ?? "");
            json.Append(",\"events\":[");
            for (int i = 0; i < n; i++)
            {
                if (i > 0) json.Append(',');
                json.Append(Queue[i]);
            }
            json.Append("]}");
            Queue.RemoveRange(0, n);
            return json.ToString();
        }

        /// <summary>Empties the queue and starts a new session id (tests).</summary>
        public static void Reset()
        {
            Queue.Clear();
            SessionId = NewSessionId();
        }

        private static string NewSessionId() => Guid.NewGuid().ToString("N").Substring(0, 12);
    }
}
