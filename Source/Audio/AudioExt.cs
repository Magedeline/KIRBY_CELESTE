using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Celeste.Mod.MaggyHelper.Audio
{
    /// <summary>
    /// Runtime audio event path overrides loaded from Audio/audio_ext.yaml.
    ///
    /// Any FMOD event path used in the mod can be overridden externally without
    /// recompiling. Place a YAML file at:
    ///   [mod folder]/Audio/audio_ext.yaml
    ///
    /// Format (flat key → event path):
    ///   char.badeline.appear: "event:/my_pack/badeline/appear"
    ///   game.general.spring:  "event:/my_pack/general/spring"
    ///
    /// Keys map 1-to-1 with the static constants in AudioEvents.cs:
    ///   ClassName.NestedClass.ConstName  →  classname.nestedclass.constname
    ///
    /// Call AudioExt.Initialize(modRoot) once at mod load, then AudioExt.Reload()
    /// to re-read the file at runtime (e.g. after editing).
    /// Use AudioExt.Get("key", fallback) to resolve a path with automatic fallback.
    /// </summary>
    public static class AudioExt
    {
        private const string RelativePath = "Audio/audio_ext.yaml";

        private static string _filePath;
        private static Dictionary<string, string> _overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static bool _loaded = false;

        private static readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        // ── Loading ──────────────────────────────────────────────────────────

        /// <summary>
        /// Set the mod root path and load overrides. Call once during MaggyHelperModule.Load().
        /// </summary>
        public static void Initialize(string modRoot)
        {
            _filePath = Path.Combine(modRoot, RelativePath);
            Reload();
        }

        /// <summary>
        /// Ensure the override table is loaded. Called lazily on first Get()
        /// if Initialize() was not called yet.
        /// </summary>
        public static void EnsureLoaded()
        {
            if (!_loaded)
                Reload();
        }

        /// <summary>
        /// Re-read Audio/audio_ext.yaml from disk.
        /// Safe to call at runtime; clears the previous table first.
        /// </summary>
        public static void Reload()
        {
            _overrides.Clear();
            _loaded = true;

            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath))
            {
                Logger.Log(LogLevel.Verbose, "MaggyHelper/AudioExt", $"No audio_ext.yaml found – using compiled defaults");
                return;
            }

            try
            {
                string yaml = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(yaml))
                    return;

                var raw = _deserializer.Deserialize<Dictionary<string, string>>(yaml);
                if (raw != null)
                {
                    foreach (var kv in raw)
                    {
                        if (!string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                            _overrides[kv.Key.Trim()] = kv.Value.Trim();
                    }
                }

                Logger.Log(LogLevel.Info, "MaggyHelper/AudioExt", $"Loaded {_overrides.Count} audio event override(s) from {RelativePath}");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper/AudioExt", $"Failed to load {RelativePath}: {ex.Message}");
            }
        }

        // ── Core API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve an audio event path. Returns the override if one exists for
        /// <paramref name="key"/>, otherwise returns <paramref name="fallback"/>.
        /// </summary>
        /// <param name="key">Dot-separated key, e.g. "char.badeline.appear"</param>
        /// <param name="fallback">The compiled-in default event path to use when no override is defined.</param>
        public static string Get(string key, string fallback)
        {
            EnsureLoaded();
            return _overrides.TryGetValue(key, out string value) ? value : fallback;
        }

        /// <summary>
        /// Returns true if an override exists for the given key.
        /// </summary>
        public static bool Has(string key)
        {
            EnsureLoaded();
            return _overrides.ContainsKey(key);
        }

        /// <summary>
        /// Read-only view of all loaded overrides.
        /// </summary>
        public static IReadOnlyDictionary<string, string> All
        {
            get
            {
                EnsureLoaded();
                return _overrides;
            }
        }

        // ── Typed Lookups ────────────────────────────────────────────────────
        // Convenience wrappers that mirror AudioEvents.cs, so call sites can use
        // AudioExt.Char.Badeline.Appear instead of AudioExt.Get("char.badeline.appear", ...)

        public static class Char
        {
            public static class Badeline
            {
                public static string Appear         => Get("char.badeline.appear",           CharacterSfx.Badeline.Appear);
                public static string BoosterBegin   => Get("char.badeline.booster_begin",    CharacterSfx.Badeline.BoosterBegin);
                public static string BoosterFinal   => Get("char.badeline.booster_final",    CharacterSfx.Badeline.BoosterFinal);
                public static string BoosterReappear=> Get("char.badeline.booster_reappear", CharacterSfx.Badeline.BoosterReappear);
                public static string BoosterRelocate=> Get("char.badeline.booster_relocate", CharacterSfx.Badeline.BoosterRelocate);
                public static string BoosterThrow   => Get("char.badeline.booster_throw",    CharacterSfx.Badeline.BoosterThrow);
                public static string BossBullet     => Get("char.badeline.boss_bullet",      CharacterSfx.Badeline.BossBullet);
                public static string BossHug        => Get("char.badeline.boss_hug",         CharacterSfx.Badeline.BossHug);
                public static string BossIdleAir    => Get("char.badeline.boss_idle_air",    CharacterSfx.Badeline.BossIdleAir);
                public static string BossLaserCharge=> Get("char.badeline.boss_laser_charge",CharacterSfx.Badeline.BossLaserCharge);
                public static string BossLaserFire  => Get("char.badeline.boss_laser_fire",  CharacterSfx.Badeline.BossLaserFire);
                public static string ClimbLedge     => Get("char.badeline.climb_ledge",      CharacterSfx.Badeline.ClimbLedge);
                public static string DashRedLeft    => Get("char.badeline.dash_red_left",    CharacterSfx.Badeline.DashRedLeft);
                public static string DashRedRight   => Get("char.badeline.dash_red_right",   CharacterSfx.Badeline.DashRedRight);
                public static string Disappear      => Get("char.badeline.disappear",        CharacterSfx.Badeline.Disappear);
                public static string DreamblockEnter=> Get("char.badeline.dreamblock_enter", CharacterSfx.Badeline.DreamblockEnter);
                public static string DreamblockExit => Get("char.badeline.dreamblock_exit",  CharacterSfx.Badeline.DreamblockExit);
                public static string Duck           => Get("char.badeline.duck",             CharacterSfx.Badeline.Duck);
                public static string Footstep       => Get("char.badeline.footstep",         CharacterSfx.Badeline.Footstep);
                public static string Grab           => Get("char.badeline.grab",             CharacterSfx.Badeline.Grab);
                public static string GrabLetgo      => Get("char.badeline.grab_letgo",       CharacterSfx.Badeline.GrabLetgo);
                public static string Jump           => Get("char.badeline.jump",             CharacterSfx.Badeline.Jump);
                public static string JumpAssisted   => Get("char.badeline.jump_assisted",    CharacterSfx.Badeline.JumpAssisted);
                public static string LevelEntry     => Get("char.badeline.level_entry",      CharacterSfx.Badeline.LevelEntry);
                public static string MaddyJoin      => Get("char.badeline.maddy_join",       CharacterSfx.Badeline.MaddyJoin);
                public static string MaddySplit     => Get("char.badeline.maddy_split",      CharacterSfx.Badeline.MaddySplit);
                public static string Stand          => Get("char.badeline.stand",            CharacterSfx.Badeline.Stand);
                public static string Wallslide      => Get("char.badeline.wallslide",        CharacterSfx.Badeline.Wallslide);
            }

            public static class Madeline
            {
                public static string BackpackDrop   => Get("char.madeline.backpack_drop",    CharacterSfx.Madeline.BackpackDrop);
                public static string ClimbLedge     => Get("char.madeline.climb_ledge",      CharacterSfx.Madeline.ClimbLedge);
                public static string DashPinkLeft   => Get("char.madeline.dash_pink_left",   CharacterSfx.Madeline.DashPinkLeft);
                public static string DashPinkRight  => Get("char.madeline.dash_pink_right",  CharacterSfx.Madeline.DashPinkRight);
                public static string DashRedLeft    => Get("char.madeline.dash_red_left",    CharacterSfx.Madeline.DashRedLeft);
                public static string DashRedRight   => Get("char.madeline.dash_red_right",   CharacterSfx.Madeline.DashRedRight);
                public static string Death          => Get("char.madeline.death",            CharacterSfx.Madeline.Death);
                public static string DreamblockEnter=> Get("char.madeline.dreamblock_enter", CharacterSfx.Madeline.DreamblockEnter);
                public static string DreamblockExit => Get("char.madeline.dreamblock_exit",  CharacterSfx.Madeline.DreamblockExit);
                public static string Duck           => Get("char.madeline.duck",             CharacterSfx.Madeline.Duck);
                public static string Footstep       => Get("char.madeline.footstep",         CharacterSfx.Madeline.Footstep);
                public static string Grab           => Get("char.madeline.grab",             CharacterSfx.Madeline.Grab);
                public static string GrabLetgo      => Get("char.madeline.grab_letgo",       CharacterSfx.Madeline.GrabLetgo);
                public static string Jump           => Get("char.madeline.jump",             CharacterSfx.Madeline.Jump);
                public static string JumpAssisted   => Get("char.madeline.jump_assisted",    CharacterSfx.Madeline.JumpAssisted);
                public static string Landing        => Get("char.madeline.landing",          CharacterSfx.Madeline.Landing);
                public static string Predeath       => Get("char.madeline.predeath",         CharacterSfx.Madeline.Predeath);
                public static string Revive         => Get("char.madeline.revive",           CharacterSfx.Madeline.Revive);
                public static string Stand          => Get("char.madeline.stand",            CharacterSfx.Madeline.Stand);
                public static string Wallslide      => Get("char.madeline.wallslide",        CharacterSfx.Madeline.Wallslide);
                public static string WaterIn        => Get("char.madeline.water_in",         CharacterSfx.Madeline.WaterIn);
                public static string WaterOut       => Get("char.madeline.water_out",        CharacterSfx.Madeline.WaterOut);
            }
        }

        public static class Ui
        {
            public static string ButtonBack         => Get("ui.main.button_back",       UiSfx.MainButtonBack);
            public static string ButtonSelect       => Get("ui.main.button_select",     UiSfx.MainButtonSelect);
            public static string ButtonInvalid      => Get("ui.main.button_invalid",    UiSfx.MainButtonInvalid);
            public static string ButtonToggleOn     => Get("ui.main.button_toggle_on",  UiSfx.MainButtonToggleOn);
            public static string ButtonToggleOff    => Get("ui.main.button_toggle_off", UiSfx.MainButtonToggleOff);
            public static string GamePause          => Get("ui.game.pause",             UiSfx.GamePause);
            public static string GameUnpause        => Get("ui.game.unpause",           UiSfx.GameUnpause);
        }

        public static class Game
        {
            public static string Spring             => Get("game.general.spring",               GameSfx.GeneralSpring);
            public static string DiamondTouch       => Get("game.general.diamond_touch",        GameSfx.GeneralDiamondTouch);
            public static string DiamondReturn      => Get("game.general.diamond_return",       GameSfx.GeneralDiamondReturn);
            public static string KeyGet             => Get("game.general.key_get",              GameSfx.GeneralKeyGet);
            public static string StrawberryGet      => Get("game.general.strawberry_get",       GameSfx.GeneralStrawberryGet);
            public static string CrystalheartRed    => Get("game.general.crystalheart_red_get", GameSfx.GeneralCrystalheartRedGet);
            public static string CrystalheartBlue   => Get("game.general.crystalheart_blue_get",GameSfx.GeneralCrystalheartBlueGet);
            public static string CrystalheartGold   => Get("game.general.crystalheart_gold_get",GameSfx.GeneralCrystalheartGoldGet);
            public static string CassetteGet        => Get("game.general.cassette_get",         GameSfx.GeneralCassetteGet);
        }

        public static class Mus
        {
            public static class Cassette
            {
                public static string ForsakenCity   => Get("music.cassette.forsaken_city", Audio.Music.Cassette.ForsakenCity);
                public static string OldSite        => Get("music.cassette.old_site",      Audio.Music.Cassette.OldSite);
                public static string Resort         => Get("music.cassette.resort",        Audio.Music.Cassette.Resort);
                public static string Cliffside      => Get("music.cassette.cliffside",     Audio.Music.Cassette.Cliffside);
                public static string MirrorTemple   => Get("music.cassette.mirror_temple", Audio.Music.Cassette.MirrorTemple);
                public static string Reflection     => Get("music.cassette.reflection",    Audio.Music.Cassette.Reflection);
                public static string Summit         => Get("music.cassette.summit",        Audio.Music.Cassette.Summit);
                public static string Core           => Get("music.cassette.core",          Audio.Music.Cassette.Core);
            }

            public static class Level
            {
                public static string Lvl1Main       => Get("music.lvl1.main",     Audio.Music.Level.Lvl1Main);
                public static string Lvl2Beginning  => Get("music.lvl2.beginning",Audio.Music.Level.Lvl2Beginning);
                public static string Lvl2Chase      => Get("music.lvl2.chase",    Audio.Music.Level.Lvl2Chase);
                public static string Lvl6BadelineFight => Get("music.lvl6.badeline_fight", Audio.Music.Level.Lvl6BadelineFight);
                public static string Lvl7Main       => Get("music.lvl7.main",     Audio.Music.Level.Lvl7Main);
                public static string Lvl9Main       => Get("music.lvl9.main",     Audio.Music.Level.Lvl9Main);
            }
        }
    }
}
