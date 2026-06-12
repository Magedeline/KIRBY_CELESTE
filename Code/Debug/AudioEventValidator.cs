using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using Monocle;

namespace Celeste.Mod.MaggyHelper.Debug
{
    /// <summary>
    /// Audio event validation and debugging utility.
    /// Helps identify missing audio events and their sources.
    ///
    /// Usage: Call AudioEventValidator.ValidateEvent(eventPath) before playing audio
    /// </summary>
    public static class AudioEventValidator
    {
        private static Dictionary<string, string> _eventCache = new();
        private static Dictionary<string, string> _missingEvents = new();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                "Audio Event Validator initialized");
        }

        /// <summary>
        /// Validate if an audio event exists before playing it.
        /// Returns true if event is valid, false if missing.
        /// </summary>
        public static bool ValidateEvent(string eventPath)
        {
            if (string.IsNullOrEmpty(eventPath))
            {
                LogMissingEvent("(null)", "Event path is null or empty");
                return false;
            }

            try
            {
                // Check if event is already cached
                if (_eventCache.ContainsKey(eventPath))
                    return true;

                if (_missingEvents.ContainsKey(eventPath))
                    return false;

                // Try to get the event from FMOD
                var system = Audio.System;
                if (system == null)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/AudioValidator",
                        $"FMOD System not available for event: {eventPath}");
                    return false;
                }

                FMOD.RESULT result = system.getEvent(eventPath, out EventDescription eventDesc);

                if (result == FMOD.RESULT.OK && eventDesc.isValid())
                {
                    _eventCache[eventPath] = eventPath;
                    return true;
                }
                else
                {
                    LogMissingEvent(eventPath, $"FMOD result: {result}");
                    _missingEvents[eventPath] = result.ToString();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/AudioValidator",
                    $"Error validating event '{eventPath}': {ex.Message}");
                LogMissingEvent(eventPath, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Suggest the closest valid event based on partial matching.
        /// </summary>
        public static string SuggestAlternative(string missingEvent)
        {
            if (string.IsNullOrEmpty(missingEvent))
                return null;

            // Extract partial path for fuzzy matching
            var parts = missingEvent.Split('/');
            if (parts.Length < 2)
                return null;

            var category = parts[parts.Length - 2]; // e.g., "pusheen"
            var eventName = parts[parts.Length - 1]; // e.g., "gen_last_push"

            // Look for similar events
            var candidates = new List<string>();

            // Strategy 1: Same category, similar name
            var keywordMatches = _eventCache.Keys
                .Where(e => e.Contains(category) &&
                       (e.Contains("last") || e.Contains("push")))
                .ToList();

            if (keywordMatches.Count > 0)
                return keywordMatches.First();

            // Strategy 2: Similar event path structure
            keywordMatches = _eventCache.Keys
                .Where(e => e.Contains("lvl19") || e.Contains("level19"))
                .ToList();

            if (keywordMatches.Count > 0)
                return keywordMatches.FirstOrDefault(e => e.Contains("last") || e.Contains("push"))
                    ?? keywordMatches.First();

            return null;
        }

        private static void LogMissingEvent(string eventPath, string reason)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper/AudioValidator",
                $"[AUDIO EVENT NOT FOUND] {eventPath}");
            Logger.Log(LogLevel.Warn, "MaggyHelper/AudioValidator",
                $"  Reason: {reason}");

            // Try to suggest alternatives
            var suggestion = SuggestAlternative(eventPath);
            if (!string.IsNullOrEmpty(suggestion))
            {
                Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                    $"  Suggestion: Use '{suggestion}' instead");
            }
        }

        public static void PrintMissingEvents()
        {
            if (_missingEvents.Count == 0)
            {
                Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                    "No missing audio events detected!");
                return;
            }

            Logger.Log(LogLevel.Warn, "MaggyHelper/AudioValidator",
                $"=== MISSING AUDIO EVENTS ({_missingEvents.Count}) ===");

            foreach (var kvp in _missingEvents.OrderBy(x => x.Key))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/AudioValidator",
                    $"  ✗ {kvp.Key}");

                var suggestion = SuggestAlternative(kvp.Key);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                        $"    → Try: {suggestion}");
                }
            }
        }

        public static void PrintAudioStats()
        {
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                "=== AUDIO STATISTICS ===");
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                $"  Valid events cached: {_eventCache.Count}");
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                $"  Missing events found: {_missingEvents.Count}");
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                $"  Hit rate: {(_eventCache.Count > 0 ? (_eventCache.Count * 100 / (_eventCache.Count + _missingEvents.Count)) : 0)}%");
        }

        public static Dictionary<string, string> GetMissingEvents()
        {
            return new Dictionary<string, string>(_missingEvents);
        }

        public static Dictionary<string, string> GetValidEvents()
        {
            return new Dictionary<string, string>(_eventCache);
        }

        public static void Reset()
        {
            _eventCache.Clear();
            _missingEvents.Clear();
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioValidator",
                "Audio event cache cleared");
        }
    }
}
