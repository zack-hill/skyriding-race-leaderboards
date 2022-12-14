AppName = "Dragonriding Race Leaderboards"
DRL = LibStub("AceAddon-3.0"):NewAddon(AppName, "AceEvent-3.0")

local lastQuestAccepted = ""
local listenForRaceTime = false
local raceName = ""
local raceTime = ""

local options = {
    name = AppName,
    handler = DRL,
    type = 'group',
    args = {
        communityName = {
            type = 'input',
            name = 'Community Name',
            desc = 'The name of the community where race times will be posted.',
            set = 'SetCommunityName',
            get = 'GetCommunityName',
            order = 1,
        },
        channelName = {
            type = 'input',
            name = 'Channel Name',
            desc = 'The name of the channel in the community where race times will be posted.',
            set = 'SetChannelName',
            get = 'GetChannelName',
            order = 2,
        },
    },
}

function DRL:OnInitialize()
    LibStub("AceConfig-3.0"):RegisterOptionsTable(AppName, options, nil)
    LibStub("AceConfigDialog-3.0"):AddToBlizOptions(AppName, AppName)

    DRL:RegisterEvent("CHAT_MSG_SYSTEM")
    DRL:RegisterEvent("CHAT_MSG_MONSTER_SAY")
    DRL:RegisterEvent("UNIT_AURA")
end

function DRL:OnEnable()    
    -- Get the club id
    local clubId = -1
    local clubs = C_Club.GetSubscribedClubs()
    for i = 1, #clubs, 1 do
        if clubs[i].name == CommunityName then
            clubId = clubs[i].clubId
        end
    end
    print(clubId)

    -- Get the stream id
    local streamId = -1
    streams = C_Club.GetStreams(clubId)
    for i = 1, #streams, 1 do
        if streams[i].name == ChannelName then
            streamId = streams[i].streamId
        end
    end
    print(streamId)

    -- Get location information
    local zone = GetZoneText()
    local y, x, _, instance1 = UnitPosition("player")

    start_message = "Starting a race in "..zone.." at "..x..", "..y
    macroText = "/run C_Club.SendMessage("..clubId..","..streamId..",\""..start_message.."\")"
    print(macroText)

    buttonName = "SubmitButton"
    button = CreateFrame("Button", buttonName, UIParent, "SecureActionButtonTemplate")

    button:SetPoint("CENTER", mainframe, "CENTER", -500, -500)
    button:SetWidth(130)
    button:SetHeight(30)

    button:SetText("Submit Race Time")
    button:SetNormalFontObject("GameFontNormal")

    local ntex = button:CreateTexture()
    ntex:SetTexture("Interface/Buttons/UI-Panel-Button-Up")
    ntex:SetTexCoord(0, 0.625, 0, 0.6875)
    ntex:SetAllPoints()	
    button:SetNormalTexture(ntex)

    local htex = button:CreateTexture()
    htex:SetTexture("Interface/Buttons/UI-Panel-Button-Highlight")
    htex:SetTexCoord(0, 0.625, 0, 0.6875)
    htex:SetAllPoints()
    button:SetHighlightTexture(htex)

    local ptex = button:CreateTexture()
    ptex:SetTexture("Interface/Buttons/UI-Panel-Button-Down")
    ptex:SetTexCoord(0, 0.625, 0, 0.6875)
    ptex:SetAllPoints()
    button:SetPushedTexture(ptex)

    button:SetAttribute("type", "macro")
    button:SetAttribute("macrotext", macroText)
    button:RegisterForClicks("LeftButtonDown", "LeftButtonUp")

    -- local status = SetBindingClick("W", buttonName, "LeftButton 1")
end

function DRL:OnDisable()
    -- Called when the addon is disabled
end

function DRL:CHAT_MSG_SYSTEM(event, text)
    local questAcceptedPrefix = "Quest accepted: "
    if #text >= #questAcceptedPrefix and string.sub(text, 1, #questAcceptedPrefix) == questAcceptedPrefix then
        lastQuestAccepted = string.sub(text, #questAcceptedPrefix, #text)
    end
end

function DRL:CHAT_MSG_MONSTER_SAY(event, text, name)
    if listenForRaceTime and name == "Bronze Timekeeper" then
        local pattern = "Your race time was (%d+\.%d+) seconds"
        local _, _, timeString = string.find(text, pattern)
        if timeString ~= nil then
            print("Race time was " .. timeString)
            listenForRaceTime = false
        end
    end
end

function DRL:UNIT_AURA(_, target, info)
    if target == "player" then
        for _, aura in ipairs(info.addedAuras or {}) do
            local raceStartingSpellId = 382632
            if aura.spellId == raceStartingSpellId then
                raceName = lastQuestAccepted
                listenForRaceTime = true
            end
        end
    end
end

function DRL:GetCommunityName(info)
    return CommunityName
end

function DRL:SetCommunityName(info, input)
    CommunityName = input
end

function DRL:GetChannelName(info)
    return ChannelName
end

function DRL:SetChannelName(info, input)
    ChannelName = input
end