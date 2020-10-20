

--去除URL中的<修饰符>
function convertURL(str)

    return string.gsub(str, "<[color|b|size|i|material|quad|/color|/b|/size|/i|/material|/quad][^>]*>", "")

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

--匹配出<a href=""></a>中的URL文本
function matchURL(str)
    local pattern1 = "<a href=\"(.-)\">.-</a>"
    local pattern2 = "<a href=\".-\">(.-)</a>"
    local findURL = nil
    local retIter = string.gmatch(str, pattern1)
    for m in retIter do
        findURL = m
    end
    str = string.gsub(str, pattern2,
        function(a)
            return a
        end)
    return str, findURL
end


local b = "<a href=\"www<day>.baidu.com\"><color=#ffe760><i><b>www<day>.baidu.com</b></i></color></a>"

print(matchURL(b))
