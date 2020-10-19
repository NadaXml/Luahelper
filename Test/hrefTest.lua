

--去除URL中的<修饰符>
function convertURL(url)
    local rt = string.gsub(url, "</?(%S-)>", function(a)
        return ""
    end)
    print(rt)
    return rt
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


local b = "<a href=\"<color=#ffe760><i><b>www.baidu.com</b></i></color>\"></a>"

matchURL(b)
