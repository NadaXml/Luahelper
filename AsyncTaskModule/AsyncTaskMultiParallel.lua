
--并行加载
local AsyncTaskMultiParallel = Class({}, Assets.req("AsyncTaskModule.AsyncTaskMultiBase"))

--加载策略参数初始化
function AsyncTaskMultiParallel:SetParam(taskList)
    self.getSuper(self, AsyncTaskMultiParallel).SetParam(self, taskList)
    --所有加载成功标志位
    self.maxBit = 0
    --当前记载进度标志位
    self.curBit = 0
    --当前加载成功的标志位
    self.curSuccess = 0
    --当前加载失败的标志位
    self.curError = 0
    --task和加载标记map
    self.task2bit = {}
end

--单个Task加载成功
function AsyncTaskMultiParallel:onTaskFinish(task)
    self.getSuper(self, AsyncTaskMultiParallel).onTaskFinish(self, task)
    local bit = self.task2bit[task]
    self.curSuccess = self.curSuccess & bit
    self:postTask()
end

--单个Task加载失败
function AsyncTaskMultiParallel:onTaskError(task)
    self.getSuper(self, AsyncTaskMultiParallel).onTaskFinish(self, task)
    local bit = self.task2bit[task]
    self.curError = self.curError & bit
    self:postTask()
end

--并行加载Task列表
function AsyncTaskMultiParallel:LoadTaskList()
    if self.super.LoadTaskList(self) then
        return 
    end
    if self.taskList == nil then
        App.asyncLogger:error("AsyncTaskMultiParallel:LoadTaskList:","list nil")
        return
    end

    local taskList = self.taskList
    for i=1, #taskList do
        local tsk = taskList[i]
        self.task2bit[tsk] = 1 << (i-1)
        tsk:SetMulti(self)
        tsk:LoadTask()
    end
end

--任务加载结束后，执行的回调
function AsyncTaskMultiParallel:postTask()
    self.curBit = self.curSuccess | self.curError
    if self.curBit == self.maxBit then
        self.getSuper(self, AsyncTaskMultiParallel).onAllTaskFinish(self)
    end
end

return AsyncTaskMultiParallel