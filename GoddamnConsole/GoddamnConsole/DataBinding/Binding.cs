using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using GoddamnConsole.Controls;

namespace GoddamnConsole.DataBinding
{
    public class Binding : MarkupExtension
    {
        public Binding(string shit)
        {
            _path = shit;
        }

        private readonly string _path;
        public BindingMode Mode { get; set; } = BindingMode.OneWay;
        private BindingInternal _internalBinding;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = (IProvideValueTarget) serviceProvider.GetService(typeof (IProvideValueTarget));
            var property = (PropertyInfo) pvt.TargetProperty;
            var target = (IHasDataContext) pvt.TargetObject;
            _internalBinding = new BindingInternal(target, property, _path, Mode, true);
            return _internalBinding.Value();
        }
    }

    internal class BindingInternal
    {
        private class BindingNode
        {
            public object Object { get; set; }
            public Type ObjectType { get; set; }
            public PropertyInfo Property { get; set; }
            public PropertyChangedEventHandler PcHandler { get; set; }
            public NotifyCollectionChangedEventHandler CcHandler { get; set; }
            public object[] Indices { get; set; } 
        }
        
        private readonly IHasDataContext _control;
        private readonly PropertyInfo _property;
        
        private readonly List<BindingNode> _nodes = new List<BindingNode>();
        private readonly bool _strict;
        private readonly BindingMode _mode;
        private readonly BindingPath _path;

        public BindingInternal(IHasDataContext control, PropertyInfo property, string path, BindingMode mode, bool strict)
        {
            _path = new BindingPath(path);
            _property = property;
            _control = control;
            _mode = mode;
            _strict = strict;
            control.PropertyChanged += OnTargetPropertyChanged;
            Refresh();
        }

        private readonly object _lock = new object();
        private BindingNode _changingNode;

        private object GetDataContext(IHasDataContext cont)
        {
            return cont.DataContext ?? (cont.ParentContainer != null ? GetDataContext(cont.ParentContainer) : null);
        }

        public object Value() => Traverse(_path.Nodes, GetDataContext(_control));

        private void OnTargetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IHasDataContext.DataContext) ||
                e.PropertyName == nameof(IHasDataContext.ParentContainer))
            {
                Refresh();
                return;
            }
            if (_mode == BindingMode.OneWayToSource || _mode == BindingMode.TwoWay)
            {
                var last = _nodes.LastOrDefault();
                if (last == null) return;
                lock (_lock)
                {
                    try
                    {
                        _changingNode = last;
                        if (last.Indices == null)
                            last.Property.SetValue(last.Object, _property.GetValue(_control));
                        else
                            last.Property.SetValue(last.Object, _property.GetValue(_control), last.Indices);
                    }
                    catch
                    {
                        if (_strict) throw;
                    }
                    finally
                    {
                        _changingNode = null;
                    }
                }
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = _nodes.FirstOrDefault(x => x.Object == sender && x.Property.Name == e.PropertyName);
            if (node != null && node != _changingNode) Refresh();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var node = _nodes.FirstOrDefault(x => x.Object == sender);
            if (node != null && node != _changingNode) Refresh();
        }

        public void Cleanup(bool unbindTarget = false)
        {
            if (unbindTarget)
                _control.PropertyChanged -= OnTargetPropertyChanged;
            foreach (var node in _nodes.Where(x => x.PcHandler != null))
            {
                ((INotifyPropertyChanged)node.Object).PropertyChanged -= node.PcHandler;
            }
            foreach (var node in _nodes.Where(x => x.CcHandler != null))
            {
                ((INotifyCollectionChanged) node.Object).CollectionChanged -= node.CcHandler;
            }
            _nodes.Clear();
        }

        private static readonly Type[] NumberTypes =
        {
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(sbyte),
            typeof(byte),
            typeof(float),
            typeof(double),
            typeof(decimal),
        };

        private Type TypeOfIndex(Index index, object dc, Type target)
        {
            switch (index.Type)
            {
                case IndexType.Boolean:
                    return typeof(bool);
                case IndexType.String:
                    return typeof(string);
                case IndexType.Number:
                    if (NumberTypes.Contains(target)) return target;
                    throw new Exception("Target type mismatch");
                case IndexType.Path:
                    return Traverse(index.Path, dc).GetType();
                default:
                    throw new Exception("Unknown index type");
            }
        }

        private object EvaluateIndex(Index index, object dc, Type target)
        {
            switch (index.Type)
            {
                case IndexType.Boolean:
                    return index.Boolean;
                case IndexType.String:
                    return index.String;
                case IndexType.Number:
                    if (NumberTypes.Contains(target)) return Convert.ChangeType(index.Number, target);
                    throw new Exception("Target type mismatch");
                case IndexType.Path:
                    return Traverse(index.Path, dc);
                default:
                    throw new Exception("Unknown index type");
            }
        }

        private object Traverse(List<BindingPathNode> nodes, object dc)
        {
            if (dc == null) return null;
            var rdc = dc;
            try
            {
                foreach (var node in nodes)
                {
                    var dct = dc.GetType();
                    if (node.Property != null)
                    {
                        var prop = dct.GetProperty(node.Property);
                        if (prop == null)
                            throw new Exception($"Property does not exist: {dct.Name}.{node.Property}");
                        var val = prop.GetValue(dc);
                        if (!_nodes.Exists(x => x.Object == dc && x.PcHandler != null && x.Property == prop))
                        {
                            PropertyChangedEventHandler pc = null;
                            if (dc is INotifyPropertyChanged)
                            {
                                pc = OnPropertyChanged;
                                ((INotifyPropertyChanged) dc).PropertyChanged += pc;
                            }
                            _nodes.Add(new BindingNode
                            {
                                Property = prop,
                                Object = dc,
                                ObjectType = dct,
                                PcHandler = pc
                            });
                        }
                        dc = val;
                        dct = dc.GetType();
                    }
                    if (node.Indices.Count > 0)
                    {
                        var prop = dct.GetProperties().FirstOrDefault(x =>
                        {
                            var xindices = x.GetIndexParameters();
                            if (xindices.Length == node.Indices.Count)
                                return
                                    xindices.Zip(node.Indices, (a, b) => a.ParameterType.IsAssignableFrom(TypeOfIndex(b, rdc, a.ParameterType)))
                                           .All(a => a);
                            return false;
                        });
                        if (prop == null) throw new Exception("Indexer does not exist");
                        var indices = prop.GetIndexParameters();
                        var realIndices = node.Indices.Zip(indices,
                                                           (x, y) => EvaluateIndex(x, rdc, y.ParameterType))
                                              .ToArray();
                        var val = prop.GetValue(
                            dc,
                            realIndices);
                        if (!_nodes.Exists(x => x.Object == dc && x.CcHandler != null && x.Property == prop))
                        {
                            NotifyCollectionChangedEventHandler cc = null;
                            if (dc is INotifyCollectionChanged)
                            {
                                cc = OnCollectionChanged;
                                ((INotifyCollectionChanged)dc).CollectionChanged += cc;
                            }
                            _nodes.Add(new BindingNode
                            {
                                Property = prop,
                                Object = dc,
                                ObjectType = dct,
                                CcHandler = cc,
                                Indices = realIndices
                            });
                        }
                        dc = val;
                    }
                }
            }
            catch
            {
                if (_strict) throw;
            }
            return dc;
        }

        public void Refresh()
        {
            Cleanup();
            var dc = GetDataContext(_control);
            if (dc == null) return;
            try
            {
                _property.SetValue(_control, Traverse(_path.Nodes, dc));
            }
            catch
            {
                if (_strict) throw;
            }
        }
    }

    public enum BindingMode
    {
        OneWay,
        TwoWay,
        OneWayToSource
    }
}
