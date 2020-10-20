


local AsyncTaskUI = Class({}, Assets.req("AsyncTaskModule.AsyncTask"))


function AsyncTaskUI:Recycle()
    self.getSuper(self, AsyncTaskUI).Recycle(self)
    local result = self.getSuper(self, AsyncTaskUI).GetResult(self)
    if result ~= nil then
        result:destroy()
    end
end


return AsyncTaskUI