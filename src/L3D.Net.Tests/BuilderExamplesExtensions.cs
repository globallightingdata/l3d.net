using System.IO;
using L3D.Net.API.Dto;
using L3D.Net.BuilderOptions;

namespace L3D.Net.Tests
{
    static class BuilderExamplesExtensions
    {
        public static LuminaireBuilder BuildExample000(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_000");
            var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");

            builder
                .WithTool("Example-Tool")
                .AddGeometry("luminaire", cubeObjPath, GeometricUnits.m, geomOptions => geomOptions
                .AddRectangularLightEmittingObject("leo", 0.5, 0.25, leoOptions => leoOptions
                    .WithLightEmittingSurfaceOnParent(3)
                )
            );

            return builder;
        }

        public static LuminaireBuilder BuildExample001(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_001");
            var cubeObjPath = Path.Combine(exampleDirectory, "cube", "cube.obj");

            builder
                .WithTool("Example-Tool")
                .AddGeometry("luminaire", cubeObjPath, GeometricUnits.m, geomOptions => geomOptions
                .WithPosition(-0.25, -0.125, 0.05)
                .AddRectangularLightEmittingObject("leo", 0.5, 0.25, leoOptions => leoOptions
                    .WithPosition(0.25, 0.125, -0.05)
                    .WithLightEmittingSurfaceOnParent(3)
                )
            );

            return builder;
        }

        public static LuminaireBuilder BuildExample002(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_002");
            var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
            var baseArmConObjPath = Path.Combine(exampleDirectory, "base-arm-con", "base-arm-con.obj");
            var armObjPath = Path.Combine(exampleDirectory, "arm", "arm.obj");
            var armHeadConObjPath = Path.Combine(exampleDirectory, "arm-head-con", "arm-head-con.obj");
            var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");

            builder
                .WithTool("Keyboard-v1.0")
                .WithModelName("First example")
                .WithDescription("First ever xml luminaire geometry description")
                .AddGeometry("base", baseObjPath, GeometricUnits.mm, geomOptions => geomOptions
                    .AddJoint("base-con-hinge", baseConHindeOptions => baseConHindeOptions
                        .WithPosition(0, 0.05, 0.05)
                        .WithZAxisDegreesOfFreedom(-15, 15, 1)
                        .AddGeometry("base-arm-con", baseArmConObjPath, GeometricUnits.mm, baseArmConOptions =>
                            baseArmConOptions
                                .WithPosition(0, -0.05, -0.05)
                                .AddJoint("con-arm-hinge", conArmHingeOptions => conArmHingeOptions
                                    .WithPosition(0, 0.05, 0.085)
                                    .WithXAxisDegreesOfFreedom(0, 60, 1)
                                    .WithDefaultRotation(30, 0, 0)
                                    .AddGeometry("arm", armObjPath, GeometricUnits.mm, armOptions => armOptions
                                        .WithPosition(0, -0.05, -0.085)
                                        .AddJoint("arm-con-hinge", armConHingeOptions => armConHingeOptions
                                            .WithPosition(0, 0.05, 0.485)
                                            .WithRotation(90, 0, 0)
                                            .WithXAxisDegreesOfFreedom(-60, 0, 1)
                                            .WithDefaultRotation(-30, 0, 0)
                                            .AddGeometry("arm-head-con", armHeadConObjPath, GeometricUnits.mm,
                                                armHeadConOptions => armHeadConOptions
                                                    .WithPosition(0, -0.05, -0.485)
                                                    .AddJoint("con-head-hinge", conHeadHingeOptions =>
                                                        conHeadHingeOptions
                                                            .WithPosition(0, 0.05, 0.525)
                                                            .WithZAxisDegreesOfFreedom(-15, 15, 1)
                                                            .AddGeometry("head", headObjPath, GeometricUnits.mm,
                                                                headOptions => headOptions
                                                                    .WithPosition(0, -0.05, -0.525)
                                                                    .WithExcludedFromMeasurement()
                                                                    .AddRectangularLightEmittingObject("leo", 0.18, 0.08,
                                                                        leoOptions => leoOptions
                                                                            .WithPosition(0, 0.04631, 0.6771)
                                                                            .WithRotation(90, 0, 180)
                                                                            .WithLightEmittingSurfaceOnParent(16)
                                                                            .WithLightEmittingSurfaceOnParent(17)
                                                                            .WithLightEmittingSurfaceOnParent(18)
                                                                            .WithLightEmittingSurfacesOnParent(19, 21)
                                                                    )
                                                            )
                                                    )
                                            )
                                        )
                                    )
                                )
                        )
                    )
                );

            return builder;
        }

