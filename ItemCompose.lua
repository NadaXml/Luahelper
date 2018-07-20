
--∫œ≥…≤È’“

local stIndex = {
    "q1",
    "q2",
    "q3",
    "q4",
    "q5",
}


local fd = 
{
	["q1"] = "",
    ["q2"] = 
	{
		item = "q1",
		count = 81,
		count_t = 3,
		cu = 300
	},
    ["q3"] = 
	{
		item = "q2",
		count = 27,
		count_t = 3,
		cu = 78,
	},
    ["q4"] = 
	{
		item = "q3",
		count = 9,
		count_t = 3,
		cu = 55,
	},
    ["q5"] = 
	{
		item = "q4",
		count = 3,
		count_t = 3,
		cu = 33
	}
}

local cc = 0

function Judge(key, left)
    local d = fd[key]
    if d ~= nil and d ~= "" then

        local who = d.cu + left * d.count_t
        local left2 = who % d.count
		print(left2)
        local own = (who - left2) / d.count
		print(own)
		cc = cc + own
        Judge(d.item, left2)
    else
        return 
    end
end

cc = 0
Judge("q5",0)

print("final  "..cc)
