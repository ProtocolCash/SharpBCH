using System.Collections.Generic;

namespace SharpBCH.Util
{
    public static class QueueHelper
    {
        public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize)
        {
            for (var i = 0; i < chunkSize && queue.Count > 0; i++)
                yield return queue.Dequeue();
        }
    }
}