using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downlink
{
    public static class Extensions
    {
        /// <summary>
        /// Computes the sample Variance of a sequence of int values.
        /// </summary>
        /// <param name="source">A sequence of int values to calculate the Variance of.</param>
        /// <returns>       
        ///     The Variance of the sequence of values.
        /// </returns>
        public static double Variance(this IEnumerable<double> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            long n = 0;
            double mean = 0;
            double M2 = 0;
            checked
            {
                foreach (double x in source)
                {
                    n++;

                    double delta = x - mean;
                    mean += delta / n;
                    M2 += delta * (x - mean);
                }
            }
            return (n < 2) ?  0d: M2 / (n - 1);
        }

        /// <summary>
        /// Computes the sample Variance of a sequence of int values that are obtained
        ///     by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence of elements.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>       
        ///     The Variance of the sequence of values.
        /// </returns>
        public static double Variance<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return source.Select(selector).Variance();
        }

        /// <summary>
        /// Computes the sample StandardDeviation of a sequence of int values.
        /// </summary>
        /// <param name="source">A sequence of int values to calculate the StandardDeviation of.</param>
        /// <returns>       
        ///     The StandardDeviation of the sequence of values.
        /// </returns>
        public static double StandardDeviation(this IEnumerable<double> source)
        {
            return Math.Sqrt((double)source.Variance());
        }

        /// <summary>
        /// Computes the sample StandardDeviation of a sequence of double values that are obtained
        ///     by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence of elements.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>       
        ///     The StandardDeviation of the sequence of values.
        /// </returns>
        public static double StandardDeviation<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return source.Select(selector).StandardDeviation();
        }

        public static double SafeAverage(this IEnumerable<double> source)
        {
            double sum = 0d;
            long n = 0;
            checked
            {
                foreach (double x in source)
                {
                    n++;
                    sum += x;
                }
            }
            return n == 0 ? double.NaN : sum / n;
        }

        public static double SafeAverage<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return source.Select(selector).SafeAverage();
        }

        public static double SafeMin(this IEnumerable<double> source)
        {
            double min = double.MaxValue;
            long n = 0;
            checked
            {
                foreach (double x in source)
                {
                    n++;
                    if (x < min)
                        min = x;
                }
            }
            return n == 0 ? double.NaN : min;
        }

        public static double SafeMin<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return source.Select(selector).SafeMin();
        }

        public static double SafeMax(this IEnumerable<double> source)
        {
            double max = double.MinValue;
            long n = 0;
            checked
            {
                foreach (double x in source)
                {
                    n++;
                    if (x > max)
                        max = x;
                }
            }
            return n == 0 ? double.NaN : max;
        }

        public static double SafeMax<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return source.Select(selector).SafeMax();
        }
    }
}
