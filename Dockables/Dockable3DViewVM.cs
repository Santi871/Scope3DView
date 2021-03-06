/*
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using Scope3DView.Classes;
using Scope3DView.Properties;
using Model3D = Scope3DView.Classes.Model3D;

namespace Scope3DView.Dockables
{
    [Export(typeof(IDockableVM))]
    public class Dockable3DViewVM : DockableVM
    {
        private readonly ITelescopeMediator _telescopeMediator;
        private readonly ITelescopeModel _telescopeModel;
        private readonly Material _compassN, _compassS;
        private CancellationTokenSource _cancellationTokenSource;
        private bool? _previouslySouthern;

        [ImportingConstructor]
        public Dockable3DViewVM(
            IProfileService profileService,
            ITelescopeMediator telescopeMediator
            ) : base(profileService)
        {
            Title = "3D View";
            
            // load plugin icon
            var dict = new ResourceDictionary
            {
                Source = new Uri("Scope3DView;component/Options.xaml", UriKind.RelativeOrAbsolute)
            };
            ImageGeometry = (GeometryGroup)dict["Scope3DView_SVG"];
            ImageGeometry.Freeze();

            _telescopeMediator = telescopeMediator;
            _telescopeModel = new TelescopeModel(_telescopeMediator);
            Settings.Default.TeardownRequested = false;
            Settings.Default.PropertyChanged += OnSettingChanged;
            profileService.ActiveProfile.ColorSchemaSettings.PropertyChanged += ColorSchemaSettingsOnPropertyChanged;
            
            // load the 3D model and reset camera on startup
            LoadModel();
            ResetCamera();

            _compassN = LoadCompassFile(false);
            _compassS = LoadCompassFile(true);
            
            var telescopePollingCommand = new AsyncCommand<bool>(async () =>
            {
                await TelescopePollingTask(TimeSpan.FromMilliseconds(Settings.Default.RefreshInterval));
                return true;
            });
            telescopePollingCommand.ExecuteAsync(null);
        }

        #region Properties
        
        public override bool IsTool => false;
        private bool SouthernHemisphere => _telescopeMediator.GetInfo().SiteLatitude < 0;
        public double XOffset => Settings.Default.DecOffset;
        public double YOffset => Settings.Default.RaOffset;
        public double ZOffset => 0;

        private double _xAxis;
        public double XAxis
        {
            get => _xAxis;
            set
            {
                _xAxis = value;
                XAxisOffset = value + XOffset;
                RaisePropertyChanged();
            }
        }

        private double _yAxis;
        public double YAxis
        {
            get => _yAxis;
            set
            {
                _yAxis = value;
                YAxisOffset = value + YOffset;
                RaisePropertyChanged();
            }
        }

        private double _zAxis;
        public double ZAxis
        {
            get => _zAxis;
            set
            {
                _zAxis = value;
                ZAxisOffset = ZOffset - value;
                RaisePropertyChanged();
            }
        }

        private double _xAxisOffset;
        public double XAxisOffset
        {
            get => _xAxisOffset;
            set
            {
                _xAxisOffset = value;
                RaisePropertyChanged();
            }
        }

        private double _yAxisOffset;
        public double YAxisOffset
        {
            get => _yAxisOffset;
            set
            {
                _yAxisOffset = value;
                RaisePropertyChanged();
            }
        }

        private double _zAxisOffset;
        public double ZAxisOffset
        {
            get => _zAxisOffset;
            set
            {
                _zAxisOffset = value;
                RaisePropertyChanged();
            }
        }


        private System.Windows.Media.Media3D.Model3D _model;
        public System.Windows.Media.Media3D.Model3D Model
        {
            get => _model;
            set
            {
                if (_model == value) return;
                _model = value;
                RaisePropertyChanged();
            }
        }
        
        private Point3D _position;
        public Point3D Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                _position = value;
                RaisePropertyChanged();
            }
        }

        private Vector3D _lookDirection;
        public Vector3D LookDirection
        {
            get => _lookDirection;
            set
            {
                if (_lookDirection == value) return;
                _lookDirection = value;
                RaisePropertyChanged();
            }
        }
        
        private Vector3D _upDirection;
        public Vector3D UpDirection
        {
            get => _upDirection;
            set
            {
                if (_upDirection == value) return;
                _upDirection = value;
                RaisePropertyChanged();
            }
        }

        public int CameraFov => Settings.Default.CameraFov;
        
        private Material _compass;
        public Material Compass
        {
            get => _compass;
            set
            {
                _compass = value;
                RaisePropertyChanged();
            }
        }
        
        private double _axis0;
        public double Axis0
        {
            get => _axis0;
            set
            {
                _axis0 = value;
                RaisePropertyChanged();
            }
        }
        
        private double _axis1;
        public double Axis1
        {
            get => _axis1;
            set
            {
                _axis1 = value;
                RaisePropertyChanged();
            }
        }

        #endregion
        
        /// <summary>
        /// Reads a compass image file and loads it into memory
        /// </summary>
        /// <param name="southernHemisphere">Whether to load the southern or northern hemisphere file</param>
        /// <returns>Material for the compass texture</returns>
        private static Material LoadCompassFile(bool southernHemisphere)
        {
            Material material = null;

            try
            {
                var compassImagePath = Model3D.GetCompassFile(southernHemisphere);
                var compassImage = Model3D.LoadImageToMemory(compassImagePath);
                material = MaterialHelper.CreateImageMaterial(compassImage, 100);
            }
            catch (FileNotFoundException e)
            {
                Notification.ShowError($"Scope 3D View could not find compass material files: {e.Message}");
                Logger.Error($"Could not find compass material files: {e.Message}");
            }

            return material;
        }
        
        private void ColorSchemaSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // reload model when profile colors change
            LoadModel();
        }
        
        private void OnSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            Logger.Debug($"Scope 3D View setting changed: {e.PropertyName}");

            switch (e.PropertyName)
            {
                case "TeardownRequested":
                    if (Settings.Default.TeardownRequested)
                    {
                        Teardown();
                    }
                    break;
                
                case "RaOffset":
                case "DecOffset":
                case "OtaAccentColor":
                case "AutoOtaColorChange":
                case "ModelType":
                    LoadModel();
                    break;
                
                case "LookDirectionX":
                case "LookDirectionY":
                case "LookDirectionZ":
                case "UpDirectionX":
                case "UpDirectionY":
                case "UpDirectionZ":
                case "CameraPositionX":
                case "CameraPositionY":
                case "CameraPositionZ":
                case "CameraFov":
                    ResetCamera();
                    break;
            }
        }
        
        /// <summary>
        /// Monitors connected telescope every interval to update the 3D model
        /// </summary>
        /// <param name="pollInterval">Telescope polling interval</param>
        private async Task TelescopePollingTask(TimeSpan pollInterval)
        {
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                var token = _cancellationTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    var pos = _telescopeMediator.GetCurrentPosition();
                    if (pos != null)
                    {
                        // switch the compass texture if telescope hemisphere changes
                        var southernHemisphere = SouthernHemisphere;
                        if (_previouslySouthern == null || southernHemisphere != _previouslySouthern)
                        {
                            Compass = southernHemisphere ? _compassS : _compassN;
                            _previouslySouthern = southernHemisphere;
                        }
                        
                        // poll scope position and rotate the 3D model
                        var newPosition = _telescopeModel.GetModelRotation();
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                            new Action(() => UpdateModelPosition(
                                newPosition[1],
                                newPosition[0],
                                newPosition[2])
                            ));
                    }
                    else
                    {
                        // no telescope connected
                        Compass = null;
                    }
                    
                    // NINA doesn't appear to wait for a cancellable Task.Delay to exit before exiting
                    // so using the non-cancellable overload as a workaround for now
                    // ReSharper disable once MethodSupportsCancellation
                    await Task.Delay(pollInterval).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Resets the viewport camera to the defined starting position
        /// </summary>
        private void ResetCamera()
        {
            var lookDirectionX = Settings.Default.LookDirectionX;
            var lookDirectionY = Settings.Default.LookDirectionY;
            var lookDirectionZ = Settings.Default.LookDirectionZ;

            var upDirectionX = Settings.Default.UpDirectionX;
            var upDirectionY = Settings.Default.UpDirectionY;
            var upDirectionZ = Settings.Default.UpDirectionZ;

            var cameraPositionX = Settings.Default.CameraPositionX;
            var cameraPositionY = Settings.Default.CameraPositionY;
            var cameraPositionZ = Settings.Default.CameraPositionZ;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                LookDirection = new Vector3D(lookDirectionX, lookDirectionY, lookDirectionZ);
                UpDirection = new Vector3D(upDirectionX, upDirectionY, upDirectionZ);
                Position = new Point3D(cameraPositionX, cameraPositionY, cameraPositionZ);
            }));
        }

        /// <summary>
        /// Obtains a brush to color the model's OTA based on user and profile color settings
        /// </summary>
        /// <returns></returns>
        private Brush GetOtaBrush()
        {
            var converter = new BrushConverter();
            var color = profileService.ActiveProfile.ColorSchemaSettings.ColorSchema.ButtonBackgroundColor;
            Brush accentbrush = new SolidColorBrush(color);
            if (Settings.Default.AutoOtaColorChange) return accentbrush;
            
            try
            {
                accentbrush = (Brush)converter.ConvertFromString($"#{Settings.Default.OtaAccentColor}");
            } catch (FormatException)
            {
                Notification.ShowWarning($"Scope 3D View invalid telescope color string" +
                                         $" #{Settings.Default.OtaAccentColor}, using theme-based color instead");

                Logger.Warning(
                    $"Invalid telescope color string #{Settings.Default.OtaAccentColor}," +
                    $" using theme-based color instead");
            }
            return accentbrush;
        }

        private void UpdateModelPosition(double x, double y, double z)
        {
            XAxis = x;
            YAxis = y;
            ZAxis = z;
        }

        /// <summary>
        /// Reads a 3D model from disk and loads it
        /// </summary>
        private void LoadModel()
        {
            Logger.Info($"Attempting to load telescope model {Settings.Default.ModelType}");
            var result = Enum.TryParse(Settings.Default.ModelType, out Model3DType modelType);
            
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var accentbrush = GetOtaBrush();
            
                Model3DGroup model = null;
                try
                {
                    model = _telescopeModel.LoadModel(result ? modelType : Model3DType.Default, accentbrush);
                }
                catch (FileNotFoundException e)
                {
                    Notification.ShowError($"Scope 3D View failed to load 3D model files: {e.Message}");
                    Logger.Error($"Failed to load telescope 3D model files: {e.Message}");
                }
                Model = model;
            }));
        }

        public void Teardown()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}