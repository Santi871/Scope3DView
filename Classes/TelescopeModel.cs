using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using NINA.Equipment.Interfaces.Mediator;

namespace Scope3DView.Classes
{
    public interface ITelescopeModel
    {
        Model3DGroup LoadModel(Model3DType modeltype, Brush otaBrush);
        double[] GetModelRotation();
    }
    
    public class TelescopeModel : ITelescopeModel
    {
        private readonly ITelescopeMediator _telescopeMediator;
        private readonly Axes _axes;

        public TelescopeModel(ITelescopeMediator telescopeMediator)
        {
            _telescopeMediator = telescopeMediator;
            _axes = new Axes(_telescopeMediator);
        }

        /// <summary>
        /// Creates the 3D model and applies a color to the telescope
        /// </summary>
        /// <param name="modeltype">The 3D model type of telescope to load</param>
        /// <param name="otaBrush">Brush to color the telescope's model</param>
        /// <returns>Telescope 3D model</returns>
        public Model3DGroup LoadModel(Model3DType modeltype, Brush otaBrush)
        {
            var import = new ModelImporter();
            var model = import.Load(Model3D.GetModelFile(modeltype));

            var materialota = MaterialHelper.CreateMaterial(otaBrush);
            if (model?.Children[0] is GeometryModel3D ota) ota.Material = materialota;

            //color weights
            var materialweights = MaterialHelper.CreateMaterial(new SolidColorBrush(Color.FromRgb(64, 64, 64)));
            if (model?.Children[1] is GeometryModel3D weights) weights.Material = materialweights;
            //color bar
            var materialbar = MaterialHelper.CreateMaterial(Brushes.Gainsboro);
            if (model?.Children[2] is GeometryModel3D bar) bar.Material = materialbar;

            return model;
        }

        /// <summary>
        /// Gets 3D model's rotation coordinates based on current telescope pointing coordinates
        /// </summary>
        /// <returns>Array containing the X, Y and Z rotation coordinates</returns>
        public double[] GetModelRotation()
        {
            var position = _telescopeMediator.GetCurrentPosition();
            var latitude = _telescopeMediator.GetInfo().SiteLatitude;
            var southernHemisphere = latitude < 0;
            
            var raDec = _axes.RaDecToAxesXY(AlignMode.algGermanPolar, new[]{position.RA, position.Dec});
            var axes = Model3D.RotateModel( raDec[0], raDec[1], southernHemisphere);
            axes[2] = Math.Round(Math.Abs(latitude), 2);
            return axes;
        }
    }
}