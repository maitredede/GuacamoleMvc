using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Guacamole.Client
{
    public abstract class SynchronizedDataItem : INotifyPropertyChanged
    {
        protected readonly SynchronizationContext m_syncContext;

        public SynchronizedDataItem()
        {
            this.m_syncContext = SynchronizationContext.Current;
        }

        protected void Set(string value, ref string field, [CallerMemberName] string propName = null)
        {
            if (field != value)
            {
                field = value;
                this.RaisePropertyChanged(propName);
            }
        }

        protected virtual void RaisePropertyChanged(string propName)
        {
            PropertyChangedEventHandler d = this.PropertyChanged;
            if (d != null)
            {
                if (this.m_syncContext != null)
                {
                    this.m_syncContext.Send((state) =>
                    {
                        d(this, new PropertyChangedEventArgs((string)state));
                    }, propName);
                }
                else
                {
                    d(this, new PropertyChangedEventArgs(propName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
