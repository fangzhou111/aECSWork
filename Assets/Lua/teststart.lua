DEBUG = true

require "utils.init"
require "supermobs.init"
require "game.init"


ecs.start()

ui.addpackage("Test")
:add("myui","Out", "game.testui.testui",1,true)


print("start")

local gamepool = ecs.pool.create("game")

gamepool:settick(0)

local testsystems = ecs.feature.create(gamepool, "testsystems")
:addsystem(ecs.system.game.tick)

:addsystem(ecs.system.game.run)
:addsystem(ecs.system.game.changepos)
:addsystem(ecs.system.game.checkend)

testsystems:initialize()

function START()
	local s = rx.CooperativeScheduler.create()

	rx.Observable.fromRange(1, 8)
	  :filter(function(x) return x % 2 == 0 end)
	  :concat(rx.Observable.of('who do we appreciate'))
	  :map(function(value) return value .. '!' end)
	  :subscribe(print)

  	--[[ui.open("myui")

	local dog = gamepool:createentity()
	dog:adddog()
	:addpos(Vector3(0,0,0))
	:addspeed(3)

	local pig = gamepool:createentity()
	pig:addpig()
	pig:addpos(Vector3(0,0,0))
	pig:addspeed(1)]]--
end

function __UPDATE()
	testsystems:execute()
	
	ecs.hookupdate()
end

function BBB()
	for i=1,100000 do
		local e = ecs.pool.get("game"):createentity()
	end
end

function AAA()
	ecs.pool.get("game"):destroyallentity()
end

function LUAGC()
	collectgarbage()
end