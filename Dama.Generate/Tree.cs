using System.Collections.Generic;

namespace Dama.Generate
{
    public class Tree<T>
    {
        private int tmpCount = 0;

        public Leaf<T> RootItem { get; set; }

        public Leaf<T> this[int index]
        {
            get
            {
                int count = GetCount(RootItem);
                if (!(index >= 0 && index < count))
                    return null;

                ClearStatus(RootItem);
                return Iterate(RootItem, index);
            }
        }

        public Tree(int priority, T value)
        {
            RootItem = new Leaf<T>(priority, value);
        }

        int GetCount(Leaf<T> item)
        {
            if (item.Leaves.Count == 0)
                return 1;

            int sum = 0;
            foreach (var i in item.Leaves)
                sum += GetCount(i);

            return sum + 1;
        }

        void ClearStatus(Leaf<T> leaf)
        {
            leaf.IsChecked = false;

            if (leaf.Leaves.Count == 0)
                return;

            foreach (var i in leaf.Leaves)
                ClearStatus(i);
            return;
        }

        Leaf<T> Iterate(Leaf<T> leaf, int index)
        {
            if (tmpCount == index)
                return leaf;

            if (leaf.IsChecked == false && leaf != RootItem)
            {
                leaf.IsChecked = true;
                return null;
            }
            else
            {
                Leaf<T> result = null;
                while (result == null)
                {
                    foreach (var item in leaf.Leaves)
                    {
                        if (item.IsChecked == false)
                            tmpCount++;

                        result = Iterate(item, index);
                        if (result != null)
                            return result;
                    }

                    if (leaf != RootItem)
                        return null;
                }
                return result;
            }
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
