function initcom()
	

	btn1 = mainview:GetChild("n0")
	btn2 = mainview:GetChild("n2")

	label = mainview:GetChild("n1")

	count = 1

	btn1.onClick:Set(function ()
		label.text = count

		count = count + 1
	end)

	btn2.onClick:Set(function ()
		close()
	end)

	ecstrigger("game")
	:addtrigger(ecs.matcher.pos.added)
	:on(function(entities, count)
		local str = ""
		for i=1,count do
			local e = entities[i]

			if e.isdog then
				str = str.." dog pos z:"..e.pos.v3.z
			elseif e.ispig then
				str = str.." pig pos z:"..e.pos.v3.z
			end
		end

		label.text = str
	end)

	ecstrigger("game")
	:addtrigger(ecs.matcher.gameend.added)
	:on(function(entities, count)
		label.text = "gameend"
	end)
end

function show()
	print("show ui")
end

function onclose()
	-- body
	print("close ui")
end