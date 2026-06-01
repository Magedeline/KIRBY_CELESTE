using System;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaggyHelper.Audio
{
    /// <summary>
    /// Centralized audio manager for Pusheen FMOD events and VCA control.
    /// Provides easy access to Pusheen-specific audio with proper fallback handling.
    /// </summary>
    public static class PusheenAudioManager
    {
        private static bool _initialized = false;
        private static readonly Dictionary<string, EventInstance> _activeInstances = new Dictionary<string, EventInstance>();
        private static readonly Dictionary<string, VCA> _cachedVCAs = new Dictionary<string, VCA>();

        // ── Initialization ──────────────────────────────────────────────────────

        /// <summary>
        /// Initialize the Pusheen audio system. Call once during mod load.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            try
            {
                // Pre-cache commonly used VCAs for better performance
                CacheVCA(PusheenVca.Master);
                CacheVCA(PusheenVca.Sfx);
                CacheVCA(PusheenVca.Music);
                CacheVCA(PusheenVca.Dialogue);
                CacheVCA(PusheenVca.Ui);

                _initialized = true;
                Logger.Log(LogLevel.Info, "MaggyHelper/PusheenAudio", "Pusheen audio system initialized");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/PusheenAudio", $"Failed to initialize: {ex.Message}");
            }
        }

        // ── Core Audio Playback ─────────────────────────────────────────────────

        /// <summary>
        /// Play a Pusheen character sound effect with optional position.
        /// </summary>
        public static void PlayCharacterSfx(string eventPath, Vector2? position = null)
        {
            Play(eventPath, position);
        }

        /// <summary>
        /// Play a Pusheen environment sound effect with optional position.
        /// </summary>
        public static void PlayEnvironmentSfx(string eventPath, Vector2? position = null)
        {
            Play(eventPath, position);
        }

        /// <summary>
        /// Play a Pusheen gameplay sound effect with optional position.
        /// </summary>
        public static void PlayGameplaySfx(string eventPath, Vector2? position = null)
        {
            Play(eventPath, position);
        }

        /// <summary>
        /// Play a Pusheen UI sound effect (2D, no position).
        /// </summary>
        public static void PlayUiSfx(string eventPath)
        {
            Play(eventPath, null);
        }

        /// <summary>
        /// Play Pusheen music (looping, managed separately).
        /// </summary>
        public static EventInstance PlayMusic(string eventPath)
        {
            return PlayLooping(eventPath, "music");
        }

        /// <summary>
        /// Play a Pusheen dialogue/voice line.
        /// </summary>
        public static void PlayDialogue(string eventPath)
        {
            Play(eventPath, null);
        }

        // ── Low-Level Playback Methods ─────────────────────────────────────────────

        /// <summary>
        /// Play any Pusheen audio event with optional position.
        /// </summary>
        public static void Play(string eventPath, Vector2? position = null)
        {
            if (string.IsNullOrEmpty(eventPath))
                return;

            try
            {
                string resolvedPath = AudioExt.Get(eventPath, eventPath);
                EventInstance instance = global::Celeste.Audio.Play(resolvedPath, position ?? Vector2.Zero);

                if (instance.isValid())
                {
                    // Store for cleanup if needed
                    string key = $"{resolvedPath}_{Guid.NewGuid():N}";
                    _activeInstances[key] = instance;

                    // Note: Callback removed due to FMOD API compatibility issues
                    // Cleanup will be handled manually or through the StopAll method
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/PusheenAudio", $"Failed to play {eventPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Play a looping audio event (for music/ambient).
        /// </summary>
        public static EventInstance PlayLooping(string eventPath, string category = "ambient")
        {
            if (string.IsNullOrEmpty(eventPath))
                return default(EventInstance);

            try
            {
                string resolvedPath = AudioExt.Get(eventPath, eventPath);
                EventInstance instance = global::Celeste.Audio.Play(resolvedPath, Vector2.Zero);

                if (instance.isValid())
                {
                    string key = $"{category}_{resolvedPath}";
                    
                    // Stop existing instance if playing
                    if (_activeInstances.TryGetValue(key, out EventInstance existing))
                    {
                        existing.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        existing.release();
                    }

                    _activeInstances[key] = instance;
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/PusheenAudio", $"Failed to play looping {eventPath}: {ex.Message}");
            }

            return default(EventInstance);
        }

        // ── VCA Control ───────────────────────────────────────────────────────────

        /// <summary>
        /// Set the volume of a Pusheen VCA (0.0 to 1.0).
        /// </summary>
        public static void SetVcaVolume(string vcaPath, float volume)
        {
            if (!_cachedVCAs.TryGetValue(vcaPath, out VCA vca))
            {
                vca = CacheVCA(vcaPath);
            }

            if (vca.isValid())
            {
                vca.setVolume(MathHelper.Clamp(volume, 0f, 1f));
            }
        }

        /// <summary>
        /// Get the current volume of a Pusheen VCA.
        /// </summary>
        public static float GetVcaVolume(string vcaPath)
        {
            if (!_cachedVCAs.TryGetValue(vcaPath, out VCA vca))
            {
                vca = CacheVCA(vcaPath);
            }

            if (vca.isValid())
            {
                vca.getVolume(out float volume, out _);
                return volume;
            }

            return 0f;
        }

        /// <summary>
        /// Mute or unmute a Pusheen VCA.
        /// </summary>
        public static void SetVcaMute(string vcaPath, bool muted)
        {
            if (!_cachedVCAs.TryGetValue(vcaPath, out VCA vca))
            {
                vca = CacheVCA(vcaPath);
            }

            if (vca.isValid())
            {
                // Note: setMute may not be available in this FMOD version
                // Alternative: set volume to 0 for mute, restore for unmute
                if (muted)
                    vca.setVolume(0f);
                else
                    vca.setVolume(1f);
            }
        }

        // ── Convenience Methods ─────────────────────────────────────────────────────

        /// <summary>
        /// Play common Pusheen character sounds.
        /// </summary>
        public static class Character
        {
            public static void Jump(Vector2? position = null) => PlayCharacterSfx(PusheenCharacterSfx.Jump, position);
            public static void Land(Vector2? position = null) => PlayCharacterSfx(PusheenCharacterSfx.Landing, position);
            public static void Footstep(Vector2? position = null) => PlayCharacterSfx(PusheenCharacterSfx.Footstep, position);
            public static void Dash(Vector2? position = null) => PlayCharacterSfx(PusheenCharacterSfx.DashLeft, position);
            public static void Death(Vector2? position = null) => PlayCharacterSfx(PusheenCharacterSfx.Death, position);
            public static void Hurt(Vector2? position = null) => PlayCharacterSfx(PusheenCharacterSfx.Hurt, position);
            public static void Celebrate(Vector2? position = null) => PlayCharacterSfx(PusheenCharacterSfx.Celebrate, position);
        }

        /// <summary>
        /// Play common Pusheen gameplay sounds.
        /// </summary>
        public static class Gameplay
        {
            public static void StrawberryGet(Vector2? position = null) => PlayGameplaySfx(PusheenGameplaySfx.StrawberryGet, position);
            public static void KeyGet(Vector2? position = null) => PlayGameplaySfx(PusheenGameplaySfx.KeyGet, position);
            public static void HeartGet(Vector2? position = null) => PlayGameplaySfx(PusheenGameplaySfx.HeartGet, position);
            public static void RefillGet(Vector2? position = null) => PlayGameplaySfx(PusheenGameplaySfx.RefillGet, position);
            public static void FeatherGet(Vector2? position = null) => PlayGameplaySfx(PusheenGameplaySfx.FeatherGet, position);
            public static void Spring(Vector2? position = null) => PlayGameplaySfx(PusheenGameplaySfx.Spring, position);
            public static void CheckpointTouch(Vector2? position = null) => PlayGameplaySfx(PusheenGameplaySfx.CheckpointTouch, position);
        }

        /// <summary>
        /// Play common Pusheen UI sounds.
        /// </summary>
        public static class UI
        {
            public static void ButtonSelect() => PlayUiSfx(PusheenUiSfx.ButtonSelect);
            public static void ButtonBack() => PlayUiSfx(PusheenUiSfx.ButtonBack);
            public static void ButtonInvalid() => PlayUiSfx(PusheenUiSfx.ButtonInvalid);
            public static void Pause() => PlayUiSfx(PusheenUiSfx.Pause);
            public static void Unpause() => PlayUiSfx(PusheenUiSfx.Unpause);
            public static void WhooshIn() => PlayUiSfx(PusheenUiSfx.WhooshIn);
            public static void WhooshOut() => PlayUiSfx(PusheenUiSfx.WhooshOut);
        }

        // ── Volume Control Presets ─────────────────────────────────────────────────

        /// <summary>
        /// Set master volume for all Pusheen audio.
        /// </summary>
        public static float MasterVolume
        {
            get => GetVcaVolume(PusheenVca.Master);
            set => SetVcaVolume(PusheenVca.Master, value);
        }

        /// <summary>
        /// Set SFX volume for all Pusheen sound effects.
        /// </summary>
        public static float SfxVolume
        {
            get => GetVcaVolume(PusheenVca.Sfx);
            set => SetVcaVolume(PusheenVca.Sfx, value);
        }

        /// <summary>
        /// Set music volume for all Pusheen music.
        /// </summary>
        public static float MusicVolume
        {
            get => GetVcaVolume(PusheenVca.Music);
            set => SetVcaVolume(PusheenVca.Music, value);
        }

        /// <summary>
        /// Set dialogue volume for all Pusheen voice lines.
        /// </summary>
        public static float DialogueVolume
        {
            get => GetVcaVolume(PusheenVca.Dialogue);
            set => SetVcaVolume(PusheenVca.Dialogue, value);
        }

        // ── Utility Methods ───────────────────────────────────────────────────────

        /// <summary>
        /// Stop all active Pusheen audio instances.
        /// </summary>
        public static void StopAll()
        {
            foreach (var kvp in _activeInstances)
            {
                try
                {
                    kvp.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    kvp.Value.release();
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper/PusheenAudio", $"Failed to stop instance: {ex.Message}");
                }
            }
            _activeInstances.Clear();
        }

        /// <summary>
        /// Stop a specific category of Pusheen audio.
        /// </summary>
        public static void StopCategory(string category)
        {
            var toRemove = new List<string>();
            foreach (var kvp in _activeInstances)
            {
                if (kvp.Key.StartsWith(category))
                {
                    try
                    {
                        kvp.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        kvp.Value.release();
                        toRemove.Add(kvp.Key);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Warn, "MaggyHelper/PusheenAudio", $"Failed to stop category {category}: {ex.Message}");
                    }
                }
            }

            foreach (string key in toRemove)
            {
                _activeInstances.Remove(key);
            }
        }

        /// <summary>
        /// Clean up resources. Call during mod unload.
        /// </summary>
        public static void Dispose()
        {
            StopAll();
            _cachedVCAs.Clear();
            _initialized = false;
            Logger.Log(LogLevel.Info, "MaggyHelper/PusheenAudio", "Pusheen audio system disposed");
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        private static VCA CacheVCA(string vcaPath)
        {
            try
            {
                // For now, return a default VCA since we don't have direct access to FMOD system
                // In a real implementation, this would access the FMOD VCA system
                Logger.Log(LogLevel.Info, "MaggyHelper/PusheenAudio", $"VCA access not implemented for {vcaPath}");
                return default(VCA);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/PusheenAudio", $"Failed to cache VCA {vcaPath}: {ex.Message}");
                return default(VCA);
            }
        }
    }
}
