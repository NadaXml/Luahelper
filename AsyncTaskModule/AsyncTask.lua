--单个异步加载任务
local AsyncTask = Class({})
local AsyncManage = Assets.req("AsyncTaskModule.AsyncManage")

AsyncTask.LType = {
    None = 0,
    Pending = 1,
    Suspend = 2,
    Finish = 3,
    Error = 4
}

--初始化
--param
--path 资源路径
--finish 加载成功回调
--err --加载失败回调
function AsyncTask:ctor(key, path, finish, err)
    self.asyncType = self.LType.None
    self.result = nil
    self.path = path
    self.finish = finish
    self.err = err
    self.key = key
end

--设置多个任务加载Loader
function AsyncTask:SetMulti(multi)
    self.multi = multi
end

--通知Loader,单个任务加载成功
function AsyncTask:TryNotifyFinish()
    local multi = self.multi
    if multi ~= nil then
        multi:onTaskFinish(self)
    end
end

--通知Loader，单个任务加载失败
function AsyncTask:TryNotifyError()
    local multi = self.multi
    if multi ~= nil then
        multi:onTaskError(self)
    end
end

--加载某个任务
function AsyncTask:LoadTask()
    AssetsMock:LoadAsync(self.path,
    function(asset)
        self:TryNotifyFinish()
    end,
    function()
        self:OnError()
    end)
end

--加载完成逻辑
function AsyncTask:OnLoaded(asset)
    if self.asyncType == self.LType.Pending then
        self:SetLType(self.LType.Finish)
        self.result = asset

        if self.finish ~= nil then
            self.finish(asset)
        end
        self:TryNotifyFinish()
    else
        --Task被外部终止，因为Asset接口一定有会回调
        --resutl可能直接Destory或者进入pool
        self:Recycle()
        if self.Multi == nil then
            AsyncManage:RemoveTask(self.key)
        end
        self:TryNotifyError()
    end

end

--加载错误逻辑
function AsyncTask:OnError()
    self:SetLType(self.LType.Error)
    if self.err ~= nil then
        print("Load error")
        self.err()
    end
end

--设置加载状态
function AsyncTask:SetLType(ltype)
    self.asyncType = ltype
end


--得到加载结果
function AsyncTask:GetResult()
    if self.LType == self.LType.Finish then
        return self.result
    else
        return nil
    end
end

--放弃某个加载（包括加载中）
function AsyncTask:SuspendTask()
    if self.asyncType == self.LType.Finish then
        self:Recycle()
        return true
    else
        self:SetLType(self.LType.Suspend)
        return false
    end
end

--回收加载成功的资源
function AsyncTask:Recycle()
    Destroy(asset)
end

return AsyncTask