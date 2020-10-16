

local AssetsMock = {}

function AssetsMock:LoadAsync(path, finish, error)
    if finish ~= nil then
        finish(obj)
    end
end


local asyncTask = {

    LType = {
        None = 0,
        Pending = 1,
        Suspend = 2,
        Finish = 3,
        Error = 4
    }

}

function asyncTask:New(path, finish, err)
    self.asyncType = self.LType.None
    self.result = nil
    self.path = path
    self.finish = finish
    self.err = err
end

function asyncTask:SetMulti(multi, bit)
    self.Multi = multi
    self.bit = bit
end

--加载任务
function asyncTask:LoadTask()
    AssetsMock:LoadAsync(self.path, function()
        if self.asyncType == self.LType.Pending then
            if self.finish ~= nil then
                self.finish()
            end
            self.asyncType = self.LType.Finish
            if self.Multi ~= nil then
            
            end
        else
            --resutl可能直接Destory或者进入pool
        end
    end, function()
        if self.err ~= nil then
            print("Load error")
            self.err()
        end
    end)
end

function asyncTask:GetResult()
    if self.LType == self.LType.Finish then
        return self.result
    else
        return nil
    end
end


local asyncTaskMultiBase = {}


function asyncTaskMultiBase:SetFlag()
    self.maxBit = 0
    self.curBit = 0
    self.curSuccess = 0
    self.curError = 0
end

function asyncTaskMultiBase:onAllTaskFinish()
    --to be continue
end

function asyncTaskMultiBase:postTask()
    self.curBit = self.curSuccess | self.curError
    if self.curBit == self.maxBit then
        self:onAllTaskFinish()
    end
end


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

function asyncTaskMultiParallel:onTaskFinish(bit)
    self.curSuccess = self.curSuccess & bit
    self:postTask()
end

function asyncTaskMultiParallel:onTaskError(bit)
    self.curError = self.curError & bit
    self:postTask()
end

function asyncTaskMultiParallel:LoadTaskList(taskList)
    self.taskList = taskList
    for i=1, #taskList do
        local tsk = taskList[i]
        tsk:SetMulti(self, 1 << (i-1))
        tsk:LoadTask()
    end
end






--支持串行加载
--支持并行加载
--标志位控制