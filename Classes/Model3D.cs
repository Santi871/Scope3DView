using System;
using System.Reflection;

namespace Scope3DView.Classes
{
    public enum Model3DType
    {
        Default = 0,
        Reflector = 1,
        Refractor = 2,
        SchmidtCassegrain = 3,
        RitcheyChretien = 4,
        RitcheyChretienTruss = 5
    }
    
    public static class Model3D
    {
        private static readonly string _directoryPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Models\\";
        public static string GetModelFile(Model3DType modelType)
        {
            string gpModel;
            switch (modelType)
            {
                case Model3DType.Default:
                    gpModel = @"Default.obj";
                    break;
                case Model3DType.Reflector:
                    gpModel = @"Reflector.obj";
                    break;
                case Model3DType.Refractor:
                    gpModel = @"Refractor.obj";
                    break;
                case Model3DType.SchmidtCassegrain:
                    gpModel = @"SchmidtCassegrain.obj";
                    break;
                case Model3DType.RitcheyChretien:
                    gpModel = @"RitcheyChretien.obj";
                    break;
                case Model3DType.RitcheyChretienTruss:
                    gpModel = @"RitcheyChretienTruss.obj";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modelType), modelType, null);
            }
            var filePath = System.IO.Path.Combine(_directoryPath ?? throw new InvalidOperationException(), gpModel);
            var file = new Uri(filePath).LocalPath;
            return file;
        }
        public static string GetCompassFile(bool southernHemisphere)
        {
            const string compassN = @"CompassN.png";
            const string compassS = @"CompassS.png";
            var compassFile = southernHemisphere ? compassS : compassN;
            var filePath = System.IO.Path.Combine(_directoryPath ?? throw new InvalidOperationException(), compassFile);
            var file = new Uri(filePath).LocalPath;
            return file;
        }
        public static double[] RotateModel(double ax, double ay, bool southernHemisphere)
        {
            var axes = new[] { 0.0, 0.0 };
            if (southernHemisphere)
            {
                axes[0] = Math.Round(180 - ax, 3);
                axes[1] = Math.Round(ay - 180, 3);
            }
            else
            {
                axes[0] = Math.Round(ax, 3);
                axes[1] = Math.Round(ay * -1.0, 3);
            }
            return axes;
        }
    }
}