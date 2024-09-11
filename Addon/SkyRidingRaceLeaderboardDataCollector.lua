local addonName, addonData = ...

function SaveRaceData()
    local _, battleTag = BNGetInfo()
    local characterName = UnitName("player")
    
    DataCollectorDB = DataCollectorDB or {}
    DataCollectorDB["BattleTag"] = battleTag
    DataCollectorDB["CharacterRaceData-" .. characterName] = SerializeCharacterRaceData()
end

function SerializeCharacterRaceData()
    local serialized = "{"
    for _, currencyId in ipairs(addonData.raceDataCurrencyIds) do
        local quantity = C_CurrencyInfo.GetCurrencyInfo(currencyId).quantity
        if quantity ~= 0 then
            serialized = serialized .. "\"" .. currencyId .. "\": " .. quantity .. ","
        end
    end
    serialized = serialized:sub(1, -2) .. "}"
    return serialized
end

local function OnEvent(self, event, ...)
    SaveRaceData()
end

local frame = CreateFrame("FRAME", addonName);
frame:RegisterEvent("PLAYER_ENTERING_WORLD");
frame:RegisterEvent("PLAYER_LEAVING_WORLD");
frame:SetScript("OnEvent", OnEvent);

SLASH_SaveRaceData1 = '/srdc';
function SlashCmdList.SaveRaceData(msg, editBox)
    print("Saving race data");
    SaveRaceData()
    print("Race data saved");
end
