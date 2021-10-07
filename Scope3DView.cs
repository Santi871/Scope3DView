using NINA.Plugin;
using NINA.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Scope3DView.Classes;
using Scope3DView.Properties;

namespace Scope3DView {
    /// <summary>
    /// This class exports the IPluginManifest interface and will be used for the general plugin information and options
    /// The base class "PluginBase" will populate all the necessary Manifest Meta Data out of the AssemblyInfo attributes. Please fill these accoringly
    /// 
    /// An instance of this class will be created and set as datacontext on the plugin options tab in N.I.N.A. to be able to configure global plugin settings
    /// The user interface for the settings will be defined by a DataTemplate with the key having the naming convention "<MyPlugin.Name>_Options" where MyPlugin.Name corresponds to the AssemblyTitle - In this template example it is found in the Options.xaml
    /// </summary>
    [Export(typeof(IPluginManifest))]
    public class Scope3DView : PluginBase, INotifyPropertyChanged {

        [ImportingConstructor]
        public Scope3DView() {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Settings.Default.Save();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool AutoOtaColorChange
        {
            get => Settings.Default.AutoOtaColorChange;
            set
            {
                Settings.Default.AutoOtaColorChange = value;
                Settings.Default.Save();
                AutoOtaColorChangeEnabled = !value;
                NotifyPropertyChanged();
            }
        }
        
        public bool AutoOtaColorChangeEnabled
        {
            get => !Settings.Default.AutoOtaColorChange;
            set => NotifyPropertyChanged();
        }

        public string OtaAccentColor {
            get => Settings.Default.OtaAccentColor;
            set {
                Settings.Default.OtaAccentColor = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public string ModelType
        {
            get => Settings.Default.ModelType;
            set
            {
                Settings.Default.ModelType = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public int PollingInterval
        {
            get => Settings.Default.PollingInterval;
            set
            {
                Settings.Default.PollingInterval = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public int CameraFov
        {
            get => Settings.Default.CameraFov;
            set
            {
                Settings.Default.CameraFov = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public double LookDirectionX
        {
            get => Settings.Default.LookDirectionX;
            set
            {
                Settings.Default.LookDirectionX = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
        public double LookDirectionY
        {
            get => Settings.Default.LookDirectionY;
            set
            {
                Settings.Default.LookDirectionY = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
        public double LookDirectionZ
        {
            get => Settings.Default.LookDirectionZ;
            set
            {
                Settings.Default.LookDirectionZ = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public double CameraPositionX
        {
            get => Settings.Default.CameraPositionX;
            set
            {
                Settings.Default.CameraPositionX = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
        
        public double CameraPositionY
        {
            get => Settings.Default.CameraPositionY;
            set
            {
                Settings.Default.CameraPositionY = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
        
        public double CameraPositionZ
        {
            get => Settings.Default.CameraPositionZ;
            set
            {
                Settings.Default.CameraPositionZ = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public double UpDirectionX
        {
            get => Settings.Default.UpDirectionX;
            set
            {
                Settings.Default.UpDirectionX = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
        
        public double UpDirectionY
        {
            get => Settings.Default.UpDirectionY;
            set
            {
                Settings.Default.UpDirectionY = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
        
        public double UpDirectionZ
        {
            get => Settings.Default.UpDirectionZ;
            set
            {
                Settings.Default.UpDirectionZ = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public double RaOffset
        {
            get => Settings.Default.RaOffset;
            set
            {
                Settings.Default.RaOffset = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public double DecOffset
        {
            get => Settings.Default.DecOffset;
            set
            {
                Settings.Default.DecOffset = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }
    }
}
