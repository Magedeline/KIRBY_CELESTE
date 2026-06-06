local badelineOldsiteChaser = {
    name = "MaggyHelper/BadelineOldsiteChaser",
    depth = -1,
    texture = "characters/badeline_oldsite/idle00",
    fieldInformation = {
        triggerIntro = {
            fieldType = "boolean"
        }
    },
    fieldOrder = {
        "x", "y",
        "triggerIntro"
    },
    placements = {
        {
            name = "BadelineOldsiteChaser",
            data = {
                triggerIntro = true
            }
        },
        {
            name = "BadelineOldsiteChaser (No Intro)",
            data = {
                triggerIntro = false
            }
        }
    }
}

return badelineOldsiteChaser
