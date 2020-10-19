
--串行加载策略
local AsnycTaskMultiSequence = Class({}, Assets.req("AsyncTaskModule.AsyncLoaderBase"))

function AsnycTaskMultiSequence:SetParam(taskList)
    self.getSuper(self, AsnycTaskMultiSequence, taskList)
    self.curor = 1
end

--串行加载Task列表
function AsnycTaskMultiSequence:LoadTaskList()
    self.curor = 1
    self:NextTask()
end

function AsnycTaskMultiSequence:NextTask()
    local taskList = self.taskList
    if taskList ~= nil then
        --加载为空
        return
    end

    if self.curor < #taskList then
        self.curor = self.curor + 1
        taskList[self.curor]:SetMulti(self)
        taskList[self.curor]:LoadTask()
    else
        self:postTask()
    end
end

--某个任务加载成功
function AsnycTaskMultiSequence:onTaskFinish()
    self:NextTask()
end

--某个任务加载失败
function AsnycTaskMultiSequence:onTaskError()
    self:NextTask()
end

--任务加载结束后，执行的回调
function AsnycTaskMultiSequence:postTask()
    self:onAllTaskFinish()
end

return AsnycTaskMultiSequence