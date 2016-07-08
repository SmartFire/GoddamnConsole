using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GoddamnConsole.DataBinding;

namespace GoddamnConsole.Controls
{
    public interface IHasDataContext : INotifyPropertyChanged
    {
        object DataContext { get; }
        IHasDataContext ParentContainer { get; }
    }

    public abstract partial class Control : IHasDataContext
    {
        private readonly Dictionary<PropertyInfo, BindingInternal> _bindings
            = new Dictionary<PropertyInfo, BindingInternal>();

        private object _dataContext;
        
        /// <summary>
        /// Gets or sets the data context for an element
        /// </summary>
        public object DataContext
        {
            get { return _dataContext; }
            set
            {
                _dataContext = value;
                OnPropertyChanged();
                //foreach (var binding in _bindings.Values) binding.Refresh();
            }
        }
        
        [NoInvalidateOnChange]
        IHasDataContext IHasDataContext.ParentContainer => Parent;
    }
}
