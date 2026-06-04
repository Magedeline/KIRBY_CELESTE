using Celeste.Cutscenes;

namespace Celeste;

/// <summary>
/// Hooks into the LevelEnter flow to show VHS intro remix cutscenes
/// when entering B-Side or C-Side levels for the first time.
/// </summary>
public static class IntroRemixHooks
{
    private static bool _hooked = false;

    /// <summary>
    /// Hookable delegate for D-Side chapter entry.
    /// Subscribe to this to customize D-Side intro behavior.
    /// Return true to skip default entry (handled by subscriber).
    /// </summary>
    public delegate bool DSideEnterHandler(Session session);

    /// <summary>
    /// Event invoked when entering a D-Side chapter.
    /// Hook this from anywhere in AreaMapData or other classes to customize entry.
    /// </summary>
    public static event DSideEnterHandler OnDSideEnter;

    public static void Load()
    {
        if (_hooked)
            return;

        _hooked = true;
        On.Celeste.LevelEnter.Go += OnLevelEnterGo;

        Logger.Log(LogLevel.Info, "MaggyHelper", "IntroRemixHooks loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;
        On.Celeste.LevelEnter.Go -= OnLevelEnterGo;

        Logger.Log(LogLevel.Info, "MaggyHelper", "IntroRemixHooks unloaded");
    }

    /// <summary>
    /// Intercepts level entry to show VHS remix intros for B-Side and C-Side.
    /// </summary>
    private static void OnLevelEnterGo(On.Celeste.LevelEnter.orig_Go orig,
        Session session, bool fromSaveData)
    {
        if (fromSaveData || !session.StartedFromBeginning)
        {
            orig(session, fromSaveData);
            return;
        }

        var area = AreaData.Get(session.Area);
        if (!AreaModeExtender.IsOurMap(area))
        {
            orig(session, fromSaveData);
            return;
        }

        int mode = (int)session.Area.Mode;

        switch (mode)
        {
            case AreaModeExtender.MODE_BSIDE:
                if (ShouldShowRemixIntro(session, mode))
                {
                    Engine.Scene = new CS_Gen_IntroRemix_BSide(session);
                    return;
                }
                break;

            case AreaModeExtender.MODE_CSIDE:
                if (ShouldShowRemixIntro(session, mode))
                {
                    Engine.Scene = new CS_Gen_IntroRemix_CSide(session);
                    return;
                }
                break;

            case AreaModeExtender.MODE_DSIDE:
                if (OnDSideEnter != null)
                {
                    bool handled = false;
                    foreach (DSideEnterHandler handler in OnDSideEnter.GetInvocationList())
                    {
                        if (handler(session))
                        {
                            handled = true;
                            break;
                        }
                    }
                    if (handled)
                        return;
                }
                break;
        }

        orig(session, fromSaveData);
    }

    /// <summary>
    /// Determines if the VHS remix intro should be shown.
    /// Shows on first entry or if the player hasn't seen it before.
    /// </summary>
    private static bool ShouldShowRemixIntro(Session session, int mode)
    {
        string flagKey = $"seen_remix_intro_{session.Area.SID}_{mode}";
        bool alreadySeen = MaggyHelperModule.SaveData?.HasAchievement(flagKey) == true;

        if (!alreadySeen)
        {
            MaggyHelperModule.SaveData?.UnlockAchievement(flagKey);
            return true;
        }

        return false;
    }
}
