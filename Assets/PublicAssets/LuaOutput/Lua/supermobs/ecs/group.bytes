local group = ecs.group or {}
ecs.group = group
group.__index = group

group.eventtype = {
	added = 1, -- entity, componenttypeid, component
	removed = 2, -- entity, componenttypeid, component
	updated = 3 -- entity, componenttypeid, component, previouscomponent
}

function group.create(matcher)
	local o = {}
	o.matcher = matcher
	o.entities = utils.table.createsortdict()
	o.eventhandler = {}
	
	o.indexhandler = {}
	o.indexentities = {}
	o.indexentitieskeypre = {}
	o.indexentitieskey = {}

	o.index2order = {}
	o.orderfilters = {}
	o.orderkeys = {}
	o.orderindexs = {}

	o.desc = "Group(" .. o.matcher:tostring() .. ")"

	for _,v in pairs(group.eventtype) do
		o.eventhandler[v] = {}
	end
	setmetatable(o, group)
	return o
end

function group:addorderindex(ordername, filter, compare, ...)
	if self.orderindexs[ordername] then
		error(self.desc .. " already has a order index named " .. ordername)
	end

	local orderkey = {...}

	self.orderindexs[ordername] = utils.table.createorderdict(compare)
	self.orderfilters[ordername] = filter
	self.orderkeys[ordername] = orderkey

	for i,name in ipairs(orderkey) do
		if not self.indexentitieskey[name] then
			error(self.desc .. " do not has index named " .. name .. " for orderindex " .. ordername)
		end
		if not self.index2order[name] then
			self.index2order[name] = {}
		end
		table.insert(self.index2order[name], ordername)
	end

	for _,e in self.entities:pairs() do
		if not filter or filter(e) then
			local order = {}
			for i,name in ipairs(orderkey) do
				order[i] = self.indexentitieskey[name][e.__id]
			end
			order[#orderkey + 1] = e.__id
			self.orderindexs[ordername][e.__id] = order
		end
	end
end

function group:ipairs(ordername)
	local iter = self.orderindexs[ordername]:ipairs()
	return function()
		local _, id = iter()
		if id then return self.entities[id] end
	end
end

function group:hasindex(name, handler)
	if self.indexhandler[name] == nil then
		self:addindex(name, handler)
	end
	return self.indexhandler[name] ~= nil
end

function group:addindex(name, handler)
	if self.indexhandler[name] then
		error(self.desc .. " already has a index named " .. name)
	end

	self.indexhandler[name] = handler
	self.indexentities[name] = {}
	self.indexentitieskeypre[name] = {}
	self.indexentitieskey[name] = {}

	for _,e in self.entities:pairs() do
		local key = handler(e)
		if not self.indexentities[name][key] then
			self.indexentities[name][key] = utils.table.createsortdict()
		end
		self.indexentities[name][key][e.__id] = e
		self.indexentitieskey[name][e.__id] = key
	end
end

--[[
function group:remoeindex(name)
	if not self.indexhandler[name] then
		error(self.desc .. " do not has a index named " .. name)
	end
	if self.index2order[name] and #self.index2order[name] > 0 then
		error(self.desc .. " can not remoeindex " .. name .. ", orderindex " .. self.index2order[name][1] .. " is using it")
	end
	
	self.indexhandler[name] = nil
	self.indexentities[name] = nil
	self.indexentitieskeypre[name] = nil
	self.indexentitieskey[name] = nil
end
--]]

function group:getindexentity(name, key)
	local entities = self:getindexentities(name, key)
	return entities and entities:getfirst() or nil
end

function group:getindexentities(name, key)
	if not self.indexhandler[name] then
		error(self.desc .. " do not has a index named " .. name)
	end
	return self.indexentities[name][key]
end

function group:getindexentitiesdict(name)
	if not self.indexhandler[name] then
		error(self.desc .. " do not has a index named " .. name)
	end
	return self.indexentities[name]
end

function group:listen(eventtype, handler)
	table.insert(self.eventhandler[eventtype], handler)
end

function group:unlisten(eventtype, handler)
	local list = self.eventhandler[eventtype]
	for i = 1,#list do
		if list[i] == handler then
			return table.remove(list, i)
		end
	end
end

function group:getentityindexvalue(indexname, entity)
	return self.indexentitieskey[indexname][entity.__id]
end

function group:getentityindexprevalue(indexname, entity)
	return self.indexentitieskeypre[indexname][entity.__id]
end

function group:noticehandler(eventtype, entity, componenttypeid, component, previouscomponent)
	-- 添加或删除在 group:handleentitysilently 中处理
	if eventtype == group.eventtype.updated then
		local valuechangeindexs = {}
		for name,handler in pairs(self.indexhandler) do
			local key = handler(entity)
			local oldkey = self.indexentitieskey[name][entity.__id]
			if key ~= oldkey then
				if oldkey ~= nil then
					self.indexentities[name][oldkey][entity.__id] = nil
				end
				if not self.indexentities[name][key] then
					self.indexentities[name][key] = utils.table.createsortdict()
				end
				self.indexentities[name][key][entity.__id] = entity
				valuechangeindexs[name] = true
			end
			self.indexentitieskeypre[name][entity.__id] = oldkey
			self.indexentitieskey[name][entity.__id] = key
		end

		for ordername,orderkeys in pairs(self.orderkeys) do
			local filter = self.orderfilters[ordername]
			if not filter or filter(entity) then
				local order = {}
				local orderchange = false
				for i,name in ipairs(orderkeys) do
					order[i] = self.indexentitieskey[name][entity.__id]
					orderchange = orderchange or valuechangeindexs[name]
				end
				if orderchange then
					order[#orderkeys + 1] = entity.__id
					self.orderindexs[ordername][entity.__id] = order
				end
			else
				self.orderindexs[ordername][entity.__id] = nil
			end
		end
	end
	
	for _,v in ipairs(self.eventhandler[eventtype]) do
		v(self, entity, componenttypeid, component, previouscomponent)
	end
end

function group:handleentitysilently(entity)
	if self.matcher:matches(entity) then
		if not self.entities[entity.__id] then
			self.entities[entity.__id] = entity
			
			for name,handler in pairs(self.indexhandler) do
				local key = handler(entity)
				if key ~= nil then
					if not self.indexentities[name][key] then
						self.indexentities[name][key] = utils.table.createsortdict()
					end
					self.indexentities[name][key][entity.__id] = entity
					self.indexentitieskey[name][entity.__id] = key
					self.indexentitieskeypre[name][entity.__id] = nil
				end
			end

			for ordername,orderkeys in pairs(self.orderkeys) do
				local filter = self.orderfilters[ordername]
				if not filter or filter(entity) then
					local order = {}
					for i,name in ipairs(orderkeys) do
						order[i] = self.indexentitieskey[name][entity.__id]
						if order[i] == nil then
							order = nil
							break
						end
					end
					order[#orderkeys + 1] = entity.__id
					self.orderindexs[ordername][entity.__id] = order
				end
			end

			entity:retain(self)
			return group.eventtype.added
		end
	else
		if self.entities[entity.__id] then
			self.entities[entity.__id] = nil

			for name,_ in pairs(self.indexhandler) do
				local key = self.indexentitieskey[name][entity.__id]
				if key ~= nil then
					self.indexentities[name][key][entity.__id] = nil
				end
				self.indexentitieskey[name][entity.__id] = nil
				self.indexentitieskeypre[name][entity.__id] = nil
			end

			for ordername,orderkeys in pairs(self.orderkeys) do
				self.orderindexs[ordername][entity.__id] = nil
			end

			entity:release(self)
			return group.eventtype.removed
		end
	end
end

function group:handleentity(entity, componenttypeid, component)
	local eventtype = self:handleentitysilently(entity)
	if eventtype then
		for _,v in ipairs(self.eventhandler[eventtype]) do
			v(self, entity, componenttypeid, component)
		end
	end
end

function group:getsingleentity()
	if self.entities.count > 1 then
		error("can not get single entity from group " .. self.matcher.id)
	end

	for _,v in self.entities:pairs() do
		return v
	end
end

function group:getorderentity(ordername, orderindex)
	local dict = self.orderindexs[ordername]
	local eid = dict.__keys[orderindex]
	return eid and self.entities[eid] or nil
end