﻿using System;
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
        private readonly Axes _axes;
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
            _telescopeMediator = telescopeMediator;
            _axes = new Axes(_telescopeMediator);
            Settings.Default.PropertyChanged += OnSettingChanged;

            // load the 3D model and reset camera on startup
            Application.Current.Dispatcher.Invoke(LoadModel);
            ResetCamera();

            try
            {
                _compassN = MaterialHelper.CreateImageMaterial(Model3D.GetCompassFile(false), 100);
                _compassS = MaterialHelper.CreateImageMaterial(Model3D.GetCompassFile(true), 100);
            }
            catch (FileNotFoundException e)
            {
                Notification.ShowError($"Scope 3D View could not find compass material files: {e.Message}");
                Logger.Error($"Could not find compass material files: {e.Message}");
            }

            var telescopePollingCommand = new AsyncCommand<bool>(async () =>
            {
                await TelescopePollingTask(TimeSpan.FromMilliseconds(Settings.Default.PollingInterval));
                return true;
            });
            telescopePollingCommand.ExecuteAsync(null);
        }

        #region Properties
        
        public bool IsTool => false;
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

        private bool SouthernHemisphere => _telescopeMediator.GetInfo().SiteLatitude < 0;
        public PierSide PierSide => _telescopeMediator.GetInfo().SideOfPier;

        public double SideRealtime => _telescopeMediator.GetInfo().SiderealTime;
        
        #endregion

        private void OnSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            Logger.Debug($"Scope 3D View setting changed: {e.PropertyName}");

            switch (e.PropertyName)
            {
                case "RaOffset":
                case "DecOffset":
                case "OtaAccentColor":
                case "ModelType":
                    Application.Current.Dispatcher.Invoke(LoadModel);
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
        
        private async Task TelescopePollingTask(TimeSpan pollInterval)
        {
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
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
                        var raDec = Poll();
                        Rotate(raDec[0], raDec[1]);
                    }
                    else
                    {
                        // no telescope connected
                        Compass = null;
                    }
                    
                    await Task.Delay(pollInterval, _cancellationTokenSource.Token);
                }
            }
        }

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
            
            LookDirection = new Vector3D(lookDirectionX, lookDirectionY, lookDirectionZ);
            UpDirection = new Vector3D(upDirectionX, upDirectionY, upDirectionZ);
            Position = new Point3D(cameraPositionX, cameraPositionY, cameraPositionZ);
        }

        private void LoadModel()
        {
            var import = new ModelImporter();

            Logger.Info($"Attempting to load telescope model {Settings.Default.ModelType}");
            var result = Enum.TryParse(Settings.Default.ModelType, out Model3DType modelType);
            Model3DGroup model = null;
            
            try
            {
                model = import.Load(result ? Model3D.GetModelFile(modelType) : Model3D.GetModelFile(Model3DType.Default));
            }
            catch (FileNotFoundException e)
            {
                Notification.ShowError($"Scope 3D View failed to load 3D model files: {e.Message}");
                Logger.Error($"Failed to load telescope 3D model files: {e.Message}");
            }

            Model = model;

            const string fallbackAccentColor = "FFF44336";
            var converter = new BrushConverter();
            var accentbrush = (Brush)converter.ConvertFromString($"#{fallbackAccentColor}");

            try
            {
                accentbrush = (Brush)converter.ConvertFromString($"#{Settings.Default.OtaAccentColor}");
            } catch (FormatException)
            {
                Notification.ShowWarning($"Scope 3D View invalid telescope hex color string" +
                                         $" #{Settings.Default.OtaAccentColor}, using fallback color #{fallbackAccentColor}");
                
                Logger.Warning($"Invalid telescope hex color string #{Settings.Default.OtaAccentColor}, using fallback color #{fallbackAccentColor}");
            }

            var materialota = MaterialHelper.CreateMaterial(accentbrush);
            if (model?.Children[0] is GeometryModel3D ota) ota.Material = materialota;

            //color weights
            var materialweights = MaterialHelper.CreateMaterial(new SolidColorBrush(Color.FromRgb(64, 64, 64)));
            if (model?.Children[1] is GeometryModel3D weights) weights.Material = materialweights;
            //color bar
            var materialbar = MaterialHelper.CreateMaterial(Brushes.Gainsboro);
            if (model?.Children[2] is GeometryModel3D bar) bar.Material = materialbar;
            Logger.Info($"Telescope model {Settings.Default.ModelType} loaded");
        }

        private double[] Poll()
        {
            var pos = _telescopeMediator.GetCurrentPosition();
            var raDec = _axes.RaDecToAxesXY(AlignMode.algGermanPolar, new[]{pos.RA, pos.Dec});
            return raDec;
        }
        
        private void Rotate(double axis0, double axis1)
        {
            var axes = Model3D.RotateModel( axis0, axis1, SouthernHemisphere);
            YAxis = axes[0];
            XAxis = axes[1];
            //TODO only set when telescope connected
            ZAxis = Math.Round(Math.Abs(_telescopeMediator.GetInfo().SiteLatitude), 2);
        }
    }
}