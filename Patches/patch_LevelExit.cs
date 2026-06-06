#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using System;
using System.Collections;
using Celeste.Mod.MaggyHelper;
using Monocle;
using MonoMod;

namespace Celeste {
    class patch_LevelExit : LevelExit {

        /// <summary>
        /// Patch for LevelExit.Routine - Handles postcard displays after level completion
        /// including Chapter 18 outro and side unlock postcards.
        /// </summary>
        [MonoModReplace]
        public IEnumerator Routine() {
            Session session = this.session;
            int chapterNumber = GetChapterNumberFromSID(session?.Area.SID ?? "");

            // Check for Chapter 18 outro postcard
            if (this.mode == LevelExit.Mode.Completed && chapterNumber == 18) {
                var saveData = MaggyHelperModule.SaveData;
                if (saveData != null && !saveData.Chapter18OutroPostcardShown) {
                    IEnumerator origRoutine = orig_Routine();
                    while (origRoutine.MoveNext())
                        yield return origRoutine.Current;

                    saveData.Chapter18OutroPostcardShown = true;
                    yield return new PostcardOutroVignette(session, 18).Begin();
                    yield break;
                }
            }

            // Check for side unlock postcards
            if (this.mode == LevelExit.Mode.Completed && session != null) {
                int currentMode = (int)session.Area.Mode;

                // D-Side unlock postcard (after C-Side completion)
                if (currentMode == 2) { // MODE_CSIDE
                    var saveData = MaggyHelperModule.SaveData;
                    if (saveData != null && !saveData.DSideUnlockPostcardShown) {
                        IEnumerator origRoutine = orig_Routine();
                        while (origRoutine.MoveNext())
                            yield return origRoutine.Current;

                        saveData.DSideUnlockPostcardShown = true;
                        var postcard = new PostcardMaggy("D-Side Unlocked!\nYour journey continues into darker depths.", "dsides");
                        Engine.Scene.Add(postcard);
                        yield return postcard.DisplayRoutine();
                        yield break;
                    }
                }

                // Ultra completion postcard (after D-Side completion)
                if (currentMode == 3) { // MODE_DSIDE
                    var saveData = MaggyHelperModule.SaveData;
                    if (saveData != null && !saveData.UltraCompletionPostcardShown) {
                        IEnumerator origRoutine = orig_Routine();
                        while (origRoutine.MoveNext())
                            yield return origRoutine.Current;

                        saveData.UltraCompletionPostcardShown = true;
                        var postcard = new PostcardMaggy("Ultra Completion Unlocked!\nThe ultimate challenge awaits.", "ultra");
                        Engine.Scene.Add(postcard);
                        yield return postcard.DisplayRoutine();
                        yield break;
                    }
                }
            }

            // Normal exit routine
            IEnumerator routine = orig_Routine();
            while (routine.MoveNext())
                yield return routine.Current;
        }

        private static int GetChapterNumberFromSID(string sid) {
            if (string.IsNullOrEmpty(sid))
                return -1;

            int slashIndex = sid.IndexOf('/');
            if (slashIndex >= 0 && slashIndex + 2 < sid.Length) {
                if (int.TryParse(sid.Substring(slashIndex + 1, 2), out int chapter)) {
                    return chapter;
                }
            }
            return -1;
        }

        [MonoModIgnore]
        public extern IEnumerator orig_Routine();
    }
}