        public static LuminaireBuilder BuildExample003(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_003");
            var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");

            builder
                .WithTool("Keyboard-v1.0")
                .AddGeometry("luminaire", objPath, GeometricUnits.mm, geomOptions => geomOptions
                .AddCircularLightEmittingObject("leo", 0.1, leoOptions => leoOptions
                    .WithLightEmittingSurfacesOnParent(1199, 1235)));

            return builder;
        }

        public static LuminaireBuilder BuildExample004(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_004");
            var objPath = Path.Combine(exampleDirectory, "luminaire", "luminaire.obj");

            builder
                .WithTool("Keyboard-v1.0")
                .AddGeometry("luminaire", objPath, GeometricUnits.mm, geomOptions => geomOptions
                .WithPosition(0, 0, -0.5)
                .AddRectangularLightEmittingObject("leo_top", 0.15, 1.0, leoOptions => leoOptions
                    .WithPosition(0, 0, 0.0375)
                    .WithRotation(180, 0, 90)
                    .WithLightEmittingSurfaceOnParent(84)
                    .WithLightEmittingSurfaceOnParent(85)
                )
                .AddRectangularLightEmittingObject("leo_bottom", 0.17, 1.175, leoOptions => leoOptions
                    .WithPosition(0, 0, 0.0025)
                    .WithRotation(0, 0, -90)
                    .WithLightEmittingSurfaceOnParent(90)
                    .WithLightEmittingSurfaceOnParent(91)
                )
                .WithElectricalConnector(-0.575, 0, 0.04)
                .WithPendulumConnector(-0.55, 0, 0.04)
                .WithPendulumConnector(0.55, 0, 0.04)
            );

            return builder;
        }

        public static LuminaireBuilder BuildExample005(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_005");
            var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
            var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");

            builder
                .WithTool("Keyboard-v1.0")
                .WithModelName("Another example")
                .WithDescription("Example luminaire 4")
                .AddGeometry("base", baseObjPath, GeometricUnits.mm, geomOptions => geomOptions
                    .AddJoint("head-hinge", jointOptions => jointOptions
                        .WithPosition(0, 0, 0.5)
                        .WithRotation(45, 0, 0)
                        .WithZAxisDegreesOfFreedom(-30, 30, 1)
                        .AddGeometry("head", headObjPath, GeometricUnits.mm, headOptions => headOptions
                            .WithExcludedFromMeasurement()
                            .WithPosition(0, -0.3535, -0.3535)
                            .WithRotation(-45, 0, 0)
                            .AddRectangularLightEmittingObject("leo", 0.15, 0.035, leoOptions => leoOptions
                                .WithPosition(0, -0.064, 0.475)
                                .WithRotation(-45, 0, 0)
                                .WithLightEmittingSurfacesOnParent(158, 179))))
                );

            return builder;
        }

        public static LuminaireBuilder BuildExample006(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_006");
            var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
            var baseHeadConeObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
            var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");


            void AddHead(GeometryOptions options, int index, double yPosition)
            {
                options.AddJoint($"part-joint-{index}", joint0Options => joint0Options
                    .WithPosition(0, yPosition, -0.017)
                    .WithZAxisDegreesOfFreedom(-180, 180, 1)
                    .AddGeometry($"part-{index}", baseHeadConeObjPath, GeometricUnits.m, conOptions => conOptions
                        .AddJoint($"head-joint-{index}", joint1Options => joint1Options
                            .WithPosition(0, 0, -0.128)
                            .WithYAxisDegreesOfFreedom(-30, 65, 1)
                            .AddGeometry($"head-{index}", headObjPath, GeometricUnits.m, headOptions => headOptions
                                .AddCircularLightEmittingObject($"leo-{index}", 0.1, leoOptions => leoOptions
                                    .WithPosition(0.045, -0.043, 0)
                                    .WithRotation(0, -90, 0)
                                    .WithLightEmittingSurfacesOnParent(50, 97)))

                        )));
            }

            builder
                .WithTool("Experimental")
                .AddGeometry("base", baseObjPath, GeometricUnits.m, baseOptions =>
                {
                    AddHead(baseOptions, 0, -0.55);
                    AddHead(baseOptions, 1, -0.15);
                    AddHead(baseOptions, 2, 0.25);
                    AddHead(baseOptions, 3, 0.65);
                    return baseOptions;
                });

            return builder;
        }

