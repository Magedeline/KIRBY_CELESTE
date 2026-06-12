using System;
using FMOD.Studio;
using Monocle;

namespace Celeste.Mod.MaggyHelper.Debug
{
    /// <summary>
    /// Audio debugging utilities for monitoring and validating audio events.
    /// This helps identify the source of missing audio event warnings.
    ///
    /// Usage:
    ///   AudioDebugHooks.Initialize();  // Enable audio validation
    ///   AudioDebugHooks.PrintAudioDebugStats();  // Print statistics
    /// </summary>
    public static class AudioDebugHooks
    {
        private static bool _loaded = false;
        private static int _eventCount = 0;
        private static int _errorCount = 0;

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;

            AudioEventValidator.Initialize();

            Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                "Audio Debug Hooks loaded");
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                "Commands available: maggy_check_gen_last_push, maggy_audio_missing, maggy_audio_find <keyword>");
        }

        public static void Unload()
        {
            if (!_loaded) return;
            _loaded = false;

            Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                "Audio Debug Hooks unloaded");

            PrintAudioDebugStats();
        }

        public static void RecordEvent(string path, bool isValid)
        {
            _eventCount++;

            if (!isValid)
            {
                _errorCount++;

                // Log the call stack to help identify where the bad event came from
                Logger.Log(LogLevel.Warn, "MaggyHelper/AudioDebug",
                    $"Missing audio event: {path}");

                // Try to suggest an alternative
                var suggestion = AudioEventValidator.SuggestAlternative(path);
                if (!string.IsNullOrEmpty(suggestion))
                {
                    Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                        $"  Suggestion: Try '{suggestion}' instead");
                }
            }
        }

        public static void PrintAudioDebugStats()
        {
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                "=== AUDIO DEBUG STATISTICS ===");
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                $"  Events validated: {_eventCount}");
            Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                $"  Missing/invalid events: {_errorCount}");
            if (_eventCount > 0)
            {
                Logger.Log(LogLevel.Info, "MaggyHelper/AudioDebug",
                    $"  Success rate: {(100 * (_eventCount - _errorCount) / _eventCount)}%");
            }

            AudioEventValidator.PrintMissingEvents();
            AudioEventValidator.PrintAudioStats();
        }

        public static void DumpMissingEvents()
        {
            AudioEventValidator.PrintMissingEvents();
        }

        public static int GetErrorCount() => _errorCount;
        public static int GetEventCount() => _eventCount;
    }
}
