local BitUtils = {}

local data32 = {}
for i=1,32 do
	data32[i]=2^(32-i)
end


function BitUtils:D2b(num)
	local tr = {}
	for i=1,32 do
		if num >= data32[i] then
			tr[i] = 1
			num = num - data32[i]
		else
			tr[i] = 0
		end
	end
	return tr
end

function BitUtils:B2d(tr)
	local num = 0
	for i=1,32 do
		num = num + tr[i] * data32[i]
	end
	return num
end


local funcCreater = function(func)
	return function(self, a, b)
		local tra = self:D2b(a)
		local trb = self:D2b(b)
		local tr = {}
		
		for i=1,32 do
			tr[i] = func(tra[i], trb[i]) and 1 or 0
		end
		
		return self:B2d(tr)
	end
end

BitUtils.Or = funcCreater(function(a, b)
	return a + b ~= 0
end)
BitUtils.Xor = funcCreater(function(a, b)
	return a ~= b
end)
BitUtils.And = funcCreater(function(a, b)
	return a * b == 1
end)


function BitUtils:Not(a)
	local tr = self:D2b(a)
	
	for i=1,32 do
		tr[i] = tr[i] == 1 and 0 or 1
	end
	
	return self:B2d(tr)
end


return BitUtils