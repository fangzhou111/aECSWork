local viewtimesystems = ecs.viewtimesystems or {}
ecs.viewtimesystems = viewtimesystems

viewtimesystems.__index = viewtimesystems

viewtimesystems.MAX_PHYS_FRAMES_PER_VIEW_FRAME = 5


function viewtimesystems.create(pool, name, timeperframe)
	local o = ecs.feature.create(pool, name)

	setmetatable(o, viewtimesystems)	
	if getmetatable(viewtimesystems) == nil then
		setmetatable(viewtimesystems, ecs.feature)
	end
	
	o.timeperframe = timeperframe
	o.timelast = o:now()	

	o._execute = ecs.feature.execute
	
	return o
end

function viewtimesystems:now()	
	return utils.time.now()
end

function viewtimesystems:execute()	
	local timethis = self:now()
	local dt = timethis - self.timelast 
	self.timelast = timethis
	
	local timeaccumulator = dt
	local physframecount = 0
	
	while timeaccumulator > 0 do
		local deltatime = timeaccumulator > self.timeperframe and self.timeperframe or timeaccumulator
		
		self:_execute()
		
		physframecount = physframecount + 1
		if physframecount == self.MAX_PHYS_FRAMES_PER_VIEW_FRAME then
			break
		end
		
		timeaccumulator = timeaccumulator - deltatime
	end
end