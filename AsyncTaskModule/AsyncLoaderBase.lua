--加载策略基类
local AsyncTaskMultiBase = Class({})
local AsyncManage = Assets.req("AsyncTaskModule.AsyncManage")
local logger = AsyncManage.logger


function AsyncTaskMultiBase:ctor()
end

--加载策略参数
function AsyncTaskMultiBase:SetParam(taskList)
    self.taskList = taskList
    --加载被中断，需要在所有加载完成后，删除Multi对象
    self.bIsRemove = false
    --所有所有加载过程结束
    self.bAllReady = true
end

--加载某个对象，子类重写
function AsyncTaskMultiBase:LoadTaskList()
    --需要打开遮罩,禁止操作
end

--Task加载成功,子类重写
function AsyncTaskMultiBase:onTaskFinish(task)
    logger:process("AsyncTaskMultiBase:onTaskFinish", task.key)
end

--Task加载失败，子类重写
function AsyncTaskMultiBase:onTaskError(task)
    logger:error("AsyncTaskMultiBase:onTaskError", task.key)
end


--所有Task加载结束
function AsyncTaskMultiBase:onAllTaskFinish()
    logger:process("AsyncTaskMultiBase:onAllTaskFinish")
    self.bAllReady = true
    if self.bIsRemove then
        AsyncManage:RemoveTaskMulti(self.key)
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
        AsyncManage:RemoveTaskMulti(self.key)
    else
        self.bIsRemove = true
    end
end

return AsyncTaskMultiBase