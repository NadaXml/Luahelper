--统一管理异步的数据，以为Task任务结束的生命周期和外部调用的周期不一致
--需要统一生命周期和数据管理
--也需要pool管理

local AsyncManage = Class({})
local AsyncTaskUI = Assets.req("AsyncTaskModule.AsyncTaskUI")
local AsyncTaskLogger = Assets.req("AsyncTaskModule.AsyncTaskLogger")

function AsyncManage:Init()
    self.allTaskMap = {}
    self.allTaskMultiMap = {}
    self.logger = AsyncTaskLogger.new()
end

--创建单个异步任务
function AsyncManage:CreateTaskUI(key, path, finish, err)
    local at = AsyncTaskUI.new(key, path, finish, err)
    self.allTaskMap[key] = at
    return at
end

--得到一个异步任务，多个任务用
function AsyncManage:ObtainTaskUI(path, finish, err)
    local at = AsyncTaskUI.new(nil, path, finish, err)
    return at
end

--创建Multi任务
function AsyncManage:CreateTaskMulti(key, ...)
    local ll = {...}
    self.allTaskMultiMap[key] = ll
end

function AsyncManage:RemoveTask(key)
    self.allTaskMap[key] = nil
end

function AsyncManage:RemoveTaskMulti(key)
    self.allTaskMultiMap[key] = nil
end

function AsyncManage:FindTask(key)
    return self.allTaskMap[key]
end

function AsyncManage:FindTaskMulti(key)
    return self.allTaskMultiMap[key]
end

--是否要保存数据，或者直接闭包保存

return AsyncManage