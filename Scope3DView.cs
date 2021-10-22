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

using NINA.Plugin;
using NINA.Plugin.Interfaces;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using NINA.Core.Utility;
using Scope3DView.Properties;

namespace Scope3DView {
    [Export(typeof(IPluginManifest))]
    public class Scope3DView : PluginBase, INotifyPropertyChanged
    {
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

        #region Properties

        private ICommand _resetSettingsCommand;
        public ICommand ResetSettingsCommand
        {
            get
            {
                _resetSettingsCommand = new RelayCommand(param => ResetSettings());
                return _resetSettingsCommand;
            }
        }

        public bool TeardownRequested
        {
            get => Settings.Default.TeardownRequested;
            set
            {
                Settings.Default.TeardownRequested = value;
                Settings.Default.Save();
                NotifyPropertyChanged();
            }
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
        
        #endregion

        private void ResetSettings()
        {
            PollingInterval = 100;
            RaOffset = -90;
            DecOffset = 90;
            LookDirectionX = -2616;
            LookDirectionY = -3167;
            LookDirectionZ = -1170;
            CameraPositionX = 2523;
            CameraPositionY = 3000;
            CameraPositionZ = 1379;
            UpDirectionX = 0.35;
            UpDirectionY = 0.43;
            UpDirectionZ = 0.82;
        }
        
        public override async Task Teardown()
        {
            TeardownRequested = true;
            // wait for ongoing polling to be done before exiting
            // (NINA doesn't appear to support cancellable Task.Delay for the moment)
            await Task.Delay(Settings.Default.PollingInterval);
        }
    }
}
