local package = {}
package.__index = package

function package:new(name)
	local p = {}
	p.name = name
	p.deps = {}
	p.refcount = 0
	setmetatable(p, package)
	return p
end

--依赖一个包
function package:dep(name)
	table.insert(self.deps, name)
	return self
end

--添加一个界面
function package:add(name, component, script, layer, single, ecsexplicit)
	ui.panels[name] = {}
	ui.panels[name].pkg = self.name
	ui.panels[name].com = component
	ui.panels[name].spt = script
	ui.panels[name].layer = layer or ui.defaultlayer
	ui.panels[name].single = single
	ui.panels[name].ecsexplicit = ecsexplicit or ui.defaultecsexplicit
	return self
end

--加载资源
function package:retain()
	if self.refcount == 0 then
		FairyGUI.Helper.AddPackage(self.name)
	end
	
	self.refcount = self.refcount + 1
	for _,dep in ipairs(self.deps) do
		ui.packages[dep]:retain()
	end
end

--卸载资源
function package:release()
	self.refcount = self.refcount - 1
	for _,dep in ipairs(self.deps) do
		ui.packages[dep]:release()
	end
	
	if self.refcount == 0 then
		FairyGUI.Helper.RmovePackage(self.name)
	end
end

--添加一个包
ui.addpackage = function(name)
	local p = package:new(name)
	ui.packages[name] = p
	return p
end