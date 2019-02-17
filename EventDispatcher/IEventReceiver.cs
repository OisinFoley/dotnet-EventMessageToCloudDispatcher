using System;
using System.Threading.Tasks;

namespace EventDispatcher.Generic
{
    public interface IEventReceiver<T>
    {
        void RegisterHandler(Func<T, Task> handler);
    }
}
