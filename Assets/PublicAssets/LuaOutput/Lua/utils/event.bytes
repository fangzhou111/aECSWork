local event = {
    _listeners = {},
    _typelistenerids = {}
}

local _clock
local _num = 0
local getguid = function()
    local clock = tostring(os.clock())
    if clock == _clock then
        _num = _num + 1
    else
        _num = 0
    end
    _clock = clock
    return string.format("%s_%d", _clock, _num)
end

function event.listen(eventtype, handler, obj, once)
    local id = getguid()
    event._typelistenerids[eventtype] = event._typelistenerids[eventtype] or {}
	table.insert(event._typelistenerids[eventtype], id)

    event._listeners[id] = {
        eventtype = eventtype,
        handler = handler,
        obj = obj,
        once = once
    }
    return id
end

-- events 可以包含两种格式
-- 1、数组式，收到事件无论如何都回调
-- 2、kv式，k为事件类型，v为过滤函数，通过过滤的事件再回调
function event.listenevts(events, handler, obj, once)
	local listenids = {}
	local rawhandler = once and function(obj, evt)
        for k,v in ipairs(listenids) do
            event.unlisten(v)
        end
        handler(obj, evt)
    end or handler
	
    local directevts = {}
    for _, eventtype in ipairs(events) do
        table.insert(listenids, event.listen(eventtype, rawhandler, obj))
        directevts[eventtype] = true
    end
    for eventtype, func in pairs(events) do
        if type(eventtype) == "string" then
            table.insert(listenids, event.listen(eventtype, function(obj, evt)
                if func(evt) then rawHandler(obj, evt) end
            end, obj))
        end
    end
	
	return listenids
end

function event.unlisten(id)
    if event._listeners[id] then
        local eventtype = event._listeners[id].eventtype
		for i,v in ipairs(event._typelistenerids[eventtype]) do
			if v == id then
				table.remove(event._typelistenerids[eventtype], i)
				break
			end
		end
        event._listeners[id] = nil
    end
end

function event.dispatchtype(eventtype)
    event.dispatch({type = eventtype})
end

function event.dispatch(evt)
    local ids = event._typelistenerids[evt.type] or {}
    local unlistenindexs = {}
    for index,id in ipairs(ids) do
        local listener = event._listeners[id]
        if listener then
            listener.handler(listener.obj, evt)
            if listener.once then table.insert(unlistenindexs, index) end
        end
    end

    -- quick unlisten
    for i = 1,#unlistenindexs do
        local index = unlistenindexs[i] - i + 1
        event._listeners[ids[index]] = nil
        table.remove(ids, index)
    end
end


return event