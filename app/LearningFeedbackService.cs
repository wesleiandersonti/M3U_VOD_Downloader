using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MeuGestorVODs
{
    internal sealed class LearningFeedbackService
    {
        private readonly string _filePath;

        public LearningFeedbackService(string filePath)
        {
            _filePath = filePath;
        }

        public LearningSnapshot GetSnapshot(string scope)
        {
            var all = Load();
            var items = all.Where(x => string.Equals(x.Scope, scope, StringComparison.OrdinalIgnoreCase)).ToList();
            if (items.Count == 0)
            {
                return new LearningSnapshot(0, 0, 0, Array.Empty<string>());
            }

            var success = items.Count(x => x.Success);
            var failures = items.Count - success;
            var score = items.Sum(x => x.Success ? 2 : -1);

            var topErrors = items
                .Where(x => !x.Success && !string.IsNullOrWhiteSpace(x.ErrorCode))
                .GroupBy(x => x.ErrorCode!, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => $"{g.Key} ({g.Count()})")
                .ToList();

            return new LearningSnapshot(items.Count, success, score, topErrors);
        }

        public void Log(string scope, string action, bool success, string? errorCode = null, string? note = null)
        {
            var all = Load();
            all.Add(new LearningEvent
            {
                TimestampUtc = DateTime.UtcNow,
                Scope = scope,
                Action = action,
                Success = success,
                ErrorCode = errorCode,
                Note = note
            });

            // Mantem historico enxuto
            var recent = all.OrderByDescending(x => x.TimestampUtc).Take(2000).OrderBy(x => x.TimestampUtc).ToList();
            Save(recent);
        }

        private List<LearningEvent> Load()
        {
            try
            {
                if (!File.Exists(_filePath)) return new List<LearningEvent>();
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<List<LearningEvent>>(json);
                return data ?? new List<LearningEvent>();
            }
            catch
            {
                return new List<LearningEvent>();
            }
        }

        private void Save(List<LearningEvent> data)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    internal sealed class LearningEvent
    {
        public DateTime TimestampUtc { get; set; }
        public string Scope { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? Note { get; set; }
    }

    internal sealed class LearningSnapshot
    {
        public LearningSnapshot(int total, int success, int score, IReadOnlyList<string> topErrors)
        {
            Total = total;
            Success = success;
            Score = score;
            TopErrors = topErrors;
        }

        public int Total { get; }
        public int Success { get; }
        public int Score { get; }
        public IReadOnlyList<string> TopErrors { get; }
    }
}
