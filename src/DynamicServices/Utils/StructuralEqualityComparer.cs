using System.Collections;
using System.Collections.Generic;

namespace DynamicServices.Utils {
    // https://stackoverflow.com/questions/3609823/what-problem-does-istructuralequatable-and-istructuralcomparable-solve/5601068#5601068
    internal class StructuralEqualityComparer<T> : IEqualityComparer<T> {

        public bool Equals(T a, T b) => 
            StructuralComparisons.StructuralEqualityComparer.Equals(a, b);

        public int GetHashCode(T obj) =>
            StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);

        private static StructuralEqualityComparer<T> _Default;
        public static StructuralEqualityComparer<T> Default => _Default ?? (_Default = new StructuralEqualityComparer<T>());

    }
}
