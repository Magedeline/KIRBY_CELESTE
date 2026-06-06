#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using System;
using System.Collections;
using Celeste.Entities;
using Celeste.Mod.MaggyHelper;
using Monocle;
using MonoMod;

namespace Celeste.Entities {
    class patch_HeartGem : HeartGem {

        /// <summary>
        /// Patch for HeartGem.collect - Detects heart gem collection and shows
        /// postcard dialogs for D-Side and Ultra unlocks.
        /// </summary>
        [MonoModReplace]
        private void collect(global::Celeste.Player player) {
            // Call original collect logic
            orig_collect(player);

            try {
                // Get the current level to add postcard to
                Level level = SceneAs<Level>();
                if (level == null)
                    return;

                Session session = level.Session;
                if (session == null)
                    return;

                int currentMode = (int)session.Area.Mode;

                // D-Side unlock postcard (collected heart gem while in C-Side)
                if (currentMode == 2) { // MODE_CSIDE
                    var saveData = MaggyHelperModule.SaveData;
                    if (saveData != null && !saveData.DSideUnlockPostcardShown) {
                        saveData.DSideUnlockPostcardShown = true;
                        var entity = new Entity();
                        entity.Add(new Coroutine(ShowDSidePostcard(level)));
                        level.Add(entity);
                    }
                }

                // Ultra completion postcard (collected heart gem while in D-Side)
                if (currentMode == 3) { // MODE_DSIDE
                    var saveData = MaggyHelperModule.SaveData;
                    if (saveData != null && !saveData.UltraCompletionPostcardShown) {
                        saveData.UltraCompletionPostcardShown = true;
                        var entity = new Entity();
                        entity.Add(new Coroutine(ShowUltraPostcard(level)));
                        level.Add(entity);
                    }
                }
            }
            catch (Exception ex) {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Error in heart gem collection: " + ex.Message);
            }
        }

        private IEnumerator ShowDSidePostcard(Level level) {
            yield return 1.5f;
            var postcard = new PostcardMaggy("D-Side Unlocked!\nYour journey continues into darker depths.", "dsides");
            level.Add(postcard);
            yield return postcard.DisplayRoutine();
        }

        private IEnumerator ShowUltraPostcard(Level level) {
            yield return 1.5f;
            var postcard = new PostcardMaggy("Ultra Completion Unlocked!\nThe ultimate challenge awaits.", "ultra");
            level.Add(postcard);
            yield return postcard.DisplayRoutine();
        }

        [MonoModIgnore]
        private extern void orig_collect(global::Celeste.Player player);
    }
}
