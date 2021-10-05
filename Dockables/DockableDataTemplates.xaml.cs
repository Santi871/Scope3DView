using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Scope3DView.Dockables
{
    [Export(typeof(ResourceDictionary))]
    public partial class DockableDataTemplates : ResourceDictionary
    {
        public DockableDataTemplates()
        {
            InitializeComponent();
        }
    }
}