

local AsyncTaskPoolObj = Class({}, Assets.req("AsyncTaskModule.AsyncTask"))


function AsyncTaskPoolObj:Recycle()
    self.getSuper(self, AsyncTaskPoolObj).Recycle(self)
    local result = self.getSuper(self, AsyncTaskPoolObj).GetResult(self)
    if result ~= nil then
        result:release()
    end
end

return AsyncTaskPoolObj