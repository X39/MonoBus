namespace X39.MonoBus;

public class MonoConsumerConfiguration
{
    public EQueueMode QueueMode { get; set; }
}

public enum EQueueMode
{
    /// <summary>
    /// Creates a new queue for the <see cref="IConsumer{T}"/>.
    /// Receives all messages of a given type.
    /// </summary>
    ListenToAll,
    
    /// <summary>
    /// Single queue that wakes a single sleeping consumer for every message received and is shared
    /// among all <see cref="IConsumer{T}"/> with the same T type and <see cref="SharedQueue"/>. 
    /// </summary>
    SharedQueue,
    
    /// <summary>
    /// Single queue that wakes all sleeping consumers for every message received and is shared 
    /// among all <see cref="IConsumer{T}"/> with the same T type and <see cref="SharedBulkQueue"/>. 
    /// </summary>
    SharedBulkQueue,
}