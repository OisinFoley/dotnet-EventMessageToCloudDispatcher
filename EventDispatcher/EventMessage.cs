namespace EventDispatcher
{
    public sealed class EventMessage<T>
    {
        /// <summary>
        /// Event identifier, if activated within portal, this can be used to identify duplicate messages.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifies event type.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Content of the event data.
        /// </summary>
        public T Content { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Header: {Header}, Content: {Content}";
        }
    }
}
