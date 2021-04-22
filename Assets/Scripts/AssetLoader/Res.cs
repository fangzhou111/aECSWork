using Object = UnityEngine.Object;

namespace SuperMobs.Game.AssetLoader
{
    public class Res
    {
        public Object o;//实例
        public int useIndex = -1;//第几次被使用
        public float ratainTime = float.NaN;//自动回收时间

        public Res(Object o, int index, float time)
        {
            this.o = o;
            useIndex = index;
            ratainTime = time;
        }

        public void Update(float dt)
        {
            if (!float.IsNaN(ratainTime))
                ratainTime -= dt;
        }

        public void Dispose()
        {
            if (o != null)
                Object.Destroy(o);

            o = null;
        }
    }
}