using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsCommander.Core
{
    /// <summary>
    /// Basisklasse für die Implementierung von NotifyPropertyChanged-Klassen und insbesondere für die MVVM-Modellklasse
    /// </summary>
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Hit Hilfe dieses Ereignisses werden Änderungen signalisiert
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        /// <summary>
        /// Wenn sich etwas in der Klasse ändert, muss diese Methode aufgerufen werden, damit die Signalisierung erfolgt
        /// </summary>
        /// <param name="propertyName">Der Name des Properties, welches sich geändert hat</param>
        protected void Changed([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
