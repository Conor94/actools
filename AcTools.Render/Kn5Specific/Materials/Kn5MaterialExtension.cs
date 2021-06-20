using AcTools.ExtraKn5Utils.Kn5Utils;
using AcTools.Kn5File;
using AcTools.Render.Base.Shaders;
using AcTools.Render.Kn5Specific.Textures;
using SlimDX;
using SlimDX.Direct3D11;

namespace AcTools.Render.Kn5Specific.Materials {
    public static class Kn5MaterialExtension {
        public static float GetPropertyValueAByName(this Kn5Material mat, string name, float defaultValue = 0.0f) {
            var property = mat.GetPropertyByName(name);
            return property?.ValueA ?? defaultValue;
        }

        public static Vector3 GetPropertyValueCByName(this Kn5Material mat, string name, Vector3 defaultValue) {
            var property = mat.GetPropertyByName(name);
            return property?.ValueC.ToVector3() ?? defaultValue;
        }

        public static Vector3 GetPropertyValueCByName(this Kn5Material mat, string name) {
            return GetPropertyValueCByName(mat, name, Vector3.Zero);
        }

        public static bool SetResource(this EffectResourceVariable variable, IRenderableTexture texture) {
            var res = texture?.Resource;
            variable.SetResource(res);
            return res != null;
        }

        public static bool SetResource(this EffectOnlyResourceVariable variable, IRenderableTexture texture) {
            var res = texture?.Resource;
            variable.SetResource(res);
            return res != null;
        }
    }
}