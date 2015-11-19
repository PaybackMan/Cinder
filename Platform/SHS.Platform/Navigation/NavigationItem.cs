using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.Security.Navigation
{
    public class NavigationItem
    {
        public NavigationItem()
        {
            this.IsDefault = false;
            SubItems       = new ObservableCollection<NavigationItem>();
        }

        public NavigationItem(string name)
        {
            this.Name      = name;
            this.IsDefault = false;
            SubItems       = new ObservableCollection<NavigationItem>();
        }
        public ObservableCollection<NavigationItem> SubItems { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string View { get; set; }
        public string ViewModel { get; set; }
        public string RequiredClaim { get; set; }
        public string AssemblyName { get; set; }
        public NavigationItem Parent { get; set; }
        public bool IsDefault { get; set; }
    
    }
}
