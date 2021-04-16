ecs.system.create("game.checkend")
:setinitialize(function(self)
	self.group = self.pool:getgroup(ecs.matcher.speed,ecs.matcher.pos)
end)
:setexecute(function(self)	
	if not self.pool.isgameend then
		for _,e in self.group.entities:pairs() do
			if e.pos.v3.z > 100 then
				self.pool:setgameend()

				if e.isdog then
					print("dog win")
				elseif e.ispig then
					print("pig win")
				end

				print("gameend")
			end
		end
	end
end)