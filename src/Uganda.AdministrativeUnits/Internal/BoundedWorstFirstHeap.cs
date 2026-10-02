using System;
using System.Collections.Generic;

namespace Uganda.AdministrativeUnits.Internal;

/// <summary>
/// Bounded max-heap used by search: the worst hit sits at the root so it can be replaced quickly.
/// Avoids <c>PriorityQueue&lt;T&gt;</c>, which is not available on netstandard2.0 / .NET Framework.
/// </summary>
internal sealed class BoundedWorstFirstHeap<T>
{
    private readonly List<T> _items = new();
    private readonly Comparison<T> _worseThan;

    public BoundedWorstFirstHeap(Comparison<T> worseThan)
    {
        _worseThan = worseThan;
    }

    public int Count => _items.Count;

    public void Enqueue(T item)
    {
        _items.Add(item);
        SiftUp(_items.Count - 1);
    }

    public T Dequeue()
    {
        int last = _items.Count - 1;
        T root = _items[0];
        _items[0] = _items[last];
        _items.RemoveAt(last);
        if (_items.Count > 0)
        {
            SiftDown(0);
        }

        return root;
    }

    public bool TryPeek(out T item)
    {
        if (_items.Count == 0)
        {
            item = default!;
            return false;
        }

        item = _items[0];
        return true;
    }

    public void ReplaceRoot(T item)
    {
        _items[0] = item;
        SiftDown(0);
    }

    private void SiftUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (_worseThan(_items[index], _items[parent]) <= 0)
            {
                break;
            }

            Swap(index, parent);
            index = parent;
        }
    }

    private void SiftDown(int index)
    {
        int count = _items.Count;
        while (true)
        {
            int left = (index * 2) + 1;
            if (left >= count)
            {
                break;
            }

            int right = left + 1;
            int worstChild = right < count && _worseThan(_items[right], _items[left]) > 0 ? right : left;
            if (_worseThan(_items[worstChild], _items[index]) <= 0)
            {
                break;
            }

            Swap(index, worstChild);
            index = worstChild;
        }
    }

    private void Swap(int i, int j)
    {
        T tmp = _items[i];
        _items[i] = _items[j];
        _items[j] = tmp;
    }
}
