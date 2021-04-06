ecs.system.create("game.run")
:setinitialize(function(self)
	self.group = self.pool:getgroup(ecs.matcher.speed,ecs.matcher.pos)
end)
:setexecute(function(self)	
	if self.pool.tick.curtick % 30 == 0 then
		for _,e in self.group.entities:pairs() do
			if not self.pool.isgameend then
				e:replacepos(Vector3(e.pos.v3.x,e.pos.v3.y,e.pos.v3.z + e.speed.value))
			end
		end
	end
end)