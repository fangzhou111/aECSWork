local stringutils = {}

function stringutils.split(s, p)
    if s == nil or string.len(s) == 0 then
        return {}
    end

    local result = {}
    while true do
        local pos, endp = string.find(s, p)
        if not pos then
            result[#result + 1] = s
            break
        end
        local sub_str = string.sub(s, 1, pos - 1)
        result[#result + 1] = sub_str
        s = string.sub(s, endp + 1)
    end
    return result
end

function stringutils.to01(str)
    return string.gsub(
        str,
        ".",
        function(c)
            return string.format("%02x", string.byte(c))
        end
    )
end

function stringutils.CountDownStr(time, showSec, fix)
    fix = fix == nil and ":" or fix
    if time > 0 then
        local mEndDay = math.floor(time / 86400)

        local timeHour = math.fmod(math.floor(time / 3600), 24)
        local timeMinute = math.fmod(math.floor(time / 60), 60)
        local timeSecond = math.floor(math.fmod(time, 60))

        if showSec then
            if timeHour < 10 then
                time = "0" .. timeHour .. fix
            else
                time = timeHour .. fix
            end
            if timeMinute < 10 then
                time = time .. "0" .. timeMinute .. fix
            else
                time = time .. timeMinute .. fix
            end
        else
            time = timeHour .. TEXT_ENUM.TimeStr.Hour .. timeMinute .. TEXT_ENUM.TimeStr.Min
        end

        if showSec then
            if timeSecond < 10 then
                time = time .. "0" .. timeSecond
            else
                time = time .. timeSecond
            end
        end
        return mEndDay, time, false
    else
        return 0, "00" .. fix .. "00" .. fix .. "00", true
    end
end

function stringutils.CountDownStr2(endDaySec, showSec)
    local time = endDaySec - playerapi.getglobaltime()
    return utils.string.CountDownStr(time, showSec)
end

function stringutils.Convertedto24hours(time)
    if time > 0 then
        local timeHour = math.floor(time / 3600)
        local timeMinute = math.fmod(math.floor(time / 60), 60)
        local timeSecond = math.floor(math.fmod(time, 60))

        if timeHour < 10 then
            time = "0" .. timeHour .. ":"
        else
            time = timeHour .. ":"
        end
        if timeMinute < 10 then
            time = time .. "0" .. timeMinute .. ":"
        else
            time = time .. timeMinute .. ":"
        end
        if timeSecond < 10 then
            time = time .. "0" .. timeSecond
        else
            time = time .. timeSecond
        end

        return time
    else
        return "00:00:00"
    end
end

--获得倒计时 cuttype:1 "分:秒"，2 "时：分:秒"
function stringutils.getcountdownstr(time, cuttype)
    local hour = math.floor(time / 3600)
    local min = math.floor((time - hour * 3600) / 60)
    local sec = (time - hour * 3600) % 60
    if cuttype == 1 then
        return string.format("%02d:%02d", min, sec)
    else
        return string.format("%02d:%02d:%02d", hour, min, sec)
    end
end

function stringutils.offlineTimeStr(offTime, notShowSec)
    local str = TEXT_ENUM.TimeStr.offTime
    local timeStr = math.floor(offTime)
    if offTime < 60 then
        if notShowSec then
            return str .. "1 " .. TEXT_ENUM.TimeStr.Minute
        else
            return str .. timeStr .. " " .. TEXT_ENUM.TimeStr.Sec
        end
    elseif offTime < 3600 then
        timeStr = math.floor(offTime / 60)
        return str .. timeStr .. " " .. TEXT_ENUM.TimeStr.Minute
    elseif offTime < 86400 then
        timeStr = math.floor(offTime / 3600)
        return str .. timeStr .. " " .. TEXT_ENUM.TimeStr.Hour
    elseif offTime < 2592000 then
        timeStr = math.floor(offTime / 86400)
        return str .. timeStr .. " " .. TEXT_ENUM.TimeStr.D
    else
        timeStr = math.ceil(offTime / 2592000)
        return timeStr .. TEXT_ENUM.TimeStr.MonthsAgo
    end
end

-- x天x时x分
function stringutils.GetTimeToDayStr(offTime, notShowSec)
    local str = ""

    local timeStr = offTime > 0 and offTime or 0

    local timeDay = math.floor(timeStr / 86400)
    local timeHour = math.fmod(math.floor(timeStr / 3600), 24)
    local timeMinute = math.fmod(math.floor(timeStr / 60), 60)
    local timeSecond = math.floor(math.fmod(timeStr, 60))

    if timeDay > 0 then
        str = timeDay .. TEXT_ENUM.TimeStr.D
    end

    if timeHour > 0 then
        str = str .. timeHour .. TEXT_ENUM.TimeStr.Hour
    end

    if timeMinute > 0 then
        str = str .. timeMinute .. TEXT_ENUM.TimeStr.Min
    end

    if timeSecond >= 0 and not notShowSec then
        if timeStr < 3600 then
            str = str .. timeSecond .. TEXT_ENUM.TimeStr.Sec
        end
    end

    return str
end

-- 按格式转换时间戳
-- %Y 年 full
-- %m 月
-- %d 天
-- %h 时
-- %M 分
function stringutils.UTCTimeToDate(t, format)
    if t == 0 or t == nil then
        return 0
    end
    if format == nil then
        format = "%Y-%m-%d %H:%M"
    end
    local tab = os.date(format, t)
    return tab
end

function stringutils.UTCTimeToHour(t)
    if t == 0 then
        return
    end
    return math.floor((t % 86400) / 3600)
end

-- xx 时 xx 分
function stringutils.ChangeToTimeStr(t, showZero)
    if t <= 0 then
        return ""
    end

    local h = t / 3600
    local m = (t % 3600) / 60
    local s = t % 60
    local Hstr = ""
    local Mstr = ""
    local Sstr = ""

    if s > 0 or showZero then
        Sstr = math.floor(s) .. TEXT_ENUM.TimeStr.Sec
    end

    if m > 0 or showZero then
        Mstr = math.floor(m) .. TEXT_ENUM.TimeStr.Min
    end

    if h > 0 or showZero then
        Hstr = math.floor(h) .. TEXT_ENUM.TimeStr.Hour
    end
    return Hstr .. Mstr .. Sstr
end

--获得此时距离下一个整点的时间节点：如获得5点整的时间戳
function stringutils.getStampForHour(witchHour)
    local curTimeStamp = playerapi.getglobaltime()
    local curDate = os.date("*t", curTimeStamp)
    local hourStamp = os.time({year = curDate.year, month = curDate.month, day = curDate.day, hour = witchHour}) --今天witchHour整点的时间戳
    if curDate.hour > witchHour then
        hourStamp = hourStamp + 24 * 60 * 60
    end

    return hourStamp
end

-- 截取数字小数点后几位
function stringutils.SubNumber(t, num)
    local t = tostring(t)
    local pos = string.find(t, "[.]")
    if pos and string.len(t) > pos + num then
        t = string.sub(t, 1, pos + num)
    end
    return t
end

-- lua
-- 判断utf8字符byte长度
-- 0xxxxxxx - 1 byte
-- 110yxxxx - 192, 2 byte
-- 1110yyyy - 225, 3 byte
-- 11110zzz - 240, 4 byte
local function cnsize(char)
    if not char then
        print("not char")
        return 0
    elseif char > 240 then
        return 4
    elseif char > 225 then
        return 3
    elseif char > 192 then
        return 2
    else
        return 1
    end
end

-- 计算utf8字符串字符数, 各种字符都按一个字符计算
-- 例如utf8len("1你好") => 3
function stringutils.utf8len(str)
    local len = 0
    local currentIndex = 1
    while currentIndex <= #str do
        local char = string.byte(str, currentIndex)
        currentIndex = currentIndex + cnsize(char)
        len = len + 1
    end
    return len
end

--@brief 切割字符串，并用“...”替换尾部
--@param sName:要切割的字符串
--@return nMaxCount，字符串上限,中文字为2的倍数
--@param nShowCount：显示英文字个数，中文字为2的倍数,可为空
--@note         函数实现：截取字符串一部分，剩余用“...”替换
function stringutils.utf8sub(sName, nMaxCount, nShowCount, fixStr)
    if sName == nil or nMaxCount == nil then
        return
    end
    fixStr = fixStr == nil and "..." or fixStr

    local sStr = sName
    local tCode = {}
    local tName = {}
    local nLenInByte = #sStr
    local nWidth = 0
    if nShowCount == nil then
        nShowCount = nMaxCount - 3
    end
    for i = 1, nLenInByte do
        local curByte = string.byte(sStr, i)
        local byteCount = 0
        if curByte > 0 and curByte <= 127 then
            byteCount = 1
        elseif curByte >= 192 and curByte < 223 then
            byteCount = 2
        elseif curByte >= 224 and curByte < 239 then
            byteCount = 3
        elseif curByte >= 240 and curByte <= 247 then
            byteCount = 4
        end
        local char = nil
        if byteCount > 0 then
            char = string.sub(sStr, i, i + byteCount - 1)
            i = i + byteCount - 1
        end
        if byteCount == 1 then
            nWidth = nWidth + 1
            table.insert(tName, char)
            table.insert(tCode, 1)
        elseif byteCount > 1 then
            nWidth = nWidth + 2
            table.insert(tName, char)
            table.insert(tCode, 2)
        end
    end

    if nWidth > nMaxCount then
        local _sN = ""
        local _len = 0
        for i = 1, #tName do
            _sN = _sN .. tName[i]
            _len = _len + tCode[i]
            if _len >= nShowCount then
                break
            end
        end
        sName = _sN .. fixStr
    end
    return sName
end

--@param nMaxCount，字符串上限,中文字为2的倍数
function stringutils.checkutf8subnum(sName, nMaxCount)
    if sName == nil or nMaxCount == nil then
        return
    end

    local sStr = sName
    local tCode = {}
    local tName = {}
    local nLenInByte = #sStr
    local nWidth = 0

    for i = 1, nLenInByte do
        local curByte = string.byte(sStr, i)
        local byteCount = 0
        if curByte > 0 and curByte <= 127 then
            byteCount = 1
        elseif curByte >= 192 and curByte < 223 then
            byteCount = 2
        elseif curByte >= 224 and curByte < 239 then
            byteCount = 3
        elseif curByte >= 240 and curByte <= 247 then
            byteCount = 4
        end

        local char = nil
        if byteCount > 0 then
            char = string.sub(sStr, i, i + byteCount - 1)
            i = i + byteCount - 1
        end
        if byteCount == 1 then
            nWidth = nWidth + 1
            table.insert(tName, char)
            table.insert(tCode, 1)
        elseif byteCount > 1 then
            nWidth = nWidth + 2
            table.insert(tName, char)
            table.insert(tCode, 2)
        end
    end

    if nWidth > nMaxCount then
        return true
    end
    return false
end

function stringutils.changnumtostrlist(num)
    local strlist = {}
    local str = tostring(num)
    for i = 1, string.len(str) do
        local newnum = tonumber(string.sub(str, i, i))
        table.insert(strlist, newnum)
    end
    return strlist
end

function stringutils.stringtonewline(sName, tonewlinecount)
    if sName == nil then
        return
    end
    tonewlinecount = tonewlinecount == nil and 8 or tonewlinecount

    local sStr = sName
    local tCode = {}
    local tName = {}
    local nLenInByte = #sStr
    local nWidth = 0

    for i = 1, nLenInByte do
        local curByte = string.byte(sStr, i)
        local byteCount = 0
        if curByte > 0 and curByte <= 127 then
            byteCount = 1
        elseif curByte >= 192 and curByte < 223 then
            byteCount = 2
        elseif curByte >= 224 and curByte < 239 then
            byteCount = 3
        elseif curByte >= 240 and curByte <= 247 then
            byteCount = 4
        end
        local char = nil
        if byteCount > 0 then
            char = string.sub(sStr, i, i + byteCount - 1)
            i = i + byteCount - 1
        end
        if byteCount == 1 then
            nWidth = nWidth + 1
            table.insert(tName, char)
            table.insert(tCode, 1)
        elseif byteCount > 1 then
            nWidth = nWidth + 2
            table.insert(tName, char)
            table.insert(tCode, 2)
        end
    end

    if nWidth > 16 then
        tonewlinecount = 10
    end

    if nWidth > tonewlinecount then
        local _sN = ""
        local _len = 0
        for i = 1, #tName do
            _sN = _sN .. tName[i]
            _len = _len + tCode[i]
            if _len >= tonewlinecount then
                _sN = _sN .. "\n"
                _len = 0
            end
        end
        return _sN
    end
    return sName
end

--将庞大大的数据，转化为万为单位的字符串
function stringutils.changenumtoshort(num)
    local str = num
    if num >= 10000 then
        str = math.floor(num / 10000) .. "W"
    end
    return str
end

--获得时间戳间隔现在的描述
function stringutils.getlasttimestr(time)
    local str = ""
    if time > 60 * 60 * 24 then
        str = math.floor(time / (60 * 60 * 24)) .. TEXT_ENUM.TimeStr.D
    elseif time > 60 * 60 then
        str = math.floor(time / (60 * 60)) .. TEXT_ENUM.TimeStr.H
    elseif time > 60 then
        str = math.floor(time / 60) .. TEXT_ENUM.TimeStr.Minute
    else
        str = math.floor(60 / 60) .. TEXT_ENUM.TimeStr.Minute
    end
    return str
end

--截取中英混合的UTF8字符串，endIndex可缺省
function stringutils.SubStringUTF8(str, startIndex, endIndex)
    if startIndex < 0 then
        startIndex = stringutils.SubStringGetTotalIndex(str) + startIndex + 1
    end

    if endIndex ~= nil and endIndex < 0 then
        endIndex = stringutils.SubStringGetTotalIndex(str) + endIndex + 1
    end

    if endIndex == nil then
        return string.sub(str, stringutils.SubStringGetTrueIndex(str, startIndex))
    else
        return string.sub(str, stringutils.SubStringGetTrueIndex(str, startIndex), stringutils.SubStringGetTrueIndex(str, endIndex + 1) - 1)
    end
end

--获取中英混合UTF8字符串的真实字符数量
function stringutils.SubStringGetTotalIndex(str)
    local curIndex = 0
    local i = 1
    local lastCount = 1
    repeat
        lastCount = SubStringGetByteCount(str, i)
        i = i + lastCount
        curIndex = curIndex + 1
    until (lastCount == 0)
    return curIndex - 1
end

function stringutils.SubStringGetTrueIndex(str, index)
    local curIndex = 0
    local i = 1
    local lastCount = 1
    repeat
        lastCount = stringutils.substringgetbytecount(str, i)
        i = i + lastCount
        curIndex = curIndex + 1
    until (curIndex >= index)
    return i - lastCount
end

--返回当前字符实际占用的字符数
function stringutils.substringgetbytecount(str, index)
    local curByte = string.byte(str, index)
    local byteCount = 1
    if curByte == nil then
        byteCount = 0
    elseif curByte > 0 and curByte <= 127 then
        byteCount = 1
    elseif curByte >= 192 and curByte <= 223 then
        byteCount = 2
    elseif curByte >= 224 and curByte <= 239 then
        byteCount = 3
    elseif curByte >= 240 and curByte <= 247 then
        byteCount = 4
    end
    return byteCount
end

return stringutils
