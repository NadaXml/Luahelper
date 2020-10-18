--加载策略基类
local AsyncTaskMultiBase = {}

--加载策略参数
function AsyncTaskMultiBase:SetParam()
    --加载被中断，需要在所有加载完成后，删除Multi对象
    self.bIsRemove = false
    --所有所有加载过程结束
    self.bAllReady = true
end

--Task加载成功
function AsyncTaskMultiBase:onTaskFinish(task)
end

--Task加载失败
function AsyncTaskMultiBase:OnTaskError(task)
end


--所有Task加载结束
function AsyncTaskMultiBase:onAllTaskFinish()
    self.bAllReady = true
    if self.bIsRemove then
        AssetManage:RemoveTaskMulti(self.key)
        self.bIsRemove = false
    end
end

function AsyncTaskMultiBase:GetTaskFromListByIndex( index)
    return self.taskList[index]
end

--单个ask过程结束触发的回调
function AsyncTaskMultiBase:postTask()

end

function AsyncTaskMultiBase:StopTaskMulti()
    local bAll = true
    for i=1, #self.taskList do
        bAll = bAll and self.taskList[i]:SuspendTask()
    end
    if bAll then
        AssetManage:RemoveTaskMulti(self.key)
    else
        self.bIsRemove = true
    end
end