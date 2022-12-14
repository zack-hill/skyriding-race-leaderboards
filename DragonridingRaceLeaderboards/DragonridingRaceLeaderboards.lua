AppName = "Dragonriding Race Leaderboards"
DRL = LibStub("AceAddon-3.0"):NewAddon(AppName, "AceConsole-3.0", "AceEvent-3.0")

local lastQuestAccepted = ""
local listenForRaceTime = false
local raceName = ""
local raceTime = ""
local raceSubmitButtonName = "RaceTimeSubmitButton"
local raceSubmitButtonFrame = nil

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

    DRL:RegisterChatCommand("readchat", "ReadChat")
    DRL:RegisterEvent("CHAT_MSG_SYSTEM")
    DRL:RegisterEvent("CHAT_MSG_MONSTER_SAY")
    DRL:RegisterEvent("UNIT_AURA")
end

function DRL:ReadChat()
    local clubId = GetClubId()
    local streamId = GetStreamId(clubId)
    local ranges = C_Club.GetMessageRanges(clubId, streamId)
    for i = 1, #ranges, 1 do
        local range = ranges[i]
        local messages = C_Club.GetMessagesInRange(clubId, streamId, range.oldestMessageId, range.newestMessageId)
        for j = 1, #messages, 1 do
            print(messages[j].content)
        end
    end 
end


function DRL:OnEnable()
    raceSubmitButtonFrame = CreateFrame("Button", raceSubmitButtonName, UIParent, "SecureActionButtonTemplate")
    raceSubmitButtonFrame:Hide()
    raceSubmitButtonFrame:SetPoint("CENTER", mainframe, "CENTER", 0, 0)
    raceSubmitButtonFrame:SetWidth(130)
    raceSubmitButtonFrame:SetHeight(30)

    raceSubmitButtonFrame:SetText("Submit Race Time")
    raceSubmitButtonFrame:SetNormalFontObject("GameFontNormal")

    local ntex = raceSubmitButtonFrame:CreateTexture()
    ntex:SetTexture("Interface/Buttons/UI-Panel-Button-Up")
    ntex:SetTexCoord(0, 0.625, 0, 0.6875)
    ntex:SetAllPoints()	
    raceSubmitButtonFrame:SetNormalTexture(ntex)

    local htex = raceSubmitButtonFrame:CreateTexture()
    htex:SetTexture("Interface/Buttons/UI-Panel-Button-Highlight")
    htex:SetTexCoord(0, 0.625, 0, 0.6875)
    htex:SetAllPoints()
    raceSubmitButtonFrame:SetHighlightTexture(htex)

    local ptex = raceSubmitButtonFrame:CreateTexture()
    ptex:SetTexture("Interface/Buttons/UI-Panel-Button-Down")
    ptex:SetTexCoord(0, 0.625, 0, 0.6875)
    ptex:SetAllPoints()
    raceSubmitButtonFrame:SetPushedTexture(ptex)

    raceSubmitButtonFrame:SetAttribute("type", "macro")
    raceSubmitButtonFrame:RegisterForClicks("LeftButtonDown", "LeftButtonUp")
end

function DRL:OnDisable()
end

function DRL:CHAT_MSG_SYSTEM(event, text)
    local questAcceptedPrefix = "Quest accepted: "
    if #text >= #questAcceptedPrefix and string.sub(text, 1, #questAcceptedPrefix) == questAcceptedPrefix then
        lastQuestAccepted = string.sub(text, #questAcceptedPrefix + 1, #text)
    end
end

function DRL:CHAT_MSG_MONSTER_SAY(event, text, name)
    if listenForRaceTime and name == "Bronze Timekeeper" then
        local pattern = "Your race time was (%d+\.%d+) seconds"
        local _, _, timeString = string.find(text, pattern)
        if timeString ~= nil then
            raceTime = timeString
            listenForRaceTime = false
            DisplaySubmitPrompt()
        end
    end
end

function DRL:UNIT_AURA(_, target, info)
    if target == "player" then
        for _, aura in ipairs(info.addedAuras or {}) do
            if aura.name == "Race Starting" then
                raceName = lastQuestAccepted
                listenForRaceTime = true
            end
        end
    end
end

function GetClubId()
    local clubs = C_Club.GetSubscribedClubs()
    for i = 1, #clubs, 1 do
        if clubs[i].name == CommunityName then
            return clubs[i].clubId
        end
    end

    return nil
end

function GetStreamId(clubId)
    local streams = C_Club.GetStreams(clubId)
    for i = 1, #streams, 1 do
        if streams[i].name == ChannelName then
            return streams[i].streamId
        end
    end

    return nil
end

function DisplaySubmitPrompt()
    local clubId = GetClubId()
    local streamId = GetStreamId(clubId)
    local zone = GetZoneText()

    local raceMessage = "Completed " .. raceName .. " in " .. zone .. " in " .. raceTime .. " seconds"
    local sendMessageCommand = "/run C_Club.SendMessage("..clubId..","..streamId..",\""..raceMessage.."\")"
    local hideButtonCommand = "/run " .. raceSubmitButtonName .. ":Hide()"
    local macroText = sendMessageCommand .. "\n" .. hideButtonCommand

    raceSubmitButtonFrame:SetAttribute("macrotext", macroText)
    raceSubmitButtonFrame:Show()

    -- local status = SetBindingClick("W", buttonName, "LeftButton 1")
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