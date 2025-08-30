using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BMATM.ViewModels.Base
{
    /// <summary>
    /// Base class for all ViewModels implementing INotifyPropertyChanged
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets the property value and raises PropertyChanged if the value has changed
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="field">Reference to the backing field</param>
        /// <param name="value">New value to set</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>True if the value was changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Called when the view associated with this ViewModel is loaded
        /// Override in derived classes to perform initialization logic
        /// </summary>
        public virtual void OnViewLoaded()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when the view associated with this ViewModel is unloaded
        /// Override in derived classes to perform cleanup logic
        /// </summary>
        public virtual void OnViewUnloaded()
        {
            // Override in derived classes
        }
    }
}