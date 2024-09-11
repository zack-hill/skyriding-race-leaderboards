local addonName, addonData = ...

function SaveRaceData()
    local _, battleTag = BNGetInfo()
    local characterName = UnitName("player")

    DataCollectorDB = DataCollectorDB or {}
    DataCollectorDB["BattleTag"] = battleTag
    DataCollectorDB["CharacterRaceData-" .. characterName] = SerializeCharacterRaceData()
end

local characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'

function Encode(data)
    return ((data:gsub('.', function(x)
        local r, b = '', x:byte()
        for i = 8, 1, -1 do r = r .. (b % 2 ^ i - b % 2 ^ (i - 1) > 0 and '1' or '0') end
        return r;
    end) .. '0000'):gsub('%d%d%d?%d?%d?%d?', function(x)
        if (#x < 6) then return '' end
        local c = 0
        for i = 1, 6 do c = c + (x:sub(i, i) == '1' and 2 ^ (6 - i) or 0) end
        return characters:sub(c + 1, c + 1)
    end) .. ({ '', '==', '=' })[#data % 3 + 1])
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
    return Encode(serialized)
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
