ecs = ecs or {}
ecs.null = {}
ecs.debug = DEBUG

ecs.setexplicitrelay = function(func, dep)
	local env = getfenv(func)
	setfenv(func, setmetatable({}, {__index = function(t, k)
		if k == "ECS_SHORT_TABLE_INDEX" then
			return getfenv(dep + 1 + (ecs.debug and 5 or 3)).ECS_SHORT_TABLE_INDEX
		end
		return env[k]
	end, __newindex = env}))
end

require "supermobs.ecs.pool"
require "supermobs.ecs.entity"
require "supermobs.ecs.component"
require "supermobs.ecs.matcher"
require "supermobs.ecs.group"
require "supermobs.ecs.observer"
require "supermobs.ecs.system"
require "supermobs.ecs.systems"
require "supermobs.ecs.debugsystems"
require "supermobs.ecs.bridge"

ecs.feature = ecs.debug and ecs.debugsystems or ecs.systems

-- short name for components
ecs.shorttablearray = {}
local function calculateshort(index)
	local shorttable = ecs.shorttablearray[index]
	for _,packagename in ipairs(shorttable) do
		local nameprefix = string.sub(packagename, 1, string.len(packagename) - 1)
		local packagecomponents = ecs.component.packagecomponents[packagename]
		if packagecomponents then
			for _,shortname in ipairs(packagecomponents) do
				if not shorttable[shortname] then
					shorttable[shortname] = nameprefix .. shortname
				end
			end
		end
	end
end

local function explicit(shorttable, dep)
	local env = getfenv(dep)
	if env == _G then
		table.insert(ecs.shorttablearray, shorttable)
		if ecs.started then calculateshort(#ecs.shorttablearray) end

		local env = {ECS_SHORT_TABLE_INDEX = #ecs.shorttablearray}
		setmetatable(env, {__index = _G, __newindex = _G})
		setfenv(dep, env)

		return #ecs.shorttablearray
	elseif env.ECS_SHORT_TABLE_INDEX == nil then
		log.w("the fenv has been changed, and do not by ecs")
		table.insert(ecs.shorttablearray, shorttable)
		if ecs.started then calculateshort(#ecs.shorttablearray) end
		env.ECS_SHORT_TABLE_INDEX = #ecs.shorttablearray
		return #ecs.shorttablearray
	else
		local raw = ecs.shorttablearray[env.ECS_SHORT_TABLE_INDEX]
		for k,v in pairs(shorttable) do
			if type(k) == "number" then
				table.insert(raw, v)
			else
				raw[k] = v
			end
		end
		if ecs.started then calculateshort(env.ECS_SHORT_TABLE_INDEX) end

		return env.ECS_SHORT_TABLE_INDEX
	end
end

ecs.explicit = function(shorttable)
	-- 兼容原生lua和luajit，请不要优化尾调用
	local index = explicit(shorttable, 3)
	return index
end

ecs.explicitrequire = function(shorttable, f)
	local f, err = loadfile(f)
	if not f then error(err) end
	explicit(shorttable, f)
	return f()
end

ecs.customexplicit = function(shorttable, env)
	table.insert(ecs.shorttablearray, shorttable)
	if ecs.started then calculateshort(#ecs.shorttablearray) end
	env.ECS_SHORT_TABLE_INDEX = #ecs.shorttablearray
end

-- init order
ecs.started = false
local componentfilesarray = {}
local systemfilesarray = {}
local moduleshorttables = {}
ecs.regmodule = function(componentfiles, systemfiles, shorttable)
	table.insert(componentfilesarray, componentfiles or {})
	table.insert(systemfilesarray, systemfiles or {})
	moduleshorttables[#systemfilesarray] = shorttable
end

ecs.start = function()
	for _,files in ipairs(componentfilesarray) do
		for _,f in ipairs(files) do
			require(f)
		end
	end
	componentfilesarray = nil

	for index = 1,#ecs.shorttablearray do
		calculateshort(index)
	end
	ecs.started = true

	for k,files in ipairs(systemfilesarray) do
		if not moduleshorttables[k] then
			for _,f in ipairs(files) do
				require(f)
			end
		else
			for _,f in ipairs(files) do
				ecs.explicitrequire(utils.table.copy(moduleshorttables[k]), f)
			end
		end
	end
	systemfilesarray = nil

	if ECS_INIT_EDITOR_ENTITY then ECS_INIT_EDITOR_ENTITY() end
	if ECS_INIT_EDITOR_POOL then ECS_INIT_EDITOR_POOL() end
	if ECS_INIT_EDITOR_SYSTEMS then ECS_INIT_EDITOR_SYSTEMS() end
end


-- listen, virtul system
local listensysss = {}
local listenid = 1
local listensysfuncs = {}
listensysfuncs["addtrigger"] = true
listensysfuncs["addtriggerindexfillter"] = true
listensysfuncs["setignoreselftriger"] = true
listensysfuncs["setensurematcher"] = true
listensysfuncs["setexcludematcher"] = true
local listenfuncs = {}
function listenfuncs:on(callback)
	if callback then
		self.system:setreactiveexecute(function(_, ...) callback(...) end)
	end
	listensysss[self.poolname]:addsystem(self.system)
	return self
end
function listenfuncs:off()
	listensysss[self.poolname]:remsystem(self.system)
end
function listenfuncs:destroy()
	self:off()
	ecs.system.destroy(self.system.name)
end

local listenmt = {
	__index = function(t, k)
		if listensysfuncs[k] then
			t.k = function(_, ...)
				t.system[k](t.system, ...)
				return t
			end
			return t.k
		end
		return listenfuncs[k]
	end
}

function ecs.hook(poolname)
	if not listensysss[poolname] then
		local ss = ecs.systems.create(ecs.pool.get(poolname), poolname .. "_listener")
		-- utils.event.listen("GLOBAL_FRAME_UPDATE", function()
		-- 	ss:execute()
		-- end)
		listensysss[poolname] = ss
	end

	local listener = {
		system = ecs.system.create(poolname .. "_listener_" .. listenid),
		poolname = poolname
	}
	listenid = listenid + 1
	return setmetatable(listener, listenmt)
end

function ecs.hookupdate()
	for _,ss in pairs(listensysss) do
		ss:execute()
	end
end