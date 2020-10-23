--加载策略基类

local AsyncTaskMultiBase = Class({})
function AsyncTaskMultiBase:ctor(key)
    self.key = key
end

--加载策略参数
function AsyncTaskMultiBase:SetParam(taskList)
    self.taskList = taskList
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
--防止冲入
function AsyncTaskMultiBase:LoadTaskList()
    --需要打开遮罩,禁止操作
    if not self.isLoading then 
        self.isLoading = true
        return false
    else
        App.asyncLogger:error("repeat load")
        return true
    end
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
    self.isLoading = false
    --这个放基类时序，可能有问题，虚函数定义需要多几个
    App.async:RemoveTaskMulti(self.key)
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
        --说明有任务，还在加载中
    end
end

return AsyncTaskMultiBase