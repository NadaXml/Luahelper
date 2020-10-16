

--加载Mock
local AssetsMock = {}

function AssetsMock:LoadAsync(path, finish, error)
    if finish ~= nil then
        finish(obj)
    end

    if error ~= nil then
        finish(obj)
    end
end

--单个异步加载任务
local asyncTask = {

    LType = {
        None = 0,
        Pending = 1,
        Suspend = 2,
        Finish = 3,
        Error = 4
    }

}

--初始化
--param
--path 资源路径
--finish 加载成功回调 
--err --加载失败回调
function asyncTask:New(path, finish, err)
    self.asyncType = self.LType.None
    self.result = nil
    self.path = path
    self.finish = finish
    self.err = err
end

--设置多个任务加载Loader
function asyncTask:SetMulti(multi, bit)
    self.Multi = multi
    self.bit = bit
end

--通知Loader,单个任务加载成功
function asyncTask:TryNotifyFinish()
    local multi = self.Multi
    if multi ~= nil then
        multi:onTaskFinish(self.bit)
    end
end

--通知Loader，单个任务加载失败
function asyncTask:TryNotifyError()
    local multi = self.Multi
    if multi ~= nil then
        multi:onTaskError(self.bit)
    end
end

--加载某个任务
function asyncTask:LoadTask()
    AssetsMock:LoadAsync(self.path, function(asset)
        if self.asyncType == self.LType.Pending then
            if self.finish ~= nil then
                self.finish(asset)
                self.result = asset
            end
            self.asyncType = self.LType.Finish
            self:TryNotify()
        else
            --resutl可能直接Destory或者进入pool
            Destroy(asset)
        end
    end, function()
        if self.err ~= nil then
            print("Load error")
            self.err()
        end
    end)
end

--得到加载结果
function asyncTask:GetResult()
    if self.LType == self.LType.Finish then
        return self.result
    else
        return nil
    end
end