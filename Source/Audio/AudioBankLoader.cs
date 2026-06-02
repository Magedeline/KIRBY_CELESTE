using global::System;
using global::System.Collections.Generic;
using global::System.IO;
using global::System.Linq;
using FMOD.Studio;
using Celeste.Mod;
using Monocle;

namespace Celeste.Mod.MaggyHelper.Audio
{
    /// <summary>
    /// Manually registers dz_*.bank files with FMOD Studio for the pusheen event namespace.
    /// This solves the mismatch between bank filenames (dz_*) and event paths (event:/pusheen/*).
    /// </summary>
    public static class AudioBankLoader
    {
        private static bool _loaded = false;
        private static bool _banksRegistered = false;
        private static readonly List<Bank> _loadedBanks = new();

        // Bank files to load with their corresponding event prefixes
        private static readonly Dictionary<string, string[]> BankMappings = new()
        {
            ["dz_mus"] = new[] { "event:/pusheen/music", "event:/pusheen/env", "event:/pusheen/state" },
            ["dz_sfx"] = new[] { "event:/pusheen/char", "event:/pusheen/game" },
            ["dz_ui"] = new[] { "event:/pusheen/ui" },
            ["dz_dlc_mus"] = new[] { "event:/pusheen/dlc_mus" },
            ["dz_dlc_sfx"] = new[] { "event:/pusheen/dlc_sfx" }
        };

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;

            // Hook Audio.Init so banks are registered as soon as FMOD is ready,
            // before any scene (overworld, vignette, level) tries to play events.
            On.Celeste.Audio.Init += OnAudioInit;

            Logger.Log(LogLevel.Info, "MaggyHelper", "AudioBankLoader initialized - will register dz_*.bank files");
        }

        public static void Unload()
        {
            if (!_loaded) return;
            _loaded = false;

            On.Celeste.Audio.Init -= OnAudioInit;

            foreach (var bank in _loadedBanks)
            {
                try
                {
                    bank.unload();
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper", $"Error unloading bank: {ex.Message}");
                }
            }
            _loadedBanks.Clear();
            _banksRegistered = false;
        }

        private static void OnAudioInit(On.Celeste.Audio.orig_Init orig)
        {
            orig();
            if (!_banksRegistered)
            {
                _banksRegistered = true;
                RegisterBanks();
            }
        }

        /// <summary>
        /// Manually loads and registers a bank file with FMOD Studio.
        /// Call this from your mod's Load() method after the FMOD system is initialized.
        /// </summary>
        public static void RegisterBanks()
        {
            if (global::Celeste.Audio.System == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "FMOD System not available - cannot register banks yet");
                return;
            }

            // Get the mod path from Everest's content system
            string modPath = null;
            foreach (var mod in Everest.Modules)
            {
                if (mod.Metadata?.Name == "MaggyHelper")
                {
                    // Use reflection to get the path since it's not directly accessible
                    var pathProperty = mod.GetType().GetProperty("Path");
                    if (pathProperty != null)
                    {
                        modPath = pathProperty.GetValue(mod) as string;
                    }
                    break;
                }
            }

            if (string.IsNullOrEmpty(modPath))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Could not find mod path");
                return;
            }

            string audioPath = Path.Combine(modPath, "Audio");
            if (!Directory.Exists(audioPath))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"Audio directory not found: {audioPath}");
                return;
            }

            foreach (var mapping in BankMappings)
            {
                string bankName = mapping.Key;
                string bankPath = Path.Combine(audioPath, $"{bankName}.bank");

                if (!File.Exists(bankPath))
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper", $"Bank file not found: {bankPath}");
                    continue;
                }

                try
                {
                    // Load the bank into FMOD
                    Bank bank;
                    FMOD.RESULT result = global::Celeste.Audio.System.loadBankFile(bankPath, LOAD_BANK_FLAGS.NORMAL, out bank);

                    if (result == FMOD.RESULT.OK && bank.isValid())
                    {
                        _loadedBanks.Add(bank);
                        Logger.Log(LogLevel.Info, "MaggyHelper", $"Successfully loaded bank: {bankName}.bank");
                        bank.loadSampleData();
                    }
                    else if (result == FMOD.RESULT.ERR_VERSION)
                    {
                        Logger.Log(LogLevel.Error, "MaggyHelper",
                            $"Bank version mismatch for {bankName}.bank — bank was compiled with a different FMOD Studio version than Celeste's runtime (1.10.x). Recompile the bank with FMOD Studio 1.10.x.");
                    }
                    else if (result == FMOD.RESULT.ERR_FILE_NOTFOUND)
                    {
                        Logger.Log(LogLevel.Error, "MaggyHelper", $"Bank file not found at runtime: {bankPath}");
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "MaggyHelper",
                            $"Failed to load bank {bankName}.bank — FMOD error: {result} ({(int)result}). All events in this bank will be silent.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Exception loading bank {bankName}.bank: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
