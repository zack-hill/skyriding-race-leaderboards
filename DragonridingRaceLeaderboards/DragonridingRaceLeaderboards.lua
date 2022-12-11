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
    -- Called when the addon is enabled
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