using System.Collections.Generic;
using System.Xaml;

namespace GoddamnConsole.Controls
{
    public interface IBetterAttachedPropertyStore : IAttachedPropertyStore
    {
        T GetValue<T>(AttachableMemberIdentifier identifier);
    }

    public abstract partial class Control : IBetterAttachedPropertyStore
    {
        private readonly IDictionary<AttachableMemberIdentifier, object> _attachedProperties =
            new Dictionary<AttachableMemberIdentifier, object>();

        void IAttachedPropertyStore.CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            _attachedProperties.CopyTo(array, index);
        }

        bool IAttachedPropertyStore.RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier)
        {
            return _attachedProperties.Remove(attachableMemberIdentifier);
        }

        void IAttachedPropertyStore.SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value)
        {
            _attachedProperties[attachableMemberIdentifier] = value;
        }

        bool IAttachedPropertyStore.TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value)
        {
            return _attachedProperties.TryGetValue(attachableMemberIdentifier, out value);
        }

        T IBetterAttachedPropertyStore.GetValue<T>(AttachableMemberIdentifier attachableMemberIdentifier)
        {
            object val;
            return _attachedProperties.TryGetValue(attachableMemberIdentifier, out val) ? (T) val : default(T);
        }

        int IAttachedPropertyStore.PropertyCount => _attachedProperties.Count;
    }
}
