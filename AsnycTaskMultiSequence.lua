
--串行加载策略
local AsnycTaskMultiSequence = {}

function AsnycTaskMultiSequence:SetParam()
    self.curor = 1
end

--串行加载Task列表
function AsnycTaskMultiSequence:LoadTaskList(taskList)
    self.taskList = taskList
    self.curor = 1
end

function AsnycTaskMultiSequence:NextTask()
    if self.curor < #self.taskList then
        self.curor = self.curor + 1
        self.taskList[self.curor]:SetMulti(self)
        self.taskList[self.curor]:LoadTask()
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