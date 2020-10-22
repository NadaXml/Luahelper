--加载策略基类
local AsyncTaskMultiBase = Class({})

function AsyncTaskMultiBase:ctor()
end

--加载策略参数
function AsyncTaskMultiBase:SetParam(taskList)
    self.taskList = taskList
    --加载被中断，需要在所有加载完成后，删除Multi对象
    self.bIsRemove = false
    --所有所有加载过程结束
    self.bAllReady = false
    --加载中
    self.isLoading = false
end

--- AsyncTaskMultiBase.appendTask 添加某一个任务,未开始加载的时候才有效
-- @param task 增加的AsyncTask
function AsyncTaskMultiBase:appendTask(task)
    if not self.isLoading then
        table.insert(self.taskList, task)
    else
        App.asyncLogger:error("loading try append failed", task.key)
    end
end

--加载某个对象，子类重写
function AsyncTaskMultiBase:LoadTaskList()
    --需要打开遮罩,禁止操作
    self.isLoading = true
end

--Task加载成功,子类重写
function AsyncTaskMultiBase:onTaskFinish(task)
    App.asyncLogger:process("AsyncTaskMultiBase:onTaskFinish", task.key)
end

--Task加载失败，子类重写
function AsyncTaskMultiBase:onTaskError(task)
    App.asyncLogger:error("AsyncTaskMultiBase:onTaskError", task.key)
end


--所有Task加载结束
function AsyncTaskMultiBase:onAllTaskFinish()
    App.asyncLogger:process("AsyncTaskMultiBase:onAllTaskFinish")
    self.bAllReady = true
    self.isLoading = false
    if self.bIsRemove then
        App.async:RemoveTaskMulti(self.key)
        self.bIsRemove = false
    end
end

--从队列中取出某一个任务
function AsyncTaskMultiBase:GetTaskFromListByIndex( index)
    return self.taskList[index]
end

--终止加载队列
function AsyncTaskMultiBase:StopTaskMulti()
    local bAll = true
    for i=1, #self.taskList do
        bAll = bAll and self.taskList[i]:SuspendTask()
    end
    if bAll then
        App.async:RemoveTaskMulti(self.key)
    else
        self.bIsRemove = true
    end
end

return AsyncTaskMultiBase