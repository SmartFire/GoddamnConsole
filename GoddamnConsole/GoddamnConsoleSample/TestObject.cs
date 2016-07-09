using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GoddamnConsoleSample
{
    class TestObject : INotifyPropertyChanged
    {
        private int _step;

        public void NextStep()
        {
            switch (_step++)
            {
                case 0:
                    Property3++;
                    break;
                case 1:
                    Property1.Property2 = new ObservableCollection<LastObject>
                    {
                        new LastObject
                        {
                            Property4 = "Value: 3"
                        },
                        new LastObject
                        {
                            Property4 = "Value: 2"
                        }
                    };
                    break;
                case 2:
                    Property3--;
                    break;
                case 3:
                    Property1.Property2[0].Property4 = "Value: 4";
                    break;
                case 4:
                    Property1 = new InnerObject
                    {
                        Property2 = new ObservableCollection<LastObject>
                        {
                            new LastObject
                            {
                                Property4 = "Value: 5"
                            }
                        }
                    };
                    break;
                case 5:
                    Property1.Property2.Insert(0, new LastObject { Property4 = "Done!" });
                    break;
            }
        }

        private InnerObject _property1 = new InnerObject
        {
            Property2 = new ObservableCollection<LastObject>
            {
                new LastObject
                {
                    Property4 = "Value: 0"
                },
                new LastObject
                {
                    Property4 = "Value: 1"
                }
            }
        };

        private int _property3;

        public InnerObject Property1
        {
            get { return _property1; }
            set { _property1 = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Property3
        {
            get { return _property3; }
            set { _property3 = value; OnPropertyChanged(); }
        }
    }

    class InnerObject : INotifyPropertyChanged
    {
        private ObservableCollection<LastObject> _property2;

        public ObservableCollection<LastObject> Property2
        {
            get { return _property2; }
            set { _property2 = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class LastObject : INotifyPropertyChanged
    {
        private string _property4;

        public string Property4
        {
            get { return _property4; }
            set { _property4 = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
