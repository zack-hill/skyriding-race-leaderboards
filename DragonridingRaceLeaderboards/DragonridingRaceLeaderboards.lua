AppName = "Dragonriding Race Leaderboards"
DRL = LibStub("AceAddon-3.0"):NewAddon(AppName)

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
LibStub("AceConfig-3.0"):RegisterOptionsTable(AppName, options, nil)
LibStub("AceConfigDialog-3.0"):AddToBlizOptions(AppName, AppName)

function DRL:OnInitialize()
    -- Code that you want to run when the addon is first loaded goes here.
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

    button:SetPoint("CENTER", mainframe, "CENTER", 0, 0)
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

    local status = SetBindingClick("W", buttonName, "LeftButton 1")
end

function DRL:OnDisable()
    -- Called when the addon is disabled
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