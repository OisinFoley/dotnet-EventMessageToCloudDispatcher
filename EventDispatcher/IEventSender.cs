using System.Threading.Tasks;

namespace EventDispatcher.generic
{
    public interface IEventSender<T>
    {
        /// <summary>
        /// Sends provided array of items to the EventDispatcher
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task SendAsync(T[] items);
    }
}
