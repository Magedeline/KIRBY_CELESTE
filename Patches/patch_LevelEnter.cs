#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using System;
using Celeste.Mod.MaggyHelper;
using Monocle;
using MonoMod;

namespace Celeste {
    class patch_LevelEnter : LevelEnter {

        // Static flag to prevent recursive postcard hook interception
        private static bool _skipPostcardOnce;

        /// <summary>
        /// Patch for LevelEnter.Go - Intercepts level loading to show postcard dialogs
        /// for first-time chapter entry (Chapters 1-16).
        /// </summary>
        [MonoModReplace]
        public static void Go(Session session, bool fromSaveData) {
            // Skip postcard interception if flag is set (used by PostcardDialogVignette)
            if (_skipPostcardOnce) {
                _skipPostcardOnce = false;
                orig_Go(session, fromSaveData);
                return;
            }

            // Check if this is a first-time chapter entry that should show postcard
            if (session?.Area != null && session.StartedFromBeginning && !fromSaveData) {
                int chapterNumber = GetChapterNumberFromSID(session.Area.SID);

                // Only show postcard for chapters 1-16 on A-Side
                if (chapterNumber >= 1 && chapterNumber <= 16 && (int)session.Area.Mode == 0) {
                    var saveData = MaggyHelperModule.SaveData;
                    if (saveData != null && !saveData.PostcardsShown.Contains(chapterNumber)) {
                        // Mark postcard as shown
                        saveData.PostcardsShown.Add(chapterNumber);

                        // Show postcard vignette
                        Logger.Log(LogLevel.Info, "MaggyHelper", $"Showing postcard for Chapter {chapterNumber}");
                        Engine.Scene = new PostcardDialogVignette(session, chapterNumber);
                        return;
                    }
                }
            }

            // Normal level entry
            orig_Go(session, fromSaveData);
        }

        /// <summary>
        /// Allows PostcardDialogVignette to signal that it's about to call LevelEnter.Go
        /// and the hook should skip interception.
        /// </summary>
        public static void SkipPostcardHookOnce() {
            _skipPostcardOnce = true;
        }

        private static int GetChapterNumberFromSID(string sid) {
            if (string.IsNullOrEmpty(sid))
                return -1;

            // Format: "Maggy/01_City_A_Side" -> extract "01"
            int slashIndex = sid.IndexOf('/');
            if (slashIndex >= 0 && slashIndex + 2 < sid.Length) {
                if (int.TryParse(sid.Substring(slashIndex + 1, 2), out int chapter)) {
                    return chapter;
                }
            }
            return -1;
        }

        [MonoModIgnore]
        public extern static void orig_Go(Session session, bool fromSaveData);
    }
}
