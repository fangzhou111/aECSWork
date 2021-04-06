local ignoretimesystems = ecs.ignoretimesystems or {}
ecs.ignoretimesystems = ignoretimesystems

ignoretimesystems.__index = ignoretimesystems

function ignoretimesystems.create(pool, name, guardfunc)
	local o = ecs.feature.create(pool, name)

	setmetatable(o, ignoretimesystems)	
	if getmetatable(ignoretimesystems) == nil then
		setmetatable(ignoretimesystems, ecs.feature)
	end

    o._guardfunc = guardfunc
    o._pool = pool
	o._execute = ecs.feature.execute
	
	return o
end

function ignoretimesystems:execute()
	self:_execute()	
end