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

        IHasDataContext IHasDataContext.ParentContainer => Parent;

        /// <summary>
        /// Binds the element property to the data context
        /// </summary>
        //public void Bind(string propertyName, string bindingPath, BindingMode mode = BindingMode.OneWay)
        //{
        //    var property = GetType().GetProperty(propertyName);
        //    if (property == null) throw new ArgumentException("Property not found");
        //    Unbind(propertyName);
        //    _bindings.Add(property, new BindingInternal(this, property, bindingPath, mode, true));
        //}

        /// <summary>
        /// Unbinds the element property
        /// </summary>
        /// <param name="propertyName"></param>
        //public void Unbind(string propertyName)
        //{
        //    var property = GetType().GetProperty(propertyName);
        //    BindingInternal existingBinding;
        //    _bindings.TryGetValue(property, out existingBinding);
        //    if (existingBinding == null) return;
        //    existingBinding.Cleanup(true);
        //    _bindings.Remove(property);
        //}
    }
}
