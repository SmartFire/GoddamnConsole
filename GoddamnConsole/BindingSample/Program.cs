using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GoddamnConsole;
using GoddamnConsole.Controls;
using GoddamnConsole.DataBinding;
using GoddamnConsole.Drawing;

namespace BindingSample
{
    class Program
    {
        static void Main()
        {
            var obj = new TestObject();
            var btn = new Button
            {
                Text = "Click here!",
                AttachedProperties =
                {
                    new GridProperties
                    {
                        Row = 1
                    }
                },
                Height = ControlSizeType.MaxByContent
            };
            btn.Clicked += (o, e) => obj.NextStep();
            var val = new TextView
            {
                Text = "Value: unbound",
                AttachedProperties =
                {
                    new GridProperties
                    {
                        Row = 2
                    }
                },
                DataContext = obj
            };
            val.Bind(nameof(TextView.Text), "Property1.Property2[Property3].Property4", BindingMode.TwoWay);
            var window = new GridWindow
            {
                Title = "Binding Sample",
                RowDefinitions =
                {
                    new GridSize(GridUnitType.Auto, 0),
                    new GridSize(GridUnitType.Auto, 0),
                    new GridSize(GridUnitType.Auto, 0)
                },
                Children =
                {
                    new TextView
                    {
                        Text = "Press button below 6 times. If binding works properly, you will see that value is incremented\n" +
                               "If you see \"Value: unbound\", binding does not work",
                        TextWrapping = TextWrapping.Wrap,
                        Height = ControlSizeType.MaxByContent
                    },
                    btn,
                    val
                }
            };
            Console.Windows.Add(window);
            Console.Start();
        }
    }
    
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
            set { _property4 = value; OnPropertyChanged();}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}