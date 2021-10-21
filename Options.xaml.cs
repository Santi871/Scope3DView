using System.ComponentModel.Composition;
using System.Windows;

namespace Scope3DView {

    [Export(typeof(ResourceDictionary))]
    partial class Options : ResourceDictionary {

        public Options() {
            InitializeComponent();
        }
    }
}