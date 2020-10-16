--加载策略基类
local AsyncTaskMultiBase = {}

function AsyncTaskMultiBase:onAllTaskFinish()
    --to be continue
end

function AsyncTaskMultiBase:postTask()

end

function AsyncTaskMultiBase:postTask()
    self.curBit = self.curSuccess | self.curError
    if self.curBit == self.maxBit then
        self:onAllTaskFinish()
    end
end
