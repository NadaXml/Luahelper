local RedTipBase = Assets.req("")
local RedTipBaseMap = Class({}, "RedTipBaseMap")

--只用于这个节点内的刷新
function RedTipBaseMap:isSubMapActive(id)
    if self.subMap[id] == nil then
        return false
    end
    return self.subMap[id]
end

--只用于这个节点内的刷新
function RedTipBaseMap:setSubMapActive(id, flag, isRefresh)
    self.subMap[id] = flag
    if isRefresh then
        self:mapRefresh()
    end
end

function RedTipBaseMap:mapRefresh()
    local red = false
    for k , v in pairs(self.subMap) do
        red = red or v
        if red then
            break
        end
    end
    self:setActive(red)
end

function RedTipBaseMap:init()
    addEvent 
    mapRefresh
end

function RedTipBaseMap:close()

end