using System;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Component that handles Kirby skin swapping on the vanilla Player.
    /// When Kirby mode is active, swaps the player's PlayerSprite over to the
    /// Kirby sprite bank entry; when Kirby mode is inactive, restores the
    /// vanilla sprite that matches the player's current PlayerSpriteMode.
    /// </summary>
    public class KirbySkinController : Component
    {
        /// <summary>Sprite bank id used for the Kirby skin.</summary>
        public const string KirbySpriteId = "kirby_player";

        private global::Celeste.Player player;
        private bool kirbyModeActive;
        private bool spriteSwapped;
        private bool wasKirbyModeLastFrame;

        public KirbySkinController() : base(true, false) { }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            player = entity as global::Celeste.Player;
        }

        public override void Update()
        {
            if (player == null || player.Sprite == null)
                return;

            // Fast path: skip if Kirby mode hasn't changed since last frame
            var session = MaggyHelperModule.Session;
            bool isKirbyModeActive = session != null && session.IsKirbyModeActive;

            if (isKirbyModeActive == wasKirbyModeLastFrame && spriteSwapped == isKirbyModeActive)
                return;

            wasKirbyModeLastFrame = isKirbyModeActive;

            if (isKirbyModeActive != kirbyModeActive)
            {
                kirbyModeActive = isKirbyModeActive;
                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            if (kirbyModeActive)
            {
                if (!spriteSwapped)
                {
                    if (TryApplySprite(KirbySpriteId))
                        spriteSwapped = true;
                }
            }
            else
            {
                if (spriteSwapped)
                {
                    RestoreVanillaSprite();
                    spriteSwapped = false;
                }
            }
        }

        private void RestoreVanillaSprite()
        {
            if (player?.Sprite == null)
                return;

            // Recreate the vanilla sprite that matches the player's sprite mode.
            string spriteId = global::Celeste.PlayerSpriteModeExtensions.GetSpriteBankId(player.Sprite.Mode);
            TryApplySprite(spriteId);
        }

        /// <summary>
        /// Swaps the player's current PlayerSprite to the given bank id,
        /// preserving the current animation/frame where possible.
        /// </summary>
        private bool TryApplySprite(string spriteId)
        {
            if (player?.Sprite == null || string.IsNullOrEmpty(spriteId))
                return false;

            if (GFX.SpriteBank == null || !GFX.SpriteBank.Has(spriteId))
                return false;

            string currentAnim = player.Sprite.CurrentAnimationID;
            int currentFrame = player.Sprite.CurrentAnimationFrame;

            GFX.SpriteBank.CreateOn(player.Sprite, spriteId);

            if (!string.IsNullOrEmpty(currentAnim) && player.Sprite.Has(currentAnim))
            {
                player.Sprite.Play(currentAnim, restart: true, randomizeFrame: false);
                player.Sprite.SetAnimationFrame(currentFrame);
            }
            else if (player.Sprite.Has("idle"))
            {
                player.Sprite.Play("idle");
            }

            return true;
        }

        public override void Removed(Entity entity)
        {
            // Restore the vanilla sprite when this component is removed.
            if (spriteSwapped)
            {
                RestoreVanillaSprite();
                spriteSwapped = false;
            }

            base.Removed(entity);
        }
    }
}
