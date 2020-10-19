

--去除URL中的<修饰符>
function convertURL(url)
    
    local bt = {}
    bt.i = true
    bt.b = true
    bt.color = true

    local rt = string.gsub(url, "</?(%a+)=?#?%w->", function(a)
        print(a)
        if bt[a] then
            return ""
        end
    end)

    print(rt)
    return rt

    --[[
    local bt = {}
    bt.i = true
    bt.b = true
    bt.color = true

    local stack = {}

    local realStrs1 = 0
    local realStre1 = 0
    local pos = 1
    local bPattern = "<(%a+)=?#?%w->"
    local ePattern = "</(%a+)>"
    repeat
        local s1,e1 = string.find(url, bPattern, pos)
        print(s1,e1)
        if s1 ~= nil then
            local tmp =  string.match(url, bPattern, pos)
            print( "input ", tmp)
            pos = e1

            if bt[tmp] then
                table.insert(stack, tmp)
                
                realStrs1 = e1
            end
        else
            local s2,e2 = string.find(url, ePattern, pos)
            print(s2,e2)
            if s2 ~= nil then

                local tmp = string.match(url, ePattern, pos)
                print( "pair ", tmp)
                pos = e2
                if bt[tmp] then
                    if realStre1 == 0 then
                        realStre1 = s2-1
                    end

                    if stack[#stack] ~= tmp then
                        --出错了
                        print("error find ")
                        realStrs1 = 0
                        realStre1 = 0
                        break
                    else
                        table.remove(stack, #stack)
                    end
                end
            else
                break
            end
        
        end
    until( #stack == 0)

    print ( realStrs1, realStre1 )
    if realStrs1 == realStre1 then
        return url
    else
        local temp = string.sub(url, realStrs1+1, realStre1)
        return temp
    end]]
    
end

--匹配出URL的文本
function matchURL(ss)
    local ret = string.gmatch(ss, "<a href=\"(.-)\"></a>")
    local fdStr = nil
    for m in ret do
        print("m  "..m)
        local url = convertURL(m)
        print(" url  "..url)
        fdStr = m
        break
    end

    if fdStr ~= nil then
        return string.gsub(ss, "<a href=\"(.-)\"></a>", fdStr)
    end

    return ss
end


local b = "<a href=\"<color=#ffe760><i><b>www<day>.baidu.com</b></i></color>\"></a>"

matchURL(b)
