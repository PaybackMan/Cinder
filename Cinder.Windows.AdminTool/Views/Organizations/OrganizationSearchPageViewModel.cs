using Cinder.Core.Domain.Administrative;
using Prism.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Windows.DocFlock.Views.Organizations
{
    public class OrganizationSearchPageViewModel : BindableBase, INavigationAware
    {
        public ObservableCollection<Organization> Organizations { get; set; }

        public OrganizationSearchPageViewModel()
        {
           
        }
        public void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {

           // throw new NotImplementedException();
        }

        public void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            //throw new NotImplementedException();
        }
    }
}