        public static LuminaireBuilder BuildExample007(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_007");
            var baseObjPath = Path.Combine(exampleDirectory, "base", "base.obj");
            var baseHeadConeObjPath = Path.Combine(exampleDirectory, "base-head-con", "base-head-con.obj");
            var headObjPath = Path.Combine(exampleDirectory, "head", "head.obj");

            builder
                .WithTool("Experimental")
                .AddGeometry("base", baseObjPath, GeometricUnits.mm, baseOptions => baseOptions
                .AddJoint("base-head-con-joint-0", joint0Options => joint0Options
                    .WithPosition(-0.4, 0, 0.0375)
                    .WithZAxisDegreesOfFreedom(-180, 180, 1)
                    .WithDefaultRotation(0, 0, 0)
                    .AddGeometry("base-head-con-0", baseHeadConeObjPath, GeometricUnits.mm, conOptions =>
                        conOptions
                            .WithPosition(0.4, 0, -0.0375)
                            .AddJoint("head-joint-0", joint1Options => joint1Options
                                .WithPosition(-0.4, 0, 0.0375)
                                .WithXAxisDegreesOfFreedom(-45, 45, 1)
                                .WithDefaultRotation(0, 0, 0)
                                .AddGeometry("head-0", headObjPath, GeometricUnits.mm, headOptions => headOptions
                                    .WithPosition(0.4, 0, -0.0375)
                                    .AddCircularLightEmittingObject("LEO0", 0.0575, leoOptions => leoOptions
                                        .WithPosition(-0.4, 0, 0.01)
                                        .WithLuminousHeights(10, 10, 10, 10)
                                        .WithLightEmittingSurfacesOnParent(574, 607))))))
                .AddJoint("base-head-con-joint-1", joint0Options => joint0Options
                    .WithPosition(0.4, 0, 0.0375)
                    .WithZAxisDegreesOfFreedom(-180, 180, 1)
                    .WithDefaultRotation(0, 0, 0)
                    .AddGeometry("base-head-con-1", baseHeadConeObjPath, GeometricUnits.mm, conOptions =>
                        conOptions
                            .WithPosition(0.4, 0, -0.0375)
                            .AddJoint("head-joint-1", joint1Options => joint1Options
                                .WithPosition(-0.4, 0, 0.0375)
                                .WithXAxisDegreesOfFreedom(-45, 45, 1)
                                .WithDefaultRotation(0, 0, 0)
                                .AddGeometry("head-1", headObjPath, GeometricUnits.mm, headOptions => headOptions
                                    .WithPosition(0.4, 0, -0.0375)
                                    .AddCircularLightEmittingObject("LEO1", 0.0575, leoOptions => leoOptions
                                        .WithPosition(-0.4, 0, 0.01)
                                        .WithLuminousHeights(10, 10, 10, 10)
                                        .WithLightEmittingSurfacesOnParent(574, 607))))))
                .AddSensorObject("Sensor", sensorOptions => sensorOptions)
            );

            return builder;
        }

        public static LuminaireBuilder BuildExample008(this LuminaireBuilder builder)
        {
            var exampleDirectory = Path.Combine(Setup.ExamplesDirectory, "example_008");
            var cubeObjPath = Path.Combine(exampleDirectory, "cube", "textured_cube.obj");

            builder
                .WithTool("Example-Tool")
                .AddGeometry("luminaire", cubeObjPath, GeometricUnits.m, geomOptions => geomOptions
                    .AddRectangularLightEmittingObject("leo", 0.5, 0.25, leoOptions => leoOptions
                        .WithLightEmittingSurfaceOnParent(3)
                    )
                );

            return builder;
        }

    }
}
