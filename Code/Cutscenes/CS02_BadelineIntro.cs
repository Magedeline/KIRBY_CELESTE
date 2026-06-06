using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    [HotReloadable]
    public class CS02_BadelineIntro : CutsceneEntity
    {
        private BadelineOldsiteChaser badeline;
        private Player player;

        public CS02_BadelineIntro(BadelineOldsiteChaser badeline) : base(false, true)
        {
            this.badeline = badeline;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            player = level.Tracker.GetEntity<Player>();
            if (player == null)
                yield break;

            player.StateMachine.State = Player.StDummy;

            // Wait for intro animation
            yield return 2f;

            // Badeline appears and startles player
            Audio.Play("event:/char/badeline/appear");
            badeline.Visible = true;
            yield return 1f;

            // Set flag so intro doesn't play again
            level.Session.SetFlag("evil_maddy_intro", true);

            level.EndCutscene();
        }

        public override void OnEnd(Level level)
        {
            // Cutscene finished, allow player to move again
        }
    }
}
