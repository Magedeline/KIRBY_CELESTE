#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Monocle;
using MonoMod;

namespace Celeste {
    class patch_Overworld : Overworld {

        /// <summary>
        /// Patch for Overworld.Update with null safety checks
        /// Prevents crashes from null references during scene updates
        /// </summary>
        [MonoModReplace]
        public override void Update() {
            try {
                // Call the original update with safety wrapping
                orig_Update();
            }
            catch (NullReferenceException ex) {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Overworld.Update] Caught null reference: {ex.Message}");
                Logger.Log(LogLevel.Verbose, "MaggyHelper",
                    $"[Overworld.Update] Stack: {ex.StackTrace}");

                // Continue without crashing
            }
            catch (Exception ex) {
                Logger.Log(LogLevel.Error, "MaggyHelper",
                    $"[Overworld.Update] Unexpected error: {ex.Message}");
                Logger.Log(LogLevel.Verbose, "MaggyHelper",
                    $"[Overworld.Update] Stack: {ex.StackTrace}");

                // Continue without crashing
            }
        }

        /// <summary>
        /// Safe access to current scene
        /// </summary>
        protected Scene SafeScene {
            get {
                try {
                    return Scene;
                }
                catch {
                    return null;
                }
            }
        }

        [MonoModIgnore]
        public extern void orig_Update();
    }
}
