-- ui脚本加载，编辑器环境不缓存
local function getscript(file)
	return loadfile(file)
end
if UnityEngine.Application.isEditor then
	getscript = function(file)
		print("load ui script " .. file)
		local f,err = loadfile(file)
		package.loaded[file] = nil
		if not f then error(err) end
		return f
	end
end

-- 在舞台显示的界面，以layer分组，每组是有序字典sortdict key是obj内存地址
local openpannels = {}
-- 单例界面，key是name
local singleobjs = {}
-- 所有的uiobjs字典，key是obj内存地址
local uiobjs = {}

local function obj__index(custom)
	return function(t, k)
		if custom[k] ~= nil then
			return custom[k]
		else
			return _G[k]
		end
	end
end

local function onviewdestroy(uiobj)
	-- 是否已经释放了
	if not uiobj.alive then return end
	-- 延时函数key 删除
	if uiobj.__timeoutkeys then
		for key,v in pairs(uiobj.__timeoutkeys) do
			utils.time.canceltimeout(key)
			uiobj.__timeoutkeys[key] = nil
		end
	end
	-- 自定义释放内容
	if uiobj.onclose then
		uiobj.onclose()
	end
	-- 释放侦听
	for _,listener in ipairs(uiobj.__ecslisteners) do
		listener:destroy()
	end
	-- 管理列表中删除
	uiobjs[uiobj.__key] = nil
	singleobjs[uiobj.__name] = nil
	if openpannels[uiobj.__config.layer] then
		openpannels[uiobj.__config.layer][uiobj.__key] = nil
	end
	-- 切换页删除
	if uiobj.__switchobjs then
		for _,v in pairs(uiobj.__switchobjs) do
			for _,vv in pairs(v) do
				vv.close()
			end
		end
	end
	-- 资源释放
	if uiobj._pkg then
		uiobj._pkg:release()
	end
	uiobj.alive = false
	
	for k,v in pairs(uiobj) do
		uiobj[k] = nil
	end
	uiobj.mainview = nil
	uiobj = nil
end

local function ecsgroup(uiobj)
	return function(poolname, matcher, func)
		local group = ecs.pool.get(poolname):getgroup(matcher)
		local listener = ecs.hook(poolname):addtrigger(matcher.addedorremoved)
		listener:on(function() func(group) end)
		func(group)
		table.insert(uiobj.__ecslisteners, listener)
	end
end

local function ecstrigger(uiobj)
	return function(poolname)
		local listener = ecs.hook(poolname)
		table.insert(uiobj.__ecslisteners, listener)
		return listener
	end
end

local function create(name, mainview)
	-- 读取配置
	local config = ui.panels[name]
	if not config then error("panel named " .. name .. " do not exist") end
	local pkg = ui.packages[config.pkg]

	-- 创建界面对象
	-- 创建隔离环境
	local obj = setmetatable({}, {__index = obj__index({
		__name = name, __config = config, __ondestroy = onviewdestroy})})
	obj.__key = tostring(obj)
	obj.alive = true
	obj.__ecslisteners = {}
	-- 为环境添加方法
	obj.ecsgroup = ecsgroup(obj)
	obj.ecstrigger = ecstrigger(obj)
	obj.close = function()
		if not mainview then
			obj.mainview:Dispose()
		end
		obj:__ondestroy()
	end
	obj.settimeout = function(timespan, callback)
		obj.__timeoutkeys = obj.__timeoutkeys or {}
		local key = utils.time.settimeout(timespan, callback)
		obj.__timeoutkeys[key] = key
	end
	-- 添加ecs声明
	if config.ecsexplicit then
		ecs.customexplicit(config.ecsexplicit, obj)
	end
	-- 载入脚本
	if config.spt == nil then
		obj.initcom = function() end
		obj.show = function() end
	else
		setfenv(getscript(config.spt), obj)()
	end
	-- 设置显示对象
	if mainview then
		obj.mainview = mainview
	else
		obj._pkg = pkg
		obj._pkg:retain()
		obj.mainview = FairyGUI.UIPackage.CreateObject(pkg.name, config.com).asCom
		obj.mainview.fairyBatching = true
	end

	-- 添加到管理列表
	uiobjs[obj.__key] = obj
	FairyGUI.Helper.ListenDestroy(obj.mainview,function() obj:__ondestroy() end)

	
	if config.single then singleobjs[name] = obj end
	
	-- 初始化界面
	obj.initcom()

	return obj
end

-- 打开一个界面显示到舞台上
function ui.open(name, ...)
	local config = ui.panels[name]
	
	-- 单例检查
	if singleobjs[name] then return singleobjs[name] end
	
	local obj = create(name)

	-- 添加到界面管理列表
	openpannels[config.layer] = openpannels[config.layer] or utils.table.createsortdict()
	openpannels[config.layer][obj.__key] = obj
	
	-- 刷新层级关系
	local order = config.layer * 1000
	for k,uiobj in openpannels[config.layer]:pairs() do
		order = order + 1
		obj.mainview.sortingOrder = order
	end

	-- 添加到舞台
	FairyGUI.GRoot.inst:AddChild(obj.mainview)
	obj.mainview:SetSize(FairyGUI.GRoot.inst.width, FairyGUI.GRoot.inst.height)
	obj.mainview:AddRelation(FairyGUI.GRoot.inst, FairyGUI.RelationType.Size)

	-- 显示
	obj.show(...)

	return obj
end

-- 把一个组件实例当做xx子界面
function ui.apply(name, com, ...)
	local uiobj = create(name, com)
	uiobj.show(...)
	return uiobj
end

-- 加载一个子界面到com上
function ui.load(name, com, dispose, ...)
	-- 删除原有子对象
	while com.numChildren > 0 do
		com:RemoveChildAt(0, dispose and true or false)
	end
	-- 创建子界面
	local obj = create(name)
	local config = ui.panels[name]
	obj.show(...)
	-- 添加到显示列表
	com:AddChild(obj.mainview)
	obj.mainview:SetSize(com.width, com.height)
	obj.mainview:AddRelation(com, FairyGUI.RelationType.Size)

	return obj
end

-- com上的子界面替换
function ui.replace(uiobj, com)
	-- 删除原有子对象
	while com.numChildren > 0 do
		com:RemoveChildAt(0, false)
	end
	-- 换子界面
	com:AddChild(uiobj.mainview)
	uiobj.mainview:SetSize(com.width, com.height)
	uiobj.mainview:AddRelation(com, FairyGUI.RelationType.Size)
end

-- 获取一个打开的单例界面
function ui.get(name)
	return singleobjs[name]
end

-- 获得当前单利界面个数
function ui.getsingleobjs()
	local uis = {}
	for _,v in pairs(singleobjs) do
		table.insert(uis,v)
	end

	return uis
end

--关闭全部ui
function ui.closeall()
	for _,layer in pairs(openpannels) do
		for _,obj in layer:pairs() do
			obj.close()
		end
	end
end
