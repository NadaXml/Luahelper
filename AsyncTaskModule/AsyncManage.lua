--统一管理异步的数据，以为Task任务结束的生命周期和外部调用的周期不一致
--需要统一生命周期和数据管理
--也需要pool管理

local AsyncManage = Class({})
local AsyncTaskUI = Assets.req("AsyncTaskModule.AsyncTaskUI")
local AsyncTaskMultiParallel = Assets.req("AsyncTaskModule.AsyncTaskMultiParallel")
local AsnycTaskMultiSequence = Assets.req("AsyncTaskModule.AsnycTaskMultiSequence")
local AsyncTaskLogger = Assets.req("AsyncTaskModule.AsyncTaskLogger")

function AsyncManage:Init()
    self.allTaskMap = {}
    self.allTaskMultiMap = {}
    self.logger = AsyncTaskLogger.new()
end

--- AsyncManage.CreateTaskUI Description of the function
-- @param key    Describe the parameter
-- @param path   Describe the parameter
-- @param finish Describe the parameter
-- @param err    Describe the parameter
function AsyncManage:CreateTaskUI(key, path, finish, err)
    local at = AsyncTaskUI.new(key, path, finish, err)
    self.allTaskMap[key] = at
    return at
end

--- AsyncManage.CreateTaskMulti 串行
-- @param key key
-- @param ... 参数
function AsyncManage:CreateTaskMultiSeq(key, ...)
    local atm = self.allTaskMultiMap[key]
    if atm == nil then
        local ll = {...}
        local atm = AsnycTaskMultiSequence.new()
        atm:SetParam(ll)
        self.allTaskMultiMap[key] = atm
        return atm
    else
        self.logger:error("CreateTaskMultiSeq:Repeate",key)
    end
end

--- AsyncManage.CreateTaskMultiparllel 并行
-- @param key key
-- @param ... 参数
function AsyncManage:CreateTaskMultiparllel(key, ...)
    local atm = self.allTaskMultiMap[key]
    if atm == nil then
        local ll = {...}
        local atm = AsyncTaskMultiParallel.new()
        atm:SetParam(ll)
        self.allTaskMultiMap[key] = atm
        return atm
    else
        self.logger:error("CreateTaskMultiparllel:Repeate",key)
    end
end

--- AsyncManage.RemoveTask Description of the function
-- @param key Describe the parameter
function AsyncManage:RemoveTask(key)
    self.allTaskMap[key] = nil
end

--- AsyncManage.RemoveTaskMulti Description of the function
-- @param key Describe the parameter
function AsyncManage:RemoveTaskMulti(key)
    self.allTaskMultiMap[key] = nil
end

--- AsyncManage.FindTask Description of the function
-- @param key Describe the parameter
function AsyncManage:FindTask(key)
    return self.allTaskMap[key]
end

--- AsyncManage.FindTaskMulti Description of the function
-- @param key Describe the parameter
function AsyncManage:FindTaskMulti(key)
    return self.allTaskMultiMap[key]
end

--是否要保存数据，或者直接闭包保存

return AsyncManage