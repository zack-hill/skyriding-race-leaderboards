local addonName, addonData = ...

AppName = "Sky Riding Race Leaderboard Data Collector"

-- local f = CreateFrame(AppName)

-- function f:OnEvent(event, ...)
-- 	self[event](self, event, ...)
-- end

-- function f:ADDON_LOADED(event, addOnName)
-- 	print(event, addOnName)
-- end

-- function f:PLAYER_ENTERING_WORLD(event, isLogin, isReload)
-- 	print(event, isLogin, isReload)
-- end

function SaveRaceData()
    local _, battleTag = BNGetInfo()
    local characterName = UnitName("player")
    local characterTimeData = {}

    for k, currencyId in ipairs(addonData.raceDataCurrencyIds) do
        local quantity = C_CurrencyInfo.GetCurrencyInfo(currencyId).quantity
        if quantity ~= 0 then
            characterTimeData[currencyId] = quantity
        end
    end
    DataCollectorDB = DataCollectorDB or {}
    DataCollectorDB[battleTag] = DataCollectorDB[battleTag] or {}
    DataCollectorDB[battleTag][characterName] = characterTimeData
end

-- f:RegisterEvent("ADDON_LOADED")
-- f:RegisterEvent("PLAYER_ENTERING_WORLD")
-- f:SetScript("OnEvent", f.OnEvent)

SLASH_SaveRaceData1 = '/srdc';
function SlashCmdList.SaveRaceData(msg, editBox)
    print("Saving race data");
    SaveRaceData()
    print("Race data saved");
end
