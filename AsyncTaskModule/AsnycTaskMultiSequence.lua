
--串行加载策略
local AsnycTaskMultiSequence = Class({}, Assets.req("AsyncTaskModule.AsyncLoaderBase"))

function AsnycTaskMultiSequence:SetParam(taskList)
    self.getSuper(self, AsnycTaskMultiSequence).SetParam(self, taskList)
    self.curor = 1
end

--串行加载Task列表
function AsnycTaskMultiSequence:LoadTaskList()
    self.super.LoadTaskList(self)
    self.curor = 0
    self:NextTask()
end

--执行下一个加载任务
function AsnycTaskMultiSequence:NextTask()
    if self.taskList ~= nil then
        App.asyncLogger:error("AsyncTaskMultiParallel:LoadTaskList:","list nil")
        return
    end

    local taskList = self.taskList
    if self.curor < #taskList then
        self.curor = self.curor + 1
        taskList[self.curor]:SetMulti(self)
        taskList[self.curor]:LoadTask()
    else
        self:postTask()
    end
end

--某个任务加载成功
function AsnycTaskMultiSequence:onTaskFinish(task)
    self.getSuper(self, AsnycTaskMultiSequence).onTaskFinish(self, task)
    self:NextTask()
end

--某个任务加载失败
function AsnycTaskMultiSequence:onTaskError(task)
    self.getSuper(self, AsnycTaskMultiSequence).onTaskError(self, task)
    self:NextTask()
end

--任务加载结束后，执行的回调
function AsnycTaskMultiSequence:postTask()
    self.getSuper(self, AsnycTaskMultiSequence).onAllTaskFinish(self)
end

return AsnycTaskMultiSequence