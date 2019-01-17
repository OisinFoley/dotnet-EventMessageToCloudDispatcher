using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventDispatcher
{
    public interface IEventReceiver<T>
    {
        void RegisterMessagHandler(Func<T, Task> handler);
    }
}
