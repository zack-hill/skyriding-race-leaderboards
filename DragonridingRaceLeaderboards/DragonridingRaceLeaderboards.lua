local frame = CreateFrame("FRAME", "FooAddonFrame");
frame:RegisterEvent("UNIT_AURA");
local function eventHandler(self, event, unitTarget, updatedAuras)
    if unitTarget ~= "player" then
        return
    end
    addedAuras = updatedAuras["addedAuraInstanceIDs"] or {}
    removedAuras = updatedAuras["removedAuraInstanceIDs"] or {}
    for a in pairs(addedAuras) do
        print("Added " .. a["name"])
    end
    for a in pairs(removedAuras) do
        print("Removed " .. a)
    end
    -- print("Event " .. event);
    -- print("Target " .. unitTarget);
    -- for k in pairs(updatedAuras) do
    --     print(k)
    -- end
end
frame:SetScript("OnEvent", eventHandler);

local clubName = ""
local streamName = ""

-- Get the club id
local clubId = -1
local clubs = C_Club.GetSubscribedClubs()
for i = 1, #clubs, 1 do
    if clubs[i].name == clubName then
        clubId = clubs[i].clubId
    end
end
print(clubId)

-- Get the stream id
local streamId = -1
streams = C_Club.GetStreams(clubId)
for i = 1, #streams, 1 do
    if streams[i].name == streamName then
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


