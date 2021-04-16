ecs.system.create("game.tick")
:setinitialize(function(self)

end)
:setexecute(function(self)	
	if not self.pool.istickpause and self.pool.hastick then        
		self.pool:replacetick(self.pool.tick.curtick + 1)

		if self.pool.tick.curtick > 1000000 then
			self.pool:replacetick(0)
		end

		utils.time.update()
	end
end)