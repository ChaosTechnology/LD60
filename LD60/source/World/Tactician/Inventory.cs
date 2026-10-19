using System;
using ChaosUtil.Reflection;
using SysCol = System.Collections.Generic;

namespace LD60.World.Tactician
{
    public class Inventory
    {
        public int gold = 100;
        readonly SysCol.Dictionary<Type, int> arsenal = [];

        public int GetArsenal<T>()
            where T : Placable
            => GetArsenalInternal(typeof(T));

        public int GetArsenal(Type t)
            => !t.InheritsOrImplements(typeof(Placable))
                ? throw new ArgumentException($"{nameof(t)} must be {nameof(Placable)}", nameof(t))
                : GetArsenalInternal(t);

        int GetArsenalInternal(Type t)
            => arsenal.TryGetValue(t, out int result) ? result : 0;

        public int AddStock<T>(int count)
            where T : Placable
            => AddStockInternal(typeof(T), count);

        public int AddStock(Type t, int count)
            => !t.InheritsOrImplements(typeof(Placable))
                ? throw new ArgumentException($"{nameof(t)} must be {nameof(Placable)}", nameof(t))
                : AddStockInternal(t, count);

        int AddStockInternal(Type t, int count)
        {
            if (count <= 0)
                throw new ArgumentException($"Can't add negative amounts!", nameof(count));

            int newStock = arsenal.TryGetValue(t, out int arse) ? arse + count : count;
            arsenal[t] = newStock;
            return newStock;
        }

        public int Use<T>()
            where T : Placable
            => GetArsenalInternal(typeof(T));

        public int Use(Type t)
            => !t.InheritsOrImplements(typeof(Placable))
                ? throw new ArgumentException($"{nameof(t)} must be {nameof(Placable)}", nameof(t))
                : UseInternal(t);

        int UseInternal(Type t)
        {
            if (!arsenal.TryGetValue(t, out int i) || i <= 0)
                throw new InvalidOperationException($"Can't use something you don't have.");

            int newStock = i - 1;
            arsenal[t] = newStock;
            return newStock;
        }
    }
}
