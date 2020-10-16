
local AsyncTaskMultiParallel = {}


--策略参数初始化
function AsyncTaskMultiParallel:SetParam()
    self.maxBit = 0
    self.curBit = 0
    self.curSuccess = 0
    self.curError = 0
end

function AsyncTaskMultiParallel:onTaskFinish(bit)
    self.curSuccess = self.curSuccess & bit
    self:postTask()
end

function AsyncTaskMultiParallel:onTaskError(bit)
    self.curError = self.curError & bit
    self:postTask()
end

function AsyncTaskMultiParallel:LoadTaskList(taskList)
    self.taskList = taskList
    for i=1, #taskList do
        local tsk = taskList[i]
        tsk:SetMulti(self, 1 << (i-1))
        tsk:LoadTask()
    end
end

function AsyncTaskMultiParallel:postTask()
    self.curBit = self.curSuccess | self.curError
    if self.curBit == self.maxBit then
        self:onAllTaskFinish()
    end
end