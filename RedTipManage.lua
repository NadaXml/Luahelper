local RedTipConst = Assets.req("LocalData.RedTip.RedTipConst")

-- 红点管理
local RedTipManage = Class({})

-- 是否注销
RedTipManage.halted = true

-- id
RedTipManage.id = 0

-- 是否延迟0.3秒执行刷新数据方法（有效降低红点执行次数）
RedTipManage.useDelay = true

-- 红点对象列表
RedTipManage.list = nil

-- 红点刷新列表(为了延迟刷新)
RedTipManage.refreshList = nil

-- 主红点
RedTipManage.main = nil

-- 初始化
function RedTipManage:init()
    self.halted = false
    self.list = {}
    self.refreshList = {}

    -- 构建树
    self:createRedTipTree()

    -- 启动红点定时器
    if self.useDelay then
        self.id =
            TimerEvent.setInterval(
            function()
                self:refresh()
            end,
            0.04
        )
    end
end

--创建红点树结构
function RedTipManage:createRedTipTree()
    local sign = RedTipConst.Main
    local class = Assets.req(sign)
    self.main = self:createRedTip(sign, nil, class, nil)
    self.main:createAllChildrens()
    self.main:doInit()
end

-- 创建红点,标示唯一
function RedTipManage:createRedTip(sign, id, class, parent)
    if self.list[sign] then
        if id then
            if self.list[sign][id] then
                error("already have a same sign&id red tip", sign, id)
                return
            end
        else
            error("already have a same sign red tip")
            return
        end
    end

    local instance = class.new(sign, id, self, parent)
    if self.useDelay then
        instance.refreshByManage = instance.refresh
        instance.refresh = function()
            self:addRefreshOne(instance)
        end
    end

    if id ~= nil then
        if not self.list[sign] then
            self.list[sign] = {}
        end
        self.list[sign][id] = instance
    else
        self.list[sign] = instance
    end

    return instance
end

function RedTipManage:createRedTipMap(sign, id, class, parent)
    local instance = class.new(sign, id, self, parent)
    if self.useDelay then
        instance.refreshByManage = instance.refresh
        instance.refresh = function()
            self:addRefreshOne(instance)
        end
    end
    return instance
end

function RedTipManage:RemoveRedTip(sign, id)
    if self.list[sign] then
        if id ~= nil then
            if self.list[sign][id] then
                self.list[sign][id] = nil
                if table.count(self.list[sign]) == 0 then
                    self.list[sign] = nil
                end
            else
                error("there is no node with ID [" .. id .. "] under node sign [" .. sign .. "]")
            end
        else
            self.list[sign] = nil
        end
    else
        error("there is no node with sign of [" .. sign .. "]")
    end
end

-- 获取红点是否激活
function RedTipManage:getActive(sign, id)
    local redTip = self:getRedTip(sign, id)
    if redTip then
        return redTip.activate
    else
        return false
    end
end

-- 获取红点数值
function RedTipManage:getValue(sign, id)
    local redTip = self:getRedTip(sign, id)
    if redTip then
        return redTip.value
    else
        return 0
    end
end

-- 获取红点类型
function RedTipManage:getType(sign)
    return self.list[sign]:getType()
end

-- 添加等待执行刷新的标记
function RedTipManage:addRefreshOne(redTip)
    if not table.exists(self.refreshList, redTip) then
        table.insert(self.refreshList, redTip)
    end
end

-- 执行下一个初始化方法
function RedTipManage:refresh()
    if self.halted then
        return
    end

    if #self.refreshList == 0 then
        return
    end

    local redTip = self.refreshList[1]
    table.remove(self.refreshList, 1)
    redTip:refreshByManage()
end

function RedTipManage:getRedTip(sign, id)
    if id ~= nil then
        if self.list[sign] then
            return self.list[sign][id]
        end
    else
        return self.list[sign]
    end

    return nil
end

-- 关闭
function RedTipManage:close()
    if self.halted then
        return
    end
    self.halted = true

    if self.id > 0 then
        TimerEvent.clear(self.id)
        self.id = 0
    end

    if self.main then
        self.main:ClearChildren()
    end
end

return RedTipManage
