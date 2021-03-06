/*
    Copyright(C) 2021  Rob Morgan (robert.morgan.e@gmail.com)
    Copyright(C) 2021  Santiago Vegega
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;
using System.Windows.Media.Imaging;

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
        
        public static BitmapImage LoadImageToMemory(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            System.IO.Stream stream = System.IO.File.Open(path, System.IO.FileMode.Open);
            image.StreamSource = new System.IO.MemoryStream();
            stream.CopyTo(image.StreamSource);
            image.EndInit();

            stream.Close();
            stream.Dispose();
            image.StreamSource.Close();
            image.StreamSource.Dispose();

            return image;
        }
        
        public static double[] RotateModel(double ax, double ay, bool southernHemisphere)
        {
            var axes = new[] { 0.0, 0.0, 0.0 };
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