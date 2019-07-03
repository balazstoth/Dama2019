using System.Collections.Generic;
using System.Linq;

namespace Dama.Generate
{
    public class Tree<T>
    {
        public Leaf<T> RootItem { get; set; }

        public Leaf<T> this[int index]
        {
            get
            {
                int count = GetCount(RootItem);
                if (index < 0 || index >= count)
                    return null;

                ClearStatus(RootItem);
                return GetItemByIndex(index);
            }
        }

        public int Count => GetCount(RootItem);

        public Tree(int priority, T value)
        {
            RootItem = new Leaf<T>(priority, value);
        } 

        private int GetCount(Leaf<T> item)
        {
            if (item.Leaves.Count == 0)
                return 1;

            int sum = 0;

            foreach (var i in item.Leaves)
                sum += GetCount(i);

            return sum + 1;
        } 

        private void ClearStatus(Leaf<T> leaf) 
        {
            leaf.IsChecked = false;

            if (leaf.Leaves.Count != 0)
                foreach (var i in leaf.Leaves)
                    ClearStatus(i);
        }
        
        private Leaf<T> GetItemByIndex(int index)
        {
            Queue<Leaf<T>> itemQueue = new Queue<Leaf<T>>();
            itemQueue.Enqueue(RootItem);

            Leaf<T> currentItem;
            int i = -1;

            while (itemQueue.Any())
            {
                currentItem = itemQueue.Dequeue();
                i++;
                
                if (i == index)
                    return currentItem;

                if (currentItem.Leaves != null)
                    foreach (var child in currentItem.Leaves)
                        itemQueue.Enqueue(child);
            }

            return null;
        }
    }

    public class Leaf<T>
    {
        public T Value { get; set; }
        public int Priority { get; set; }
        public bool IsChecked { get; set; }
        public List<Leaf<T>> Leaves { get; set; }

        public Leaf(int priority, T value, List<Leaf<T>> childItems)
            :this(priority, value)
        {
            Leaves = childItems;
        }

        public Leaf(int priority, T value)
        {
            Priority = priority;
            IsChecked = false;
            Value = value;
            Leaves = new List<Leaf<T>>();
        }
    }
}
