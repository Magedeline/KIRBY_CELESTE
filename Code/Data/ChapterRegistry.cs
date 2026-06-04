using Microsoft.Xna.Framework;

namespace Celeste;

/// <summary>
/// Separated chapter data and registration helper to reduce AreaMapData file size.
/// This factory method handles all chapter definitions so AreaMapData can focus on management.
/// </summary>
internal static class ChapterRegistry
{
    /// <summary>Register all chapters into the provided list</summary>
    public static void RegisterAllChapters(List<AreaMapData.ChapterDef> chapters)
    {
        // Prologue (Chapter 0)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 0, Name = "prolouge", SID = AreaModeExtender.BuildASideSID("00_Prologue"),
            Icon = "areas/prologue", IsInterlude = true,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "guids://e3205ef9-4d54-5f55-8a68-89cc87b5ee78" },
            AmbienceEvents = new[] { "guid://{0fa79a65-c103-4f72-84d6-7a22c9b2f43c}" },
            MountainState = 0,
            MountainData = new AreaMapData.MountainCameraData
            {
                IdlePos = new Vector3(-1.374f, 1.224f, 9.371f),
                IdleTarget = new Vector3(-0.440f, 0.499f, 7.758f),
                SelectPos = new Vector3(-1.374f, 1.224f, 7.971f),
                SelectTarget = new Vector3(-0.440f, 0.499f, 6.358f),
                ZoomPos = new Vector3(-1.007f, 0.862f, 6.965f),
                ZoomTarget = new Vector3(-0.073f, 0.137f, 5.352f),
                Cursor = new Vector3(-0.440f, 0.499f, 6.358f)
            }
        });

        // Chapter 1: Forbidden Metropolis
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 1, Name = "forbiddenmetro", SID = AreaModeExtender.BuildASideSID("01_City"),
            Icon = "areas/city", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://11126cb5-16ce-54ca-93a1-c632ed99ed38",
                "guids://11126cb5-16ce-54ca-93a1-c632ed99ed38",
                "guids://11126cb5-16ce-54ca-93a1-c632ed99ed38",
                "guids://2486dd1a-55f7-537e-87f2-2082245b3186",
                "guids://11126cb5-16ce-54ca-93a1-c632ed99ed38",
            },
            AmbienceEvents = BuildAmbienceArray(1, 5),
            CassetteSong = "guids://11126cb5-16ce-54ca-93a1-c632ed99ed38",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-1.234f, 0.677f, 9.198f), new Vector3(-0.221f, 0.734f, 7.475f),
                new Vector3(-1.234f, 0.677f, 7.598f), new Vector3(-0.221f, 0.734f, 5.875f),
                new Vector3(-0.867f, 0.315f, 6.592f), new Vector3(0.146f, 0.372f, 4.869f),
                new Vector3(-0.221f, 0.734f, 5.875f)
            )
        });

        // Chapter 2: Veil of Shadows
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 2, Name = "shadowofveil", SID = AreaModeExtender.BuildASideSID("02_Nightmare"),
            Icon = "areas/nightmare", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://df05601d-d0a3-5586-bb07-6f20100c5945",
                "guids://df05601d-d0a3-5586-bb07-6f20100c5945",
                "guids://df05601d-d0a3-5586-bb07-6f20100c5945",
                "guids://200d0812-130b-500d-8374-c016c031282e",
                "guids://df05601d-d0a3-5586-bb07-6f20100c5945",
            },
            AmbienceEvents = BuildAmbienceArray(2, 5),
            CassetteSong = "guids://df05601d-d0a3-5586-bb07-6f20100c5945",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-0.952f, 4.218f, 11.344f), new Vector3(-0.111f, 3.393f, 9.728f),
                new Vector3(-0.952f, 4.218f, 9.744f), new Vector3(-0.111f, 3.393f, 8.128f),
                new Vector3(-0.585f, 3.856f, 8.738f), new Vector3(0.256f, 3.031f, 7.122f),
                new Vector3(-0.111f, 3.393f, 8.128f)
            )
        });

        // Chapter 3: Arrival
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 3, Name = "arrivial", SID = AreaModeExtender.BuildASideSID("03_Stars"),
            Icon = "areas/stars", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://8c09bbe3-34af-5ca5-8b08-4a90cca4ac7b",
                "guids://8c09bbe3-34af-5ca5-8b08-4a90cca4ac7b",
                "guids://8c09bbe3-34af-5ca5-8b08-4a90cca4ac7b",
                "guids://d2245146-277c-5d9d-bf13-dcb4c7a91f28",
                "guids://8c09bbe3-34af-5ca5-8b08-4a90cca4ac7b",
            },
            AmbienceEvents = BuildAmbienceArray(3, 5),
            CassetteSong = "guids://8c09bbe3-34af-5ca5-8b08-4a90cca4ac7b",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-3.431f, 5.512f, 5.884f), new Vector3(-2.287f, 4.359f, 4.718f),
                new Vector3(-3.431f, 5.512f, 4.284f), new Vector3(-2.287f, 4.359f, 3.118f),
                new Vector3(-3.064f, 5.150f, 3.278f), new Vector3(-1.920f, 3.997f, 2.112f),
                new Vector3(-2.287f, 4.359f, 3.118f)
            )
        });

        // Chapter 4: Chronicles of Destiny
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 4, Name = "thelegend", SID = AreaModeExtender.BuildASideSID("04_Legend"),
            Icon = "areas/legend", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://94892270-7e5a-53f5-a7a8-90eec18d94be",
                "guids://94892270-7e5a-53f5-a7a8-90eec18d94be",
                "guids://94892270-7e5a-53f5-a7a8-90eec18d94be",
                "guids://13f2acf1-94ae-563c-ad01-44b70d94762e",
                "guids://94892270-7e5a-53f5-a7a8-90eec18d94be",
            },
            AmbienceEvents = BuildAmbienceArray(4, 5),
            CassetteSong = "guids://94892270-7e5a-53f5-a7a8-90eec18d94be",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-14.620f, 3.606f, 20.735f), new Vector3(-13.134f, 4.115f, 19.497f),
                new Vector3(-14.620f, 3.606f, 19.135f), new Vector3(-13.134f, 4.115f, 17.897f),
                new Vector3(-14.253f, 3.244f, 18.129f), new Vector3(-12.767f, 3.753f, 16.891f),
                new Vector3(-13.134f, 4.115f, 17.897f)
            )
        });

        // Chapter 5: Fractured Memories
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 5, Name = "fractureresort", SID = AreaModeExtender.BuildASideSID("05_Restore"),
            Icon = "areas/restore", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://3629aeef-9623-57e0-9414-42e15734a66b",
                "guids://3629aeef-9623-57e0-9414-42e15734a66b",
                "guids://3629aeef-9623-57e0-9414-42e15734a66b",
                "guids://c94b7e43-0661-5b9c-93f6-00363906d491",
                "guids://3629aeef-9623-57e0-9414-42e15734a66b",
            },
            AmbienceEvents = BuildAmbienceArray(5, 5),
            CassetteSong = "guids://3629aeef-9623-57e0-9414-42e15734a66b",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-4.473f, 7.158f, 7.063f), new Vector3(-3.630f, 6.660f, 5.319f),
                new Vector3(-4.473f, 7.158f, 5.463f), new Vector3(-3.630f, 6.660f, 3.719f),
                new Vector3(-4.106f, 6.796f, 4.457f), new Vector3(-3.263f, 6.298f, 2.713f),
                new Vector3(-3.630f, 6.660f, 3.719f)
            )
        });

        // Chapter 6: Fortress of Solitude
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 6, Name = "stronghold", SID = AreaModeExtender.BuildASideSID("06_Stronghold"),
            Icon = "areas/stronghold", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://599bebe0-b8ec-5bd4-abe0-86b9bffc51e4",
                "guids://599bebe0-b8ec-5bd4-abe0-86b9bffc51e4",
                "guids://599bebe0-b8ec-5bd4-abe0-86b9bffc51e4",
                "guids://b4b9646a-2dde-529f-a09d-981668b1d559",
                "guids://599bebe0-b8ec-5bd4-abe0-86b9bffc51e4",
            },
            AmbienceEvents = BuildAmbienceArray(6, 5),
            CassetteSong = "guids://599bebe0-b8ec-5bd4-abe0-86b9bffc51e4",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(5.961f, 8.823f, 6.658f), new Vector3(5.061f, 7.757f, 5.225f),
                new Vector3(5.961f, 8.823f, 5.058f), new Vector3(5.061f, 7.757f, 3.625f),
                new Vector3(6.328f, 8.461f, 4.052f), new Vector3(5.428f, 7.395f, 2.619f),
                new Vector3(5.061f, 7.757f, 3.625f)
            )
        });

        // Chapter 7: Infernal Reflections
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 7, Name = "infornoreflection", SID = AreaModeExtender.BuildASideSID("07_Hell"),
            Icon = "areas/hell", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://12f4a235-6219-5c9d-b800-b7ddcb0b9fc3",
                "guids://12f4a235-6219-5c9d-b800-b7ddcb0b9fc3",
                "guids://12f4a235-6219-5c9d-b800-b7ddcb0b9fc3",
                "guids://ac20d78e-aa50-5a5b-a03c-39debcb7e1ca",
                "guids://12f4a235-6219-5c9d-b800-b7ddcb0b9fc3",
            },
            AmbienceEvents = BuildAmbienceArray(7, 5),
            CassetteSong = "guids://12f4a235-6219-5c9d-b800-b7ddcb0b9fc3",
            MountainState = MountainOverworldManager.STATE_DARK,
            MountainData = CreateMountainCamera(
                new Vector3(9.626f, 8.824f, -2.540f), new Vector3(7.924f, 8.240f, -1.667f),
                new Vector3(9.626f, 8.824f, -4.140f), new Vector3(7.924f, 8.240f, -3.267f),
                new Vector3(9.993f, 8.462f, -5.146f), new Vector3(8.291f, 7.878f, -4.273f),
                new Vector3(7.924f, 8.240f, -3.267f)
            )
        });

        // Chapter 8: Revelation's Edge
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 8, Name = "revelationedge", SID = AreaModeExtender.BuildASideSID("08_Truth"),
            Icon = "areas/truth", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://3872c1d3-6028-5a7a-be49-a76dde063a07",
                "guids://3872c1d3-6028-5a7a-be49-a76dde063a07",
                "guids://3872c1d3-6028-5a7a-be49-a76dde063a07",
                "guids://ef444bb9-21d7-5900-88ae-6ff9944c340e",
                "guids://3872c1d3-6028-5a7a-be49-a76dde063a07",
            },
            AmbienceEvents = BuildAmbienceArray(8, 5),
            CassetteSong = "guids://3872c1d3-6028-5a7a-be49-a76dde063a07",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-0.963f, 10.542f, -3.714f), new Vector3(-0.178f, 9.588f, -2.141f),
                new Vector3(-0.963f, 10.542f, -5.314f), new Vector3(-0.178f, 9.588f, -3.741f),
                new Vector3(-0.596f, 10.180f, -6.320f), new Vector3(0.189f, 9.226f, -4.747f),
                new Vector3(-0.178f, 9.588f, -3.741f)
            )
        });

        // Chapter 9: Apex of Reality (Summit)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 9, Name = "beyondsummit", SID = AreaModeExtender.BuildASideSID("09_Summit"),
            Icon = "areas/summit", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://a9e018fa-5f30-5eca-853b-9d5d9680e478",
                "guids://a9e018fa-5f30-5eca-853b-9d5d9680e478",
                "guids://a9e018fa-5f30-5eca-853b-9d5d9680e478",
                "guids://f7dcf3ba-bf0e-5c18-81db-d7ba9bd7b85c",
                "guids://a9e018fa-5f30-5eca-853b-9d5d9680e478",
            },
            AmbienceEvents = BuildAmbienceArray(9, 5),
            CassetteSong = "guids://a9e018fa-5f30-5eca-853b-9d5d9680e478",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(1.113f, 12.154f, 7.934f), new Vector3(-0.086f, 11.118f, 6.715f),
                new Vector3(1.113f, 12.154f, 6.334f), new Vector3(-0.086f, 11.118f, 5.115f),
                new Vector3(1.480f, 11.792f, 5.328f), new Vector3(0.281f, 10.756f, 4.109f),
                new Vector3(-0.086f, 11.118f, 5.115f)
            )
        });

        // Chapter 10: Echoes of the Past
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 10, Name = "echosofpast", SID = AreaModeExtender.BuildASideSID("10_Ruins"),
            Icon = "areas/ruins", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://bb50a731-2b3f-59a3-913d-dabc77f9e7f3",
                "guids://bb50a731-2b3f-59a3-913d-dabc77f9e7f3",
                "guids://bb50a731-2b3f-59a3-913d-dabc77f9e7f3",
                "guids://94b376f7-30d6-5952-95e4-ddfd2e10105f",
                "guids://bb50a731-2b3f-59a3-913d-dabc77f9e7f3",
            },
            AmbienceEvents = BuildAmbienceArray(10, 5),
            CassetteSong = "guids://bb50a731-2b3f-59a3-913d-dabc77f9e7f3",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(0.514f, 14.102f, 8.460f), new Vector3(-0.462f, 13.157f, 6.891f),
                new Vector3(0.514f, 14.102f, 7.860f), new Vector3(-0.462f, 13.157f, 6.291f),
                new Vector3(0.881f, 13.740f, 6.854f), new Vector3(-0.095f, 12.795f, 5.285f),
                new Vector3(-0.462f, 13.157f, 6.291f)
            )
        });

        // Chapter 11: Frozen Sanctuary
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 11, Name = "frozensanctuary", SID = AreaModeExtender.BuildASideSID("11_Snow"),
            Icon = "areas/snow", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://bb53c6a3-352a-5655-bd2a-cad4b90550aa",
                "guids://bb53c6a3-352a-5655-bd2a-cad4b90550aa",
                "guids://bb53c6a3-352a-5655-bd2a-cad4b90550aa",
                "guids://cada4478-8931-51ad-b1a1-d2745f63536e",
                "guids://bb53c6a3-352a-5655-bd2a-cad4b90550aa",
            },
            AmbienceEvents = BuildAmbienceArray(11, 5),
            CassetteSong = "guids://bb53c6a3-352a-5655-bd2a-cad4b90550aa",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-0.185f, 16.051f, 9.185f), new Vector3(-0.838f, 15.230f, 7.315f),
                new Vector3(-0.185f, 16.051f, 8.585f), new Vector3(-0.838f, 15.230f, 6.715f),
                new Vector3(0.182f, 15.689f, 7.579f), new Vector3(-0.471f, 14.868f, 5.709f),
                new Vector3(-0.838f, 15.230f, 6.715f)
            )
        });

        // Chapter 12: Cascading Depths
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 12, Name = "cascadingdepths", SID = AreaModeExtender.BuildASideSID("12_Water"),
            Icon = "areas/water", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://ca164be9-bd45-5d77-8ab7-2a649ec4e204",
                "guids://ca164be9-bd45-5d77-8ab7-2a649ec4e204",
                "guids://ca164be9-bd45-5d77-8ab7-2a649ec4e204",
                "guids://a5132074-5417-5829-adf4-7832c25fa02c",
                "guids://ca164be9-bd45-5d77-8ab7-2a649ec4e204",
            },
            AmbienceEvents = BuildAmbienceArray(12, 5),
            CassetteSong = "guids://ca164be9-bd45-5d77-8ab7-2a649ec4e204",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-0.884f, 18.000f, 9.910f), new Vector3(-1.214f, 17.303f, 7.539f),
                new Vector3(-0.884f, 18.000f, 9.310f), new Vector3(-1.214f, 17.303f, 6.939f),
                new Vector3(-0.517f, 17.638f, 8.304f), new Vector3(-0.847f, 16.941f, 5.933f),
                new Vector3(-1.214f, 17.303f, 6.939f)
            )
        });

        // Chapter 13: Blazing Territories
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 13, Name = "balzingteritory", SID = AreaModeExtender.BuildASideSID("13_Fire"),
            Icon = "areas/fire", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://2ab750b7-db73-586c-b806-69f97a427998",
                "guids://2ab750b7-db73-586c-b806-69f97a427998",
                "guids://2ab750b7-db73-586c-b806-69f97a427998",
                "guids://7bbb451e-ebc6-51a0-9438-99a7a8a14b1d",
                "guids://2ab750b7-db73-586c-b806-69f97a427998",
            },
            AmbienceEvents = BuildAmbienceArray(13, 5),
            CassetteSong = "guids://2ab750b7-db73-586c-b806-69f97a427998",
            MountainState = MountainOverworldManager.STATE_DARK,
            MountainData = CreateMountainCamera(
                new Vector3(-1.583f, 19.949f, 10.635f), new Vector3(-1.590f, 19.376f, 8.163f),
                new Vector3(-1.583f, 19.949f, 10.035f), new Vector3(-1.590f, 19.376f, 7.563f),
                new Vector3(-1.216f, 19.587f, 9.029f), new Vector3(-1.223f, 19.014f, 6.557f),
                new Vector3(-1.590f, 19.376f, 7.563f)
            )
        });

        // Chapter 14: Cyber Nexus
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 14, Name = "cybernexus", SID = AreaModeExtender.BuildASideSID("14_Digital"),
            Icon = "areas/digital", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://3459e7af-1dba-5ade-b65e-a88d92222e80",
                "guids://3459e7af-1dba-5ade-b65e-a88d92222e80",
                "guids://3459e7af-1dba-5ade-b65e-a88d92222e80",
                "guids://0791d94f-4c22-55fb-a38f-e48e1f5663c6",
                "guids://3459e7af-1dba-5ade-b65e-a88d92222e80",
            },
            AmbienceEvents = BuildAmbienceArray(14, 5),
            CassetteSong = "guids://3459e7af-1dba-5ade-b65e-a88d92222e80",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-1.660f, 22.498f, 10.160f), new Vector3(-1.966f, 21.449f, 7.987f),
                new Vector3(-1.660f, 22.498f, 9.560f), new Vector3(-1.966f, 21.449f, 7.387f),
                new Vector3(-1.293f, 22.136f, 8.554f), new Vector3(-1.599f, 21.087f, 6.381f),
                new Vector3(-1.966f, 21.449f, 7.387f)
            )
        });

        // Chapter 15: Ethereal Citadel
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 15, Name = "etheraealcitadel", SID = AreaModeExtender.BuildASideSID("15_Castle"),
            Icon = "areas/castle", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://552a4028-6a86-578f-babc-a6efae9e4c92",
                "guids://552a4028-6a86-578f-babc-a6efae9e4c92",
                "guids://552a4028-6a86-578f-babc-a6efae9e4c92",
                "guids://29d834bd-fd84-51c9-af75-116981a1c1b2",
                "guids://552a4028-6a86-578f-babc-a6efae9e4c92",
            },
            AmbienceEvents = BuildAmbienceArray(15, 5),
            CassetteSong = "guids://552a4028-6a86-578f-babc-a6efae9e4c92",
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-1.737f, 25.047f, 9.685f), new Vector3(-1.722f, 24.522f, 7.411f),
                new Vector3(-1.737f, 25.047f, 9.085f), new Vector3(-1.722f, 24.522f, 6.811f),
                new Vector3(-1.370f, 24.685f, 8.079f), new Vector3(-1.355f, 24.160f, 5.805f),
                new Vector3(-1.722f, 24.522f, 6.811f)
            )
        });

        // Chapter 16: Organ Garden of Despair
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 16, Name = "organgarden", SID = AreaModeExtender.BuildASideSID("16_Corruption"),
            Icon = "areas/corruption", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://bc1a59f4-dad1-5d9f-956f-414dabc8227f",
                "guids://bc1a59f4-dad1-5d9f-956f-414dabc8227f",
                "guids://bc1a59f4-dad1-5d9f-956f-414dabc8227f",
                "event:/music/pusheen/DSide/16_corruption",
                "guids://bc1a59f4-dad1-5d9f-956f-414dabc8227f",
            },
            AmbienceEvents = new[] {
                "event:/env/amb/16_main",
                "event:/env/amb/16_main",
                "event:/env/amb/16_main",
                "event:/env/amb/16_main",
                "event:/env/amb/16_main",
            },
            CassetteSong = "guids://bc1a59f4-dad1-5d9f-956f-414dabc8227f",
            MountainState = MountainOverworldManager.STATE_DARK,
            MountainData = CreateMountainCamera(
                new Vector3(-1.916f, 33.050f, 11.185f), new Vector3(-1.479f, 32.938f, 9.236f),
                new Vector3(-1.916f, 33.050f, 9.585f), new Vector3(-1.479f, 32.938f, 7.636f),
                new Vector3(-1.549f, 32.688f, 8.579f), new Vector3(-1.112f, 32.576f, 6.630f),
                new Vector3(-1.479f, 32.938f, 7.636f)
            )
        });

        // Chapter 17: Epilogue (A-Side only)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 17, Name = "epilouge", SID = AreaModeExtender.BuildASideSID("17_Epilogue"),
            Icon = "areas/epilogue", IsInterlude = true,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "guids://0dbe6cae-8f5e-5301-b724-de4431091bc1" },
            AmbienceEvents = new[] { "event:/env/amb/00_main" },
            MountainState = 0,
            MountainData = CreateMountainCamera(
                new Vector3(-1.916f, 35.450f, 11.185f), new Vector3(-1.479f, 35.338f, 9.236f),
                new Vector3(-1.916f, 35.450f, 9.585f), new Vector3(-1.479f, 35.338f, 7.636f),
                new Vector3(-1.549f, 35.088f, 8.579f), new Vector3(-1.112f, 34.976f, 6.630f),
                new Vector3(-1.479f, 35.338f, 7.636f)
            )
        });

        // Chapter 18: Core of Existence
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 18, Name = "coreexistence", SID = AreaModeExtender.BuildASideSID("18_Heart"),
            Icon = "areas/heart", IsInterlude = false,
            HasBSide = true, HasCSide = true, HasDSide = true, HasDXSide = false,
            MusicEvents = new[] {
                "guids://1f3f4a27-523e-5703-bef0-0c24dafc8463",
                "guids://1f3f4a27-523e-5703-bef0-0c24dafc8463",
                "guids://1f3f4a27-523e-5703-bef0-0c24dafc8463",
                "guids://5123f7cb-4b7c-50f3-b20d-85cde0bcccad",
                "guids://1f3f4a27-523e-5703-bef0-0c24dafc8463",
            },
            AmbienceEvents = BuildAmbienceArray(18, 5),
            CassetteSong = "guids://1f3f4a27-523e-5703-bef0-0c24dafc8463",
            MountainState = MountainOverworldManager.STATE_DARK,
            MountainData = CreateMountainCamera(
                new Vector3(-1.916f, 37.850f, 11.185f), new Vector3(-1.479f, 37.738f, 9.236f),
                new Vector3(-1.916f, 37.850f, 9.585f), new Vector3(-1.479f, 37.738f, 7.636f),
                new Vector3(-1.549f, 37.488f, 8.579f), new Vector3(-1.112f, 37.376f, 6.630f),
                new Vector3(-1.479f, 37.738f, 7.636f)
            )
        });

        // Chapter 19: Farewell to Stars (A-Side only)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 19, Name = "farewellstar", SID = AreaModeExtender.BuildASideSID("19_Space"),
            Icon = "areas/space", IsInterlude = false,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "guids://8beee91f-5197-5d68-b855-00a6de23a555" },
            AmbienceEvents = new[] { "event:/env/amb/18_main" },
            MountainState = MountainOverworldManager.STATE_VOID,
            MountainData = CreateMountainCamera(
                new Vector3(-1.916f, 40.250f, 11.185f), new Vector3(-1.479f, 40.138f, 9.236f),
                new Vector3(-1.916f, 40.250f, 9.585f), new Vector3(-1.479f, 40.138f, 7.636f),
                new Vector3(-1.549f, 39.888f, 8.579f), new Vector3(-1.112f, 39.776f, 6.630f),
                new Vector3(-1.479f, 40.138f, 7.636f)
            )
        });

        // Chapter 20: The Last Push (A-Side only)
        Register(chapters, new AreaMapData.ChapterDef
        {
            Number = 20, Name = "lastpush", SID = AreaModeExtender.BuildASideSID("20_TheEnd"),
            Icon = "areas/theend", IsInterlude = false,
            HasBSide = false, HasCSide = false, HasDSide = false, HasDXSide = false,
            MusicEvents = new[] { "event:/" },
            AmbienceEvents = new[] { "event:/" },
            MountainState = MountainOverworldManager.STATE_DARK,
            MountainData = CreateMountainCamera(
                new Vector3(-1.916f, 42.650f, 11.185f), new Vector3(-1.479f, 42.538f, 9.236f),
                new Vector3(-1.916f, 42.650f, 9.585f), new Vector3(-1.479f, 42.538f, 7.636f),
                new Vector3(-1.549f, 42.288f, 8.579f), new Vector3(-1.112f, 42.176f, 6.630f),
                new Vector3(-1.479f, 42.538f, 7.636f)
            )
        });

        Logger.Log(LogLevel.Info, "MaggyHelper",
            $"Registered {chapters.Count} chapters in AreaMapData");
    }

    private static void Register(List<AreaMapData.ChapterDef> chapters, AreaMapData.ChapterDef chapter)
    {
        chapters.Add(chapter);
    }

    private static string[] BuildAmbienceArray(int chapterNumber, int count)
    {
        var result = new string[count];
        for (int i = 0; i < count; i++)
            result[i] = $"event:/env/amb/{chapterNumber:D2}_main";
        return result;
    }

    private static AreaMapData.MountainCameraData CreateMountainCamera(
        Vector3 idlePos, Vector3 idleTarget,
        Vector3 selectPos, Vector3 selectTarget,
        Vector3 zoomPos, Vector3 zoomTarget,
        Vector3 cursor)
    {
        return new AreaMapData.MountainCameraData
        {
            IdlePos = idlePos,
            IdleTarget = idleTarget,
            SelectPos = selectPos,
            SelectTarget = selectTarget,
            ZoomPos = zoomPos,
            ZoomTarget = zoomTarget,
            Cursor = cursor
        };
    }
}
