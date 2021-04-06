local timeutils = {
    timeoutspans = {},
    timeoutcallbacks = {}
}

local socket = nil

if UnityEngine then
	socket = require "socket"
else
	socket = {}
	
	socket.gettime = function ()
		return 0
	end
end

local starttime = socket.gettime()

function timeutils.realtimesincestartup()
	return (socket.gettime() - starttime)
end

function timeutils.now()
	return socket.gettime()
end

function timeutils.settimeout(timespan, callback)
    local key = timeutils.now() + timespan
    while timeutils.timeoutcallbacks[key] do
        key = key + 0.0001
    end
    timeutils.timeoutcallbacks[key] = callback
    table.insert(timeutils.timeoutspans, key)
    table.sort(timeutils.timeoutspans)
    return key
end

function timeutils.canceltimeout(key)
	timeutils.timeoutcallbacks[key] = nil
end

function timeutils.update()
    local now = timeutils.now()
    while timeutils.timeoutspans[1] and timeutils.timeoutspans[1] < now do
        local call = timeutils.timeoutcallbacks[timeutils.timeoutspans[1]]
        timeutils.timeoutcallbacks[timeutils.timeoutspans[1]] = nil
        table.remove(timeutils.timeoutspans, 1)
        if call then call() end
    end
end

return timeutils