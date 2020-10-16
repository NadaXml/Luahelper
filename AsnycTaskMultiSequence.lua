

--支持串行加载
--支持并行加载
--标志位控制

local asnycTaskMultiSequence = {}

function asnycTaskMultiSequence:LoadTaskList(taskList)
    self.taskList = taskList
    self.curor = 1
    taskList[self.curor]:SetMulti(self, 1 << (i-1))
    taskList[self.curor]:LoadTask()
end

function asnycTaskMultiSequence:onTaskFinish(bit)
    self.curSuccess = self.curSuccess & bit
    if self.curor < #self.taskList then
        self.curor = self.curor + 1
        self.taskList[self.curor]:SetMulti(self, 1 << (self.curor-1))
        self.taskList[self.curor]:LoadTask()
    else
        self:postTask()
    end
end

function asnycTaskMultiSequence:onTaskError(bit)
    self.curError = self.curError & bit
    if self.curor < #self.taskList then
        self.curor = self.curor + 1
        self.taskList[self.curor]:SetMulti(self, 1 << (self.curor-1))
        self.taskList[self.curor]:LoadTask()
    else
        self:postTask()
    end
end