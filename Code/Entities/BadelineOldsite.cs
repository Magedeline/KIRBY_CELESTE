using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Cutscenes;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [CustomEntity(ids: "MaggyHelper/BadelineOldsiteChaser")]
    [Tracked]
    [HotReloadable]
    public class BadelineOldsiteChaser : Entity
    {
        public static Color HairColor = Calc.HexToColor("FFD700");

        private Sprite sprite;
        private PlayerHair hair;
        private bool hovering;
        private bool triggerIntro = true;

        public BadelineOldsiteChaser(Vector2 position) : base(position)
        {
            Depth = -1;
            Collider = new Hitbox(6f, 6f, -3f, -7f);
            sprite = GFX.SpriteBank.Create("badeline_oldsite");
            sprite.Play("fallSlow", restart: true);
            hair = null;
            Add(sprite);
            Visible = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public BadelineOldsiteChaser(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
            triggerIntro = data.Bool("triggerIntro", defaultValue: true);
        }

        public bool Hovering
        {
            get { return hovering; }
            set { hovering = value; }
        }

        public Sprite Sprite
        {
            get { return sprite; }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = SceneAs<Level>();
            Session session = level.Session;

            if (session.GetLevelFlag("11") && session.Area.Mode == AreaMode.Normal)
            {
                RemoveSelf();
                return;
            }

            if (!session.GetLevelFlag("3") && session.Area.Mode == AreaMode.Normal)
            {
                RemoveSelf();
                return;
            }

            if (triggerIntro && !session.GetFlag("evil_maddy_intro") && session.Level == "3" && session.Area.Mode == AreaMode.Normal)
            {
                Hovering = false;
                Visible = true;
                if (hair != null) hair.Visible = false;
                sprite.Play("pretendDead", false, false);
                if (session.Area.Mode == AreaMode.Normal)
                {
                    session.Audio.Music.Event = null;
                    session.Audio.Apply(false);
                }
                scene.Add(new global::Celeste.Cutscenes.CS02_BadelineIntro(this));
                return;
            }

            Add(new Coroutine(StartChasingRoutine(level), true));
        }

        private IEnumerator StartChasingRoutine(Level level)
        {
            Hovering = true;
            while (true)
            {
                yield return null;
            }
        }
    }
}
