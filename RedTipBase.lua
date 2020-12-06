local RedTipBase = Class({})

-- 标示(动态赋值)
RedTipBase.sign = ""

-- 唯一id(动态赋值),供标示一致的子对象使用
RedTipBase.id = nil

-- 红点是否激活
RedTipBase.activate = false

-- 是否作用父节点
RedTipBase.actOnParent = true

-- 红点数值
RedTipBase.value = 0

-- 红点类型
RedTipBase.type = RedTipConst.Type.New

-- 进行某种操作后消失
--nil表示不进行这个规则判断
--其他规则在下方函数
RedTipBase.optActivate = nil

-- 父节点
RedTipBase.parent = nil

-- 子节点
RedTipBase.childrens = nil

-- 管理者
RedTipBase.manage = nil

function RedTipBase:ctor(sign, id, manage, parent)
    self.activate = false
    self.value = 0
    self.sign = sign
    self.id = id
    self.manage = manage
    self.parent = parent
end

-- 创建子红点
function RedTipBase:createChildren(sign, id, class)
    if not self.childrens then
        self.childrens = {}
    end
    -- 子对象
    local children = self.manage:createRedTip(sign, id, class, self)
    self.childrens[#self.childrens + 1] = children
    return children
end

-- 创建子红点 Map不遵守红点树的规则
-- 一个红点可以出现在多个地方
function RedTipBase:createChildrenMap(sign, id, class)
    if not self.childMap then
        self.childMap = {}
    end
    -- 子对象
    local children = self.manage:createRedTipMap(sign, id, class, self)
    self.childMap[id] = children
    return children
end

-- 迭代创建子红点
function RedTipBase:createAllChildrens()
    local childrenSigns = RedTipConst.childrenSigns[self.sign]
    if childrenSigns then
        for i = 1, #childrenSigns do
            local childrenSign = childrenSigns[i]
            local childrenClass = Assets.req(childrenSign)
            local children = self:createChildren(childrenSign, nil, childrenClass)
            children:createAllChildrens()
        end
    end
end

-- 迭代执行初始化
function RedTipBase:doInit()
    self:init()
    if self.childrens then
        for i = 1, #self.childrens do
            self.childrens[i]:doInit()
        end
    end
end

-- 迭代执行关闭
function RedTipBase:doClose()
    self:close()
    if self.childrens then
        for i = 1, #self.childrens do
            self.childrens[i]:doClose()
        end
    end

    if self.childMap then
        for k , v in pairs(self.childMap) do
            v:doClose()
        end
    end
end

-- 返回所有的孩子
function RedTipBase:GetChildren()
    return self.childrens
end

function RedTipBase:ClearChildren()
    self:close()
    if self.childrens then
        for i = 1, #self.childrens do
            self.childrens[i]:ClearChildren()
            self.manage:RemoveRedTip(self.childrens[i].sign, self.childrens[i].id)
        end
    end
    self.childrens = nil
end

-- 一键隐藏子节点红点显示，调用该接口时需要处理对应数据，防止子节点被重置刷新
function RedTipBase:clearChildrenActive()
    if self.childrens then
        for i = 1, #self.childrens do
            self.childrens[i]:clearChildrenActive()
        end
    else
        self:setActive(false)
    end
end

function RedTipBase:getRedTip(sign, id)
    return self.manage:getRedTip(sign, id)
end

-- 初始化,注册事件
function RedTipBase:init()
end

-- 关闭,移除事件
function RedTipBase:close()
end

-- 根据事件,数据刷新
function RedTipBase:refresh()
end

-- 根据子对象变化刷新状态(必须保证子节点一开一关)
function RedTipBase:refreshByChildren(active)
    self.value = self.value + (active and 1 or -1)
    -- print("redtip:", self.sign, self.activate, "父亲刷新")
    self:setActive(self.value > 0)
end

-- 设置数值(只可用于最末节点,不可与setActive同用)
function RedTipBase:setValue(value)
    if value == nil then
        value = 0
    end
    if self.value == value then
        if self.optActivate ~= nil then
            if self.activate == (value > 0 and self.optActivate) then
                return
            end
        else
            return
        end
    end
    self.value = value
    local active = self.value > 0
    if self.optActivate ~= nil then
        active = active and self.optActivate
    end
    if self.activate == active then
        -- print("redtip:", self.sign, self.activate, self.value)
        App.event:dispatchEvent(EventManage.EventType.redTip, self.sign)
    else
        self.activate = active
        -- print("redtip:", self.sign, self.activate, self.value)
        App.event:dispatchEvent(EventManage.EventType.redTip, self.sign)
        if self.actOnParent and self.parent then
            self.parent:refreshByChildren(self.activate)
        end
    end
end

-- 设置状态(不可与setValue同用)
function RedTipBase:setActive(active)
    if active == nil then
        active = false
    end
    if self.optActivate ~= nil then
        active = active and self.optActivate
    end
    if self.activate == active then
        return
    end
    self.activate = active
    -- print("redtip:", self.sign, self.activate)
    App.event:dispatchEvent(EventManage.EventType.redTip, self.sign)
    if self.actOnParent and self.parent then
        self.parent:refreshByChildren(self.activate)
    end
end

-- 获取状态类型
function RedTipBase:getType()
    if self.childrens then
        for i = 1, #self.childrens do
            if self.childrens[i].activate then
                return self.childrens[i]:getType()
            end
        end
    end
    return self.type
end

-- 用户操作小红点标志
-- 规则
-- self.optActivate：true表示进行过操作
-- self.optActivate：false表示进行过操作
function RedTipBase:getOptActivate()
    return self.optActivate
end

-- 进行操作，并且触发红点刷新
function RedTipBase:setOptActivate(flag)
    if self.optActivate ~= flag then
        self.optActivate = flag
        self:refresh()
    end
end

-- 规则
-- 1、进行过操作后，self.optActivate = false，本次登录红点中，本次红点都认为已访问
-- 2、当红点保持true - false，self.optActivat 不发生变化
-- 3、当红点从false -> true的时候，self.optActivate自动重置成true，再次刷新
-- 4、目前这个规则不能完全满足策划要求，所以屏蔽这个功能
function RedTipBase:switchOptActivate(oldShow, newShow)
    if oldShow ~= newShow then
        if newShow then
            self.optActivate = true
        end
    end
end

function RedTipBase:createMapRed(id, sign, parent)
    if self.subMap == nil then
        self.subMap = {}
    end

    if self.subMap[id] == nil then
        local instance = class.new(sign, id, self, parent)
        if self.useDelay then
            instance.refreshByManage = instance.refresh
            instance.refresh = function()
                self:addRefreshOne(instance)
            end
        end
    else
        print(" same red ")
    end

end


return RedTipBase
