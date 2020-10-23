--单个异步加载任务
local AsyncTask = Class({})

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
    App.asyncLogger:process("AsyncTask:TryNotifyFinish", self.key)
    local multi = self.multi
    if multi ~= nil then
        multi:onTaskFinish(self)
    end
end

--通知Loader，单个任务加载失败
function AsyncTask:TryNotifyError()
    App.asyncLogger:process("AsyncTask:TryNotifyError", self.key)
    local multi = self.multi
    if multi ~= nil then
        multi:onTaskError(self)
    end
end

--加载某个任务
function AsyncTask:LoadTask()
    App.asyncLogger:process("AsyncTask:LoadTask", self.key)
    self:SetLType(self.LType.Pending)
    Assets.loadAsync(self.path,
    function(asset)
        App.asyncLogger:process("AsyncTask:LoadTask:finish", self.key)
        self:OnLoaded(asset)
    end,
    function (currentDonelen)
        App.asyncLogger:process("AsyncTask:LoadTask:progress", self.key, currentDonelen)
    end,
    function()
        App.asyncLogger:error("AsyncTask:LoadTask:error",self.key)
        self:OnError()
    end)
end

function AsyncTask:isPending()
    return self.asyncType == self.LType.Pending
end

--加载完成逻辑
function AsyncTask:OnLoaded(asset)
    App.asyncLogger:process("AsyncTask:OnLoaded",self.key, self.asyncType)
    if self.asyncType == self.LType.Pending then
        self:SetLType(self.LType.Finish)
        --self.result = asset

        if self.finish ~= nil then
            self.finish(asset, self)
        end
        self:tryRemoveMySelf()
        self:TryNotifyFinish()
    else
        --Task被外部终止，因为Asset接口一定有会回调
        --resutl可能直接Destory或者进入pool
        self:Recycle()
        self:tryRemoveMySelf()
        self:TryNotifyError()
    end
end

function AsyncTask:tryRemoveMySelf()
    if self.Multi == nil then
        App.async:RemoveTask(self.key)
    end
end

--加载错误逻辑
function AsyncTask:OnError()
    App.asyncLogger:process("AsyncTask:OnError",self.key)
    self:SetLType(self.LType.Error)
    if self.err ~= nil then
        self.err()
    end
    self:tryRemoveMySelf()
    self:TryNotifyError()
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
    App.asyncLogger:process("AsyncTask:SuspendTask",self.key)
    if self.asyncType == self.LType.Finish then
        self:Recycle()
        return true
    else
        self:SetLType(self.LType.Suspend)
        return false
    end
end

--回收加载成功的资源--子类必须准确实现，父类
function AsyncTask:Recycle()
    self.postFuncList = nil
    App.asyncLogger:process("AsyncTask:Recycle",self.key)
end

--为了适应当前结构
--UIBase打开有一个回调
--UIManage打开有一个setOrder回调
--增加一个加载成功回调列表，绑在Task上，方法加数据，不存闭包，不好管理
--注意Table 内存的申请
--防止重复
function AsyncTask:BindPostLoaded(caller, func, data)
    local tempList = self.postFuncList
    if tempList == nil then
        tempList = {}
        self.postFuncList = tempList
    end

    local _, findRet = table.find(tempList, function(findK , findV)
        return findV.func == func
    end)

    if findRet ~= nil then
        findRet.caller = caller
        findRet.data = data
    else
        table.insert(tempList, {
            caller = caller,
            func = func,
            data = data
        })
    end

    
end

--设置
function AsyncTask:SetPostLoadedData(func, k , v)
    local tempList = self.postFuncList
    if tempList ~= nil then
        local _, findRet = table.find(tempList, function(findK , findV)
            return findV.func == func
        end)

        findRet.data[k] = v
    end
end

--执行加载成功后的回调方法
--需要保证有序执行
function AsyncTask:doPostFinish()
    for i, v in ipairs(self.postFuncList) do
        v.func(v.caller, v.data)
    end
    self.postFuncList = {} 
end

return AsyncTask