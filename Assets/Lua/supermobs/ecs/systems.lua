local systems = ecs.systems or {}
ecs.systems = systems

systems.__index = systems

function systems.create(pool, name)
	local o = {}
	o.name = name
	o.pool = pool

	o.children = {}
	o.initializes = {}
	o.executes = {}

	o.initialized = false
	o.hasexecute = true

	setmetatable(o, systems)
	return o
end

function systems:initialize()
	for _,s in ipairs(self.initializes) do
		s:initialize()
	end
	self.initialized = true
end

function systems:execute()
	for _,s in ipairs(self.executes) do
		s:execute()
	end
end

function systems:clearobservers()
	for _,s in ipairs(self.executes) do
		s:clearobservers()
	end
end

function systems:addchild(child)
	if self.children[child.name] then
		error("the child named " .. child.name .. " has been added!")
	end

	if self.initialized then
		error("the systems you addto is already running")
	end

	self.children[child.name] = child

	if child.hasexecute then
		table.insert(self.executes, child)
	end

	if child.hasinitialize then
		table.insert(self.initializes, child)
		self.hasinitialize = true
	end

	return self
end


function systems:addsystem(system)
	return self:addchild(system.create(self.pool))
end

function systems:remsystem(system)
	local child = self.children[system.name]
	if child == nil then return end

	self.children[system.name] = nil
	if child.observer then child.observer:destroy() end

	if child.hasinitialize then
		for i,c in ipairs(self.initializes) do
			if c == child then
				table.remove(self.initializes, i)
				break
			end
		end
	end
	
	if child.hasexecute then
		for i,c in ipairs(self.executes) do
			if c == child then
				table.remove(self.executes, i)
				break
			end
		end
	end
end

function systems:add(name, ...)
	local ss = getmetatable(self).create(self.pool, name)
	for _,system in ipairs({...}) do
		ss:addsystem(system)
	end

	return self:addchild(ss)
end