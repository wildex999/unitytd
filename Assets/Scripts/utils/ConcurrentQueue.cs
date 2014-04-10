using System.Collections.Generic;

//FIFO queue, Thread Safe
public class ConcurrentQueue<T>
{
    Queue<T> queue = new Queue<T>();

    //Push to end of queue
    public void push(T item)
    {
        lock (queue)
        {
            queue.Enqueue(item);
        }
    }

    //Get first element(or null if no element)
    public T pop()
    {
        T item;
        lock(queue)
        {
            if (queue.Count != 0)
                item = queue.Dequeue();
            else
                item = default(T);
        }
        return item;
    }

    //Pop all current elements
    public List<T> popAll()
    {
        List<T> items = new List<T>();
        lock(queue)
        {
            while(queue.Count > 0)
                items.Add(queue.Dequeue());
        }

        return items;
    }

    //Pop up to a number of elements
    public List<T> popMultiple(int maxItems)
    {
        List<T> items = new List<T>();
        lock(queue)
        {
            while (queue.Count > 0 && maxItems-- > 0)
                items.Add(queue.Dequeue());
        }

        return items;
    }

    public int count()
    {
        int count;
        lock(queue)
        {
            count = queue.Count;
        }
        return count;
    }

    public void clear()
    {
        lock(queue)
        {
            queue.Clear();
        }
    }
}