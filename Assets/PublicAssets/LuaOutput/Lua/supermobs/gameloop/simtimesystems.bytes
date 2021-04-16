local simtimesystems = ecs.simtimesystems or {}
ecs.simtimesystems = simtimesystems

simtimesystems.__index = simtimesystems

simtimesystems.SIM_TIME_PER_FRAME = 33
simtimesystems.MAX_SIM_FRAMES_LAG_BEFORE_RESET = 30
simtimesystems.ROLLOVER_MILLISECONDS = 60000
simtimesystems.ROLLOVER_SECONDS = simtimesystems.ROLLOVER_MILLISECONDS / 1000
simtimesystems.LOW_SIM_TIME_PER_FRAME = simtimesystems.SIM_TIME_PER_FRAME / 6000

function simtimesystems.create(pool, name, timeperframe, guardfunc)
	local o = ecs.feature.create(pool, name)

	setmetatable(o, simtimesystems)	
	if getmetatable(simtimesystems) == nil then
		setmetatable(simtimesystems, ecs.feature)
	end

	o.timeperframe = timeperframe
	
	o.timeaccumulator = 0
    o.rollovernext = 0
	
	o:scaletime(1)
	o.timelast = o:now()	

    o._guardfunc = guardfunc
    o._pool = pool
	o._execute = ecs.feature.execute
	
	return o
end

function simtimesystems:scaletime(scale)
	self.scale = scale
	self.maxlag = self.timeperframe * self.MAX_SIM_FRAMES_LAG_BEFORE_RESET * scale
end

function simtimesystems:now()
	local time = utils.time.now()
	local t = time - self.rollovernext
	
	if t >= self.ROLLOVER_SECONDS then
		t = t - self.ROLLOVER_SECONDS
		self.rollovernext = self.rollovernext + self.ROLLOVER_SECONDS 
	end
	
	local ms = math.floor(t * 1000)	
	return ms
end

function simtimesystems:execute()
	if self._guardfunc(self._pool) then
		return
	end
	
	if self.timeperframe == 0 then
		self:_execute()
		return
	end
	
	local timethis = self:now()
	local dt = timethis < self.timelast and timethis + self.ROLLOVER_MILLISECONDS - self.timelast or timethis - self.timelast
	
	self.timelast = timethis
	
	dt = dt * self.scale
	self.timeaccumulator = self.timeaccumulator + dt
	
	local t = utils.time.realtimesincestartup()
	
	local simframecount = 0
	
	while self.timeaccumulator > self.timeperframe do
		self:_execute()
		
		self.timeaccumulator = self.timeaccumulator - self.timeperframe
		
		simframecount = simframecount + 1
		if simframecount == self.scale then
			if utils.time.realtimesincestartup() - t > self.LOW_SIM_TIME_PER_FRAME then
				break
			end
		elseif simframecount == self.scale + 1 then
			break
		end
	end
	
	if self.timeaccumulator > self.maxlag then
		self.timeaccumulator = 0
	end
end