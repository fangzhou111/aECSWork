// /********************************************************************
//                  Copyright (c) 2021, IGG China R&D 3
//                          All rights reserved
// 
//     创建日期： 2021年04月15日 14:51
//     文件名称： ResConfig.cs
//     说    明：Description
// 
//     当前版本： 1.00
//     作    者：huangchaoqun
//     概    述：Summary
// 
// *********************************************************************/

namespace SuperMobs.Game.AssetLoader
{
    public class ResConfig
    {
        public uint Max = 100000;
        public uint Min = 4;
        public OwnerLevel OwnerLevel = OwnerLevel.Scene;
        public float StartAutoDestroyTime = 30f;
        public float DestroyIntervalTime = 1f;
    }
}