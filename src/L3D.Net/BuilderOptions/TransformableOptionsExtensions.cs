using System.Numerics;

namespace L3D.Net.BuilderOptions
{
    public static class TransformableOptionsExtensions
    {
        public static TOptions WithPosition<TOptions>(this TOptions options, double x, double y, double z) where TOptions : TransformableOptions
        {
            return options.WithPosition((float) x, (float) y, (float) z);
        }

        public static TOptions WithPosition<TOptions>(this TOptions options, float x, float y, float z) where TOptions : TransformableOptions
        {
            options.Data.Position = new Vector3
            {
                X = x,
                Y = y,
                Z = z
            };
            return options;
        }

        public static TOptions WithPosition<TOptions>(this TOptions options, Vector3 position) where TOptions : TransformableOptions
        {
            return options.WithPosition(position.X, position.Y, position.Z);
        }

        public static TOptions WithRotation<TOptions>(this TOptions options, float x, float y, float z) where TOptions : TransformableOptions
        {
            options.Data.Rotation = new Vector3
            {
                X = x,
                Y = y,
                Z = z
            };
            return options;
        }

        public static TOptions WithRotation<TOptions>(this TOptions options, Vector3 rotation) where TOptions : TransformableOptions
        {
            return options.WithRotation(rotation.X, rotation.Y, rotation.Z);
        }

        public static TOptions WithRotation<TOptions>(this TOptions options, double x, double y, double z) where TOptions : TransformableOptions
        {
            return options.WithRotation((float) x, (float) y, (float) z);
        }
    }
}