using Dama.Data.Models;
using Dama.Organizer.Enums;
using System.Collections.Generic;

namespace Dama.Organizer.Others
{
    class OrderedLinkedList<T> : LinkedList<T> where T : Activity
    {
        private ComparableProperties _comparableProperties;
        private ComparableSequence _comparableSequence;

        public NameComparer<T> NameComparator { get; set; }

        public OrderedLinkedList(ComparableSequence comparableSequence, ComparableProperties comparableProperties)
        {
            _comparableSequence = comparableSequence;
            _comparableProperties = comparableProperties;
            SetComparator();
        }

        public void ResetElements(IEnumerable<T> list)
        {
            LinkedListNode<T> current = First;
            foreach (T i in list)
            {
                if (current == null)
                    current = new LinkedListNode<T>(i);
                else
                    current.Value = i;

                current = current.Next;
            }
        }
        public void SetComparatorProperty(ComparableProperties comparableProperty)
        {
            _comparableProperties = comparableProperty;
            SetComparator();
        }
        public void SetComparatorMethod(ComparableSequence comparableSequence)
        {
            _comparableSequence = comparableSequence;
            SetComparator();
        }
        public void SetComparatorProperties(ComparableSequence comparableSequence, ComparableProperties comparableProperty)
        {
            _comparableProperties = comparableProperty;
            _comparableSequence = comparableSequence;
            SetComparator();
        }
        public void AddSorted(T item)
        {
            LinkedListNode<T> currentNode = First;
            LinkedListNode<T> newNode = new LinkedListNode<T>(item);

            if (currentNode == null || NameComparator.Compare(currentNode.Value, newNode.Value) != -1)
            {
                AddFirst(newNode);
            }
            else
            {
                while (currentNode != null && NameComparator.Compare(currentNode.Value, newNode.Value) == -1)
                    currentNode = currentNode.Next;

                if (currentNode == null)
                    AddLast(newNode);
                else
                    AddBefore(currentNode, newNode);
            }
        }
        public void AddSortedRange(IEnumerable<T> list)
        {
            foreach (T i in list)
                AddSorted(i);
        }
        private void SetComparator()
        {
            NameComparator = new NameComparer<T>(_comparableSequence, _comparableProperties);
        }
    }
}
