using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public static class SubscribeUtils
    {
        public static TResult GetProperty<TSource, TResult>(this TSource source, TResult value, [CallerMemberName] string propertyName = null)
            where TSource : class, INotifyPropertyChanged
        {
            NotifyContext.DependenciesCollectorContext.Subscribe(source, value, propertyName);
            return value;
        }

        public static void SubscribeProperty<TSource>(this TSource source, [CallerMemberName] string propertyName = null)
            where TSource : class, INotifyPropertyChanged
        {
            NotifyContext.DependenciesCollectorContext.SubscribeProperty(source, propertyName);
        }

        public static void PushDependencyFromCollection(this INotifyCollectionChanged source)
        {
            NotifyContext.DependenciesCollectorContext.PushDependencyFromCollection(source);
        }

        public static void RaisePropertyChanged<T>(this PropertyChangedEventHandler @delegate,
            T sender, [CallerMemberName]string propertyName = null)
            where T : INotifyPropertyChanged
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (@delegate != null)
            {
                using (NotifyContext.IgnoreChildChanges())
                {
                    @delegate(sender, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
