using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperMobs.LuaUtils
{
    public class GOUtils
    {
        public static void SetPosition(GameObject go, float x, float y, float z)
        {
            go.transform.position = new Vector3(x, y, z);
        }

        public static void SetLocalPosition(GameObject go, float x, float y, float z)
        {
            go.transform.localPosition = new Vector3(x, y, z);
        }

        public static void SetRotation(GameObject go, float x, float y, float z)
        {
            go.transform.rotation = Quaternion.Euler(x, y, z);
        }

        public static void SetLocalRotation(GameObject go, float x, float y, float z)
        {
            go.transform.localRotation = Quaternion.Euler(x, y, z);
        }

        public static void SetLocalScale(GameObject go, float x, float y, float z)
        {
            go.transform.localScale = new Vector3(x, y, z);
        }
    }
}
