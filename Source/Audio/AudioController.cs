using System;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Centralized audio controller for Desolo Zantas.
    /// Maps FMOD studio audio event instances to level progression states.
    /// Implements dynamic parameter adjustments (e.g., combat_intensity) when entering arena boundaries.
    /// </summary>
    public static class AudioController
    {
        #region State Definitions

        /// <summary>
        /// Music states that correspond to level progression.
        /// </summary>
        public enum MusicState
        {
            Ambient,        // Exploration, puzzle rooms
            Tension,        // Approaching danger
            Combat,         // Arena combat / boss fights
            Puzzle,         // Active puzzle solving
            Cutscene,       // Story sequences
            Victory,        // Post-completion
            Transition      // Screen transitions
        }

        /// <summary>
        /// Ambience states for environmental audio layering.
        /// </summary>
        public enum AmbienceState
        {
            None,
            Ruins,          // Chapter 10 - wind, dust
            Snowdin,        // Chapter 11 - cold wind, bells
            Water,          // Chapter 12 - flowing water
            Fire,           // Chapter 13 - lava rumble
            Digital,        // Chapter 14 - electronic hum
            Castle,         // Chapter 15 - grand halls
            Corruption,     // Chapter 16 - glitch/static
            Space,          // Chapter 19 - cosmic silence, starfield
            Heart           // Chapter 18 - core pulse
        }

        #endregion

        #region Event Paths

        // Base music events by chapter (structural complexity scales with progression)
        private static readonly Dictionary<string, string> ChapterMusicEvents = new Dictionary<string, string>
        {
            ["00_Prologue"]   = "event:/pusheen/music/lvl0/intro",
            ["01_City"]       = "event:/pusheen/music/lvl1/main",
            ["02_Nightmare"]  = "event:/pusheen/music/lvl2/beginning",
            ["03_Stars"]      = "event:/pusheen/music/lvl3/intro",
            ["04_Legend"]     = "event:/pusheen/music/lvl4/beginning",
            ["05_Restore"]    = "event:/pusheen/music/lvl5/intro",
            ["06_Stronghold"] = "event:/pusheen/music/lvl6/main",
            ["07_Hell"]       = "event:/pusheen/music/lvl7/main",
            ["08_Truth"]      = "event:/pusheen/music/lvl8/main",
            ["09_Summit"]     = "event:/pusheen/music/lvl9/main",
            ["10_Ruins"]      = "event:/pusheen/music/lvl10/main",
            ["11_Snow"]       = "event:/pusheen/music/lvl11/main",
            ["12_Water"]      = "event:/pusheen/music/lvl12/main",
            ["13_Fire"]       = "event:/pusheen/music/lvl13/main",
            ["14_Digital"]    = "event:/pusheen/music/lvl14/main",
            ["15_Castle"]     = "event:/pusheen/music/lvl15/main",
            ["16_Corruption"] = "event:/pusheen/music/lvl16/main",
            ["17_Epilogue"]   = "event:/pusheen/music/lvl17/main",
            ["18_Heart"]      = "event:/pusheen/music/lvl18/main",
            ["19_Space"]      = "event:/",
            ["20_TheEnd"]     = "event:/",
            ["21_LastLevel"]  = "event:/pusheen/extra_content/music/lvl21/main",
        };

        // Combat/intense music layers per chapter
        private static readonly Dictionary<string, string> ChapterCombatMusicEvents = new Dictionary<string, string>
        {
            ["10_Ruins"]      = "event:/pusheen/music/arena/battle_1",
            ["11_Snow"]       = "event:/pusheen/music/arena/battle_2",
            ["13_Fire"]       = "event:/pusheen/music/arena/battle_3",
            ["16_Corruption"] = "event:/pusheen/music/arena/battle_4",
            ["18_Heart"]      = "event:/pusheen/music/arena/battle_5",
            ["20_TheEnd"]     = "event:/pusheen/music/arena/battle_6",
            ["02_Nightmare"]  = "event:/pusheen/music/arena/battle_7",
            ["06_Stronghold"] = "event:/pusheen/music/arena/battle_8",
            ["07_Hell"]       = "event:/pusheen/music/arena/battle_9",
            ["09_Summit"]     = "event:/pusheen/music/arena/battle_10",
            ["12_Water"]      = "event:/pusheen/music/arena/battle_11",
            ["14_Digital"]    = "event:/pusheen/music/arena/battle_12",
            ["21_LastLevel"]  = "event:/pusheen/music/arena/battle_13",
        };

        // Global ambience events by chapter (00-18)
        private static readonly Dictionary<string, string> ChapterAmbienceEvents = new Dictionary<string, string>
        {
            ["00_Prologue"]   = "event:/pusheen/env/amb/00",
            ["01_City"]       = "event:/pusheen/env/amb/01",
            ["02_Nightmare"]  = "event:/pusheen/env/amb/02",
            ["03_Stars"]      = "event:/pusheen/env/amb/03",
            ["04_Legend"]     = "event:/pusheen/env/amb/04",
            ["05_Restore"]    = "event:/pusheen/env/amb/05",
            ["06_Stronghold"] = "event:/pusheen/env/amb/06",
            ["07_Hell"]       = "event:/pusheen/env/amb/07",
            ["08_Truth"]      = "event:/pusheen/env/amb/08",
            ["09_Summit"]     = "event:/pusheen/env/amb/09",
            ["10_Ruins"]      = "event:/pusheen/env/amb/10",
            ["11_Snow"]       = "event:/pusheen/env/amb/11",
            ["12_Water"]      = "event:/pusheen/env/amb/12",
            ["13_Fire"]       = "event:/pusheen/env/amb/13",
            ["14_Digital"]    = "event:/pusheen/env/amb/14",
            ["15_Castle"]     = "event:/pusheen/env/amb/15",
            ["16_Corruption"] = "event:/pusheen/env/amb/16",
            ["17_Epilogue"]   = "event:/pusheen/env/amb/17",
            ["18_Heart"]      = "event:/pusheen/env/amb/18",
            ["19_Space"]      = "event:/pusheen/extra_content/env/19",
            ["20_TheEnd"]     = "event:/pusheen/extra_content/env/20",
        };

        // Local ambience events by chapter (00-18) - for localized environmental sounds
        private static readonly Dictionary<string, string> ChapterLocalAmbienceEvents = new Dictionary<string, string>
        {
            ["00_Prologue"]   = "event:/pusheen/env/local/00",
            ["01_City"]       = "event:/pusheen/env/local/01",
            ["02_Nightmare"]  = "event:/pusheen/env/local/02",
            ["03_Stars"]      = "event:/pusheen/env/local/03",
            ["04_Legend"]     = "event:/pusheen/env/local/04",
            ["05_Restore"]    = "event:/pusheen/env/local/05",
            ["06_Stronghold"] = "event:/pusheen/env/local/06",
            ["07_Hell"]       = "event:/pusheen/env/local/07",
            ["08_Truth"]      = "event:/pusheen/env/local/08",
            ["09_Summit"]     = "event:/pusheen/env/local/09",
            ["10_Ruins"]      = "event:/pusheen/env/local/10",
            ["11_Snow"]       = "event:/pusheen/env/local/11",
            ["12_Water"]      = "event:/pusheen/env/local/12",
            ["13_Fire"]       = "event:/pusheen/env/local/13",
            ["14_Digital"]    = "event:/pusheen/env/local/14",
            ["15_Castle"]     = "event:/pusheen/env/local/15",
            ["16_Corruption"] = "event:/pusheen/env/local/16",
            ["17_Epilogue"]   = "event:/pusheen/env/local/17",
            ["18_Heart"]      = "event:/pusheen/env/local/18",
        };

        // FMOD parameter names for dynamic mixing
        private const string PARAM_COMBAT_INTENSITY = "combat_intensity";
        private const string PARAM_MUSIC_LAYER = "music_layer";
        private const string PARAM_TENSION = "tension_level";

        #endregion

        #region Runtime State

        private static MusicState _currentMusicState = MusicState.Ambient;
        private static AmbienceState _currentAmbience = AmbienceState.None;
        private static EventInstance _currentMusicInstance;
        private static EventInstance _currentAmbienceInstance;
        private static bool _hasMusicInstance = false;
        private static bool _hasAmbienceInstance = false;
        private static float _combatIntensity = 0f;
        private static string _currentChapter;
        private static bool _initialized = false;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the audio controller. Call once when the mod loads.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            Logger.Log(LogLevel.Info, "MaggyHelper/Audio", "AudioController initialized");
        }

        /// <summary>
        /// Shutdown and cleanup all active audio instances.
        /// </summary>
        public static void Shutdown()
        {
            StopMusic();
            StopAmbience();
            _initialized = false;
        }

        #endregion

        #region Music State Management

        /// <summary>
        /// Set the music state for the current level, adjusting FMOD parameters dynamically.
        /// </summary>
        public static void SetMusicState(MusicState state, Level level = null)
        {
            if (!_initialized) return;

            _currentMusicState = state;
            string chapter = GetCurrentChapter(level);
            if (chapter == null) return;

            switch (state)
            {
                case MusicState.Ambient:
                    TransitionToAmbient(chapter);
                    break;
                case MusicState.Tension:
                    TransitionToTension(chapter);
                    break;
                case MusicState.Combat:
                    TransitionToCombat(chapter);
                    break;
                case MusicState.Puzzle:
                    TransitionToPuzzle(chapter);
                    break;
                case MusicState.Cutscene:
                    TransitionToCutscene(chapter);
                    break;
                case MusicState.Victory:
                    TransitionToVictory(chapter);
                    break;
                case MusicState.Transition:
                    TransitionToTransition(chapter);
                    break;
            }

            Logger.Log(LogLevel.Debug, "MaggyHelper/Audio", $"Music state set to: {state} (chapter: {chapter})");
        }

        /// <summary>
        /// Set the combat intensity parameter (0.0 to 1.0).
        /// Higher values trigger denser percussion layers.
        /// </summary>
        public static void SetCombatIntensity(float intensity)
        {
            if (!_initialized) return;

            _combatIntensity = Calc.Clamp(intensity, 0f, 1f);

            if (_hasMusicInstance)
            {
                _currentMusicInstance.setParameterValue(PARAM_COMBAT_INTENSITY, _combatIntensity);
            }
        }

        /// <summary>
        /// Called when entering an arena boundary. Ramps up combat intensity.
        /// </summary>
        public static void EnterArenaBoundary(Level level)
        {
            SetMusicState(MusicState.Combat, level);
            SetCombatIntensity(0.5f);
            Logger.Log(LogLevel.Info, "MaggyHelper/Audio", "Entered arena boundary - combat music activated");
        }

        /// <summary>
        /// Called when leaving an arena boundary. Fades down combat intensity.
        /// </summary>
        public static void ExitArenaBoundary(Level level)
        {
            SetCombatIntensity(0f);
            SetMusicState(MusicState.Ambient, level);
            Logger.Log(LogLevel.Info, "MaggyHelper/Audio", "Exited arena boundary - returning to ambient");
        }

        /// <summary>
        /// Update combat intensity based on proximity to arena center or enemy count.
        /// Call this from a trigger or entity Update loop.
        /// </summary>
        public static void UpdateCombatIntensity(float normalizedProximity)
        {
            SetCombatIntensity(normalizedProximity);
        }

        #endregion

        #region Ambience Management

        /// <summary>
        /// Set the environmental ambience based on chapter.
        /// </summary>
        public static void SetAmbienceForChapter(string chapterName)
        {
            if (!_initialized) return;

            StopAmbience();

            if (ChapterAmbienceEvents.TryGetValue(chapterName, out string eventPath) && !string.IsNullOrEmpty(eventPath))
            {
                _currentAmbienceInstance = global::Celeste.Audio.Play(eventPath);
                _hasAmbienceInstance = true;
                Logger.Log(LogLevel.Debug, "MaggyHelper/Audio", $"Global ambience set for chapter: {chapterName}");
            }

            // Also play local ambience if available
            if (ChapterLocalAmbienceEvents.TryGetValue(chapterName, out string localEventPath) && !string.IsNullOrEmpty(localEventPath))
            {
                // Local ambience is played as a separate layer - could be stored separately if needed
                global::Celeste.Audio.Play(localEventPath);
                Logger.Log(LogLevel.Debug, "MaggyHelper/Audio", $"Local ambience set for chapter: {chapterName}");
            }
        }

        /// <summary>
        /// Set ambience based on legacy state (deprecated - use SetAmbienceForChapter instead).
        /// </summary>
        [Obsolete("Use SetAmbienceForChapter(string chapterName) instead")]
        public static void SetAmbience(AmbienceState state)
        {
            // Legacy method - map state back to approximate chapter
            string chapterName = state switch
            {
                AmbienceState.Ruins       => "10_Ruins",
                AmbienceState.Snowdin     => "11_Snow",
                AmbienceState.Water       => "12_Water",
                AmbienceState.Fire        => "13_Fire",
                AmbienceState.Digital     => "14_Digital",
                AmbienceState.Corruption  => "16_Corruption",
                AmbienceState.Heart       => "18_Heart",
                AmbienceState.Space       => "19_Space",
                _                         => null
            };

            if (chapterName != null)
            {
                SetAmbienceForChapter(chapterName);
            }
        }

        #endregion

        #region Private Transition Methods

        private static void TransitionToAmbient(string chapter)
        {
            if (ChapterMusicEvents.TryGetValue(chapter, out string eventPath))
            {
                PlayMusic(eventPath);
                SetParameter(PARAM_MUSIC_LAYER, 0f); // Subtle melodic layer
                SetParameter(PARAM_TENSION, 0f);
                SetCombatIntensity(0f);
            }
        }

        private static void TransitionToTension(string chapter)
        {
            SetParameter(PARAM_TENSION, 0.5f);
            SetParameter(PARAM_MUSIC_LAYER, 1f); // Add tension instruments
        }

        private static void TransitionToCombat(string chapter)
        {
            // Try combat-specific music first, fall back to explore with high intensity
            if (ChapterCombatMusicEvents.TryGetValue(chapter, out string combatEvent))
            {
                PlayMusic(combatEvent);
            }
            else if (ChapterMusicEvents.TryGetValue(chapter, out string exploreEvent))
            {
                PlayMusic(exploreEvent);
            }

            SetParameter(PARAM_MUSIC_LAYER, 2f); // Dense percussion layer
            SetParameter(PARAM_TENSION, 1f);
            SetCombatIntensity(1f);
        }

        private static void TransitionToPuzzle(string chapter)
        {
            if (ChapterMusicEvents.TryGetValue(chapter, out string eventPath))
            {
                PlayMusic(eventPath);
            }
            SetParameter(PARAM_MUSIC_LAYER, 0.5f); // Light puzzle layer
            SetParameter(PARAM_TENSION, 0.2f);
            SetCombatIntensity(0f);
        }

        private static void TransitionToCutscene(string chapter)
        {
            SetParameter(PARAM_TENSION, 0f);
            SetCombatIntensity(0f);
            // Cutscenes typically set their own music via Audio.SetMusic()
        }

        private static void TransitionToVictory(string chapter)
        {
            SetParameter(PARAM_MUSIC_LAYER, 0f);
            SetParameter(PARAM_TENSION, 0f);
            SetCombatIntensity(0f);
            global::Celeste.Audio.Play("event:/game/general/heartgem_get"); // Temporary victory sound
        }

        private static void TransitionToTransition(string chapter)
        {
            SetParameter(PARAM_TENSION, 0.3f);
            SetCombatIntensity(0f);
        }

        #endregion

        #region Playback Helpers

        private static void PlayMusic(string eventPath)
        {
            if (string.IsNullOrEmpty(eventPath)) return;

            StopMusic();
            _currentMusicInstance = global::Celeste.Audio.Play(eventPath);
            _hasMusicInstance = true;
        }

        private static void StopMusic()
        {
            if (_hasMusicInstance)
            {
                global::Celeste.Audio.Stop(_currentMusicInstance);
                _hasMusicInstance = false;
            }
        }

        private static void StopAmbience()
        {
            if (_hasAmbienceInstance)
            {
                global::Celeste.Audio.Stop(_currentAmbienceInstance);
                _hasAmbienceInstance = false;
            }
        }

        private static void SetParameter(string name, float value)
        {
            if (_hasMusicInstance)
            {
                _currentMusicInstance.setParameterValue(name, value);
            }
        }

        private static string GetCurrentChapter(Level level)
        {
            if (level != null && level.Session != null && level.Session.MapData != null && level.Session.MapData.Filename != null)
            {
                string filename = level.Session.MapData.Filename;
                // Extract chapter from filename like "01_City" or "10_Ruins"
                if (filename.Length >= 2)
                {
                    return filename;
                }
            }

            // Fallback: try to infer from area
            if (level != null && level.Session != null && level.Session.Area.SID != null)
            {
                string sid = level.Session.Area.SID;
                int lastSlash = sid.LastIndexOf('/');
                if (lastSlash >= 0 && lastSlash < sid.Length - 1)
                {
                    return sid.Substring(lastSlash + 1);
                }
            }

            return _currentChapter;
        }

        #endregion

        #region Level Hook Integration

        /// <summary>
        /// Call this when a level loads to set up appropriate music and ambience.
        /// </summary>
        public static void OnLevelLoad(Level level)
        {
            string chapter = GetCurrentChapter(level);
            if (chapter != null)
            {
                _currentChapter = chapter;
                SetMusicState(MusicState.Ambient, level);
                SetAmbienceForChapter(chapter);
            }
        }

        /// <summary>
        /// Call this when exiting a level to clean up audio state.
        /// </summary>
        public static void OnLevelExit()
        {
            StopMusic();
            StopAmbience();
            _currentMusicState = MusicState.Ambient;
            _currentAmbience = AmbienceState.None;
            _combatIntensity = 0f;
        }

        #endregion
    }
}
