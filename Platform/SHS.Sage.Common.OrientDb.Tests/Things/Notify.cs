using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests.Things
{
    public class Notify : _Thing, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Notify()
        {
            _names2.CollectionChanged += _names2_CollectionChanged;
        }

        private void _names2_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("OtherNames2");
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }

        private ObservableCollection<string> _names = new ObservableCollection<string>();
        public ObservableCollection<string> OtherNames
        {
            get { return _names; }
            set { _names = value; OnPropertyChanged("OtherNames"); }
        }

        private ObservableCollection<string> _names2 = new ObservableCollection<string>();
        public ObservableCollection<string> OtherNames2
        {
            get { return _names; }
            set { _names = value; OnPropertyChanged("OtherNames2"); }
        }

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
