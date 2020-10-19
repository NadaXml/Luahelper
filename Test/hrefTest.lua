

local b = "<a href=\"<color=#ffe760><i><b>www.baidu.com</b></i></color>\"></a>"


local ret = string.gmatch(b, "<a href=\"(.-)\"></a>")


for m in ret do
    print(m)
    local r1 = string.gsub(m, "<.->", "")
    print(r1)
end
