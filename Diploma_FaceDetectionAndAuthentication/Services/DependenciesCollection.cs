using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Diploma_FaceDetectionAndAuthentication.Services
{
    public struct DependenciesCollection<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
    {
        private int[] _buckets;
        private Slot[] _slots;

        private int _lastIndex;
        private int _freeIndex;

        private DependenciesCollection(EmptyUnit emptyUnit)
        {
            _buckets = CreateBucketsArray(7);
            _slots = new Slot[7];

            _lastIndex = -1;
            _freeIndex = -1;
        }

        public ref TValue GetOrRegisterSlotFor(TKey key, out bool isFree)
        {
            var hashcode = CalculateHashcode(key);
            ref var refBucketIndex = ref _buckets[hashcode % _buckets.Length];

            for (int i = refBucketIndex; i >= 0; i = _slots[i].Next)
            {
                if (hashcode == _slots[i].Hashcode && key.Equals(_slots[i].Key))
                {
                    isFree = false;
                    return ref _slots[i].Value;
                }
            }

            isFree = true;
            return ref AddNewSlot(key, ref refBucketIndex, hashcode);
        }

        private ref TValue AddNewSlot(TKey key, ref int refBucketIndex, int hashcode)
        {
            int index = _freeIndex;
            if (index >= 0)
            {
                ref var refSlot = ref _slots[index];
                _freeIndex = refSlot.Next;

                refSlot.Next = refBucketIndex;
                refSlot.Key = key;
                refSlot.Hashcode = hashcode;
                refBucketIndex = index;
                return ref refSlot.Value;
            }

            index = ++_lastIndex;
            if (index < _buckets.Length)
            {
                ref var refSlot = ref _slots[index];

                refSlot.Next = refBucketIndex;
                refSlot.Key = key;
                refSlot.Hashcode = hashcode;
                refBucketIndex = index;
                return ref refSlot.Value;
            }

            return ref Resize(key);
        }

        private ref TValue Resize(TKey key)
        {
            var newSize = checked(_slots.Length * 2 + 1);
            Array.Resize(ref _slots, newSize);

            int[] newBuckets = CreateBucketsArray(newSize);
            for (int i = 0; i < _lastIndex; i++)
            {
                int bucket = _slots[i].Hashcode % newSize;
                _slots[i].Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }

            _buckets = newBuckets;

            var hashcode = CalculateHashcode(key);
            ref var refBucketIndex = ref _buckets[hashcode % _buckets.Length];
            ref var slot = ref _slots[_lastIndex];

            slot.Next = refBucketIndex;
            slot.Key = key;
            slot.Hashcode = hashcode;
            refBucketIndex = _lastIndex;
            return ref slot.Value;
        }

        public void Remove<TFunc>(TFunc canClear)
            where TFunc : struct, IFunc<TKey, TValue, bool>
        {
            for (int i = 0; i < _buckets.Length; i++)
            {
                ref int refNextIndex = ref _buckets[i];
                int index = refNextIndex;
                if (index < 0)
                {
                    continue;
                }

                Slot slot;
                for (var iSlot = refNextIndex; iSlot >= 0; iSlot = slot.Next)
                {
                    ref var refSlot = ref _slots[iSlot];
                    slot = refSlot;
                    if (canClear.Invoke(slot.Key, slot.Value))
                    {
                        refNextIndex = slot.Next;
                        refSlot.Clear(_freeIndex);
                        _freeIndex = iSlot;
                    }
                    else
                    {
                        refNextIndex = ref refSlot.Next;
                    }
                }
            }
        }

        public void Clear()
            => this = new DependenciesCollection<TKey, TValue>(default);

        private int CalculateHashcode(TKey key)
            => key.GetHashCode() & int.MaxValue;

        private static int[] CreateBucketsArray(int size)
        {
            var result = new int[size];
            for (int i = 0; i < result.Length; i++)
            {
                --result[i];
            }
            return result;
        }

        public static DependenciesCollection<TKey, TValue> Create()
            => new DependenciesCollection<TKey, TValue>(default);

        private struct Slot
        {
            public TKey Key;
            public TValue Value;
            public int Next;
            public int Hashcode;

            internal void Clear(int freeIndex)
            {
                this = default;
                Next = freeIndex;
            }
        }
    }

    public readonly struct EmptyUnit { }
}
