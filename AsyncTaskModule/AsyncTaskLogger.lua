local AsyncTaskLogger = Class({})

function AsyncTaskLogger:ctor()
    self.logLevelSwitch = {
        process = true,
        error = true,
    }
    self.processPrefix = "[ATL:Process]"
    self.errorPrefix = "[ATL:Error]"
end

function AsyncTaskLogger:process(...)
    if not self.logLevelSwitch.process then
        return
    end
    print(self.procpessPrefix, ...)
end

function AsyncTaskLogger:error(...)
    if not self.logLevelSwitch.error then
        return
    end
    print(self.errorPrefix, ...)
end

function AsyncTaskLogger:setLogLevel(logLevel, bFlag)
    self.logLevelSwitch[logLevel] = bFlag
end


return AsyncTaskLogger