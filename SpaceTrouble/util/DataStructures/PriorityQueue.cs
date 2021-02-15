using System.Collections.Generic;

namespace SpaceTrouble.util.DataStructures {
    internal sealed class PriorityQueue<T> {
        private readonly List<KeyValuePair<T, float>> mElements = new List<KeyValuePair<T, float>>();
        public int Count => mElements.Count;

        public void Enqueue(T item, float priority) {
            mElements.Add(new KeyValuePair<T, float>(item, priority));
        }

        // Returns the Location that has the lowest priority
        public T Dequeue() {
            var bestIndex = 0;

            for (var i = 0; i < mElements.Count; i++) {
                if (mElements[i].Value < mElements[bestIndex].Value) {
                    bestIndex = i;
                }
            }

            var bestItem = mElements[bestIndex].Key;
            mElements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}
