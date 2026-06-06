local eventOptions = {
    "end_city",
    "end_oldsite_dream",
    "end_oldsite_awake",
    "ch5_see_theo",
    "ch5_found_theo",
    "ch5_mirror_reflection",
    "cancel_ch5_see_theo",
    "ch6_boss_intro",
    "ch6_reflect",
    "ch7_summit",
    "ch8_door",
    "ch9_goto_the_future",
    "ch9_goto_the_past",
    "ch9_moon_intro",
    "ch9_hub_intro",
    "ch9_hub_transition_out",
    "ch9_badeline_helps",
    "ch9_farewell",
    "ch9_ending",
    "ch9_end_golden",
    "ch9_final_room",
    "ch9_ding_ding_ding",
    "ch9_golden_snapshot"
}

local eventTrigger = {}

eventTrigger.name = "EventTrigger"
eventTrigger.canResize = {true, true}
eventTrigger.placements = {
    {
        name = "default",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            event = "",
            onSpawn = false
        }
    }
}

eventTrigger.fieldInformation = {
    event = {
        fieldType = "string",
        editable = true,
        options = eventOptions
    },
    onSpawn = {
        fieldType = "boolean"
    }
}

eventTrigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "event",
    "onSpawn"
}

function eventTrigger.color(room, entity)
    if entity.event == nil or entity.event == "" then
        return {0.85, 0.35, 0.35, 0.8}
    end

    return {0.35, 0.8, 0.95, 0.8}
end

function eventTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        {
            texture = "ahorn/entityTrigger",
            x = entity.x,
            y = entity.y,
            scaleX = width / 8,
            scaleY = height / 8,
            color = eventTrigger.color(room, entity)
        }
    }
end

function eventTrigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16

    return {
        entity.x,
        entity.y,
        width,
        height
    }
end

return eventTrigger