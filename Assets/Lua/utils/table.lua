local tableutils = {}

function tableutils.copy(obj)
    local ret = {}
    for k, v in pairs(obj) do
        if type(v) == "table" then
            ret[k] = tableutils.copy(v)
        else
            ret[k] = v
        end
    end
    return ret
end

function tableutils.get(t, k)
	local arr = utils.string.split(k, "[.]")
	local result = t
	for i = 1, #arr do
		result = result[arr[i]]
		if result == nil then return end
	end
	return result
end

function tableutils.set(t, k, v)
	local arr = utils.string.split(k, "[.]")
	local result = t
	for i = 1, #arr-1 do
		local temp = result[arr[i]]
		if temp ~= nil then
			result = temp
		else
			for j = i,#arr-1 do
				temp = {}
				result[arr[j]] = temp
				result = temp
			end
			break
		end
	end

	local lastkey = arr[#arr]
	result[lastkey] = v
	return lastkey
end

function tableutils.tryget(t, k)
	local result = tableutils.get(t, k)
	if result == nil then
		result = {}
		tableutils.set(t, k, result)
	end
	return result
end

local keysdict = {}
function keysdict.__index(t, k)
	if k == "count" then
		return t.__count
	elseif k == "keys" then
		local keys = rawget(t, "__keys")
		if keys == nil then
			keys = {}
			for dk,_ in pairs(t.__data) do
				table.insert(keys, dk)
			end
			rawset(t, "__keys", keys)
		end
		return keys
	else
		return t.__data[k]
	end
end
function keysdict.__newindex(t, k, v)
	if k == "count" or k == "keys" then error(k.." is retained for keysdict") end
	if t.__data[k] == nil then
		if v == nil then return end
		if rawget(t, "__keys") then table.insert(t.__keys, k) end
		t.__count = t.__count + 1
	elseif v == nil then
		rawset(t, "__keys", nil)
		t.__count = t.__count - 1
	end
	t.__data[k] = v
end

function tableutils.makekeysdict()
	local dict = {}
	dict.__data = {}
	dict.__count = 0
	return setmetatable(dict, keysdict)
end


function tableutils.find(orderarr, value, compare, from, to)
	local cv
	if from == nil then
		from, to = 1, #orderarr
		if to == 0 then return 0 end

		if compare == nil then
			compare = type(value) == "table" and tableutils.compare.array or tableutils.compare.number
		end

		cv = compare(value, orderarr[from])
		if cv == 0 then
			return from
		elseif compare(value, orderarr[to]) >= 0 then
			return to
		elseif cv < 0 then
			return 0
		end
	end

	if from == to or to - from == 1 then return from end
	local checkindex = math.floor((from + to) / 2)
	cv = compare(value, orderarr[checkindex])
	if cv == 0 then
		return checkindex
	elseif cv < 0 then
		return tableutils.find(orderarr, value, compare, from, checkindex)
	else
		return tableutils.find(orderarr, value, compare, checkindex, to)
	end
end

local orderdictfucs = {
	__index = function(t, k) return t.__data[k] end,
	__newindex = function(t, k, v)
		if t.__data[k] == v then return end
		local i
		if t.__data[k] ~= nil then
			i = tableutils.find(t.__values, t.__data[k], t.__compare)
			table.remove(t.__values, i)
			table.remove(t.__keys, i)
		end

		t.__data[k] = v
		if v == nil then return end
		
		i = tableutils.find(t.__values, v, t.__compare)
		if i > 0 and t.__compare(v, t.__values[i]) == 0 then
			log.w("orderdict do not support same order value")
			log.w(debug.traceback())
		end

		if i == #t.__keys then
			table.insert(t.__keys, k)
			table.insert(t.__values, v)
		else
			table.insert(t.__keys, i+1, k)
			table.insert(t.__values, i+1, v)
		end
	end,
	ipairs = function(t)
		local i = 0
		local rk,rv
		return function()
			i = i + 1
			if t.__keys[i] then
				return i, t.__keys[i]
			end
		end
	end,
	getlen = function(t)
		return #t.__keys
	end,
	getat = function(t,i)
		return t.__keys[i]
	end,
	getlast = function (t)
		return t.__keys[#t.__keys]
	end

}

tableutils.compare = {}
function tableutils.compare.number(a, b) return a-b end
function tableutils.compare.array(a, b)
	local v
	for i=1,#a do
		v = a[i] - b[i]
		if v ~= 0 then return v end
	end
	return 0
end
function tableutils.compare.makearr(...)
	local args = {...}
	return function(a, b)
		local v
		for i=1,#a do
			v = a[i] - b[i]
			if v ~= 0 then return args[i] and v or -v end
		end
		return 0
	end
end

function tableutils.createorderdict(compare)
	local dict = {
		__keys = {},
		__values = {},
		__data = {},
		__compare = compare,
		ipairs = orderdictfucs.ipairs,
		getlen = orderdictfucs.getlen,
		getat = orderdictfucs.getat,
		getlast = orderdictfucs.getlast
	}
	setmetatable(dict, orderdictfucs)
	return dict
end


local sortdictmetatable = {
	__index = function(t, k) return t.__data[k] end,
	__newindex = function(t, k, v)
		if v ~= nil then
			if t.__data[k] == nil then
				if t.__end == nil then
					rawset(t, "__first", k)
					rawset(t, "__end", k)
				else
					t.__nxt[t.__end] = k
					t.__pre[k] = t.__end
					t.__end = k
				end
			end
			t.__data[k] = v
			t.count = t.count + 1
		elseif t.__data[k] ~= nil then
			t.__data[k] = nil

			if t.__pre[k] == nil then
				t.__first = t.__nxt[k]
				if t.__first then t.__pre[t.__first] = nil end
			else
				t.__nxt[t.__pre[k]] = t.__nxt[k]
			end

			if t.__nxt[k] == nil then
				t.__end = t.__pre[k]
				if t.__end then t.__nxt[t.__end] = nil end
			else
				t.__pre[t.__nxt[k]] = t.__pre[k]
			end

			t.__nxt[k] = nil
			t.__pre[k] = nil
			t.count = t.count - 1
		end
	end,
	__pairs = function(self)
		local k = self.__first
		local rk,rv
		return function()
			if k ~= nil then
				rk,rv = k, self.__data[k]
				k = self.__nxt[k]
				return rk,rv
			end
		end
	end,
	__getfirst = function(self)
		return self.__first ~= nil and self.__data[self.__first] or nil
	end,
	__debug = function(self)
		if self.__first == nil then return "empty" end

		local ret = "["..self.__first
		local key = self.__nxt[self.__first]
		while key ~= nil do
			ret = ret.. "," .. key
			key = self.__nxt[key]
		end
		ret = ret .. "],["..self.__end
		key = self.__pre[self.__end]
		while key ~= nil do
			ret = ret.. "," .. key
			key = self.__pre[key]
		end
		ret = ret .. "]"

		return ret
	end
}
function tableutils.createsortdict()
	local dict = {
		__first = nil,
		__end = nil,
		__nxt = {},
		__pre = {},
		__data = {},
		count = 0,
		pairs = sortdictmetatable.__pairs,
		getfirst = sortdictmetatable.__getfirst,
		debug = sortdictmetatable.__debug
	}
	setmetatable(dict, sortdictmetatable)
	return dict
end

return tableutils