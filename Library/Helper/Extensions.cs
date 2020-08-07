using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Library
{
    public static class Extensions
    {
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        public static T ToEnum<T>(this string target)
        {
            if (!(typeof(T).IsEnum))
                throw new ArgumentException("The given type is not of type enum");

            if (Enum.TryParse(typeof(T), target, out var result))
                return (T)result;
            else
                throw new ArgumentException("The string " + target + "could not be converted to the target enum");
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerableList)
        {
            if (enumerableList != null)
            {
                var observableCollection = new ObservableCollection<T>();

                foreach (var item in enumerableList)
                    observableCollection.Add(item);

                return observableCollection;
            }
            return null;
        }

        public static int[] RandomizePositions(this int[] collection)
        {
            Random rand = new Random();
            for(int i = 0; i < collection.Length; i++)
            {
                int randomIndex = rand.Next(0, collection.Length);
                int temp = collection[i];
                collection[i] = collection[randomIndex];
                collection[randomIndex] = temp;
            }

            return collection;
        }

        public static string ToTxtLine(this int[] collection)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < collection.Length; i++)
            {
                builder.Append(collection[i]);

                if (i != collection.Length - 1)
                {
                    builder.Append(" ");
                }
            }

            return builder.ToString();
        }
    }
}
