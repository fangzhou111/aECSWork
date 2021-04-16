local sort = {}

local function partion(array,left,right,compareFunc)
	local key = array[left] -- 哨兵  一趟排序的比较基准
	local index = left
	array[index],array[right] = array[right],array[index] -- 与最后一个元素交换
	local i = left
	while i< right do
		if compareFunc( key,array[i]) then
			array[index],array[i] = array[i],array[index]-- 发现不符合规则 进行交换
			index = index + 1
		end
		i = i + 1
	end
	array[right],array[index] = array[index],array[right] -- 把哨兵放回
	return index;
end

local function quick(array,left,right,compareFunc)
	if(left < right ) then
		local index = partion(array,left,right,compareFunc)
		quick(array,left,index-1,compareFunc)
		quick(array,index+1,right,compareFunc)
	end
end

function sort.quicksort(array,compareFunc)
	quick(array,1,#array,compareFunc)
end

return sort