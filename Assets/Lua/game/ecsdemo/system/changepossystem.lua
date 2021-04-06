ecs.system.create("game.changepos")
:setinitialize(function(self)
	
end)
:setreactiveexecute(function(self, entities, count)
	for i = 1,count do
		local e = entities[i]

		if e.isdog then
			print("dog position",e.pos.v3.x,e.pos.v3.y,e.pos.v3.z)
		elseif e.ispig then
			print("pig position",e.pos.v3.x,e.pos.v3.y,e.pos.v3.z)
		end
	end
end)
:addtrigger(ecs.matcher.pos.added)