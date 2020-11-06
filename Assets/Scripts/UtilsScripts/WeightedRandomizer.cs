using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Utils {
    /// <summary>
    /// Static class to improve readability
    /// Example:
    /// <code>
    /// var selected = WeightedRandomizer.From(weights).TakeOne();
    /// </code>
    /// 
    /// </summary>
    public static class WeightedRandomizer
    {
        public static WeightedRandomizer<R> From<R>(Dictionary<R, float> spawnRate)
        {
            return new WeightedRandomizer<R>(spawnRate);
        }
    }

    public class WeightedRandomizer<T>
    {
        private Dictionary<T, float> _weights;

        /// <summary>
        /// Instead of calling this constructor directly,
        /// consider calling a static method on the WeightedRandomizer (non-generic) class
        /// for a more readable method call, i.e.:
        /// 
        /// <code>
        /// var selected = WeightedRandomizer.From(weights).TakeOne();
        /// </code>
        /// 
        /// </summary>
        /// <param name="weights"></param>
        public WeightedRandomizer(Dictionary<T, float> weights)
        {
            if (weights == null || weights.Count() == 0)
            {
                throw new System.Exception("Invalid parameter, tried to get random weighted from empty dictionary");
            }
            _weights = weights;
        }

        /// <summary>
        /// Randomizes one item
        /// </summary>
        /// <param name="spawnRate">An ordered list withe the current spawn rates. The list will be updated so that selected items will have a smaller chance of being repeated.</param>
        /// <returns>The randomized item.</returns>
        public T TakeOne()
        {
            // Sorts the spawn rate list
            var sortedSpawnRate = Sort(_weights);

            // Sums all spawn rates
            float sum = 0;
            foreach (var spawn in _weights)
            {
                sum += spawn.Value;
            }

            // Randomizes a number from Zero to Sum
            float roll = Random.Range(0, sum);

            // Finds chosen item based on spawn rate
            T selected = sortedSpawnRate[sortedSpawnRate.Count - 1].Key;
            foreach (var spawn in sortedSpawnRate)
            {
                if (roll < spawn.Value)
                {
                    selected = spawn.Key;
                    break;
                }
                roll -= spawn.Value;
            }

            // Returns the selected item
            return selected;
        }

        private List<KeyValuePair<T, float>> Sort(Dictionary<T, float> weights)
        {
            var list = new List<KeyValuePair<T, float>>(weights);

            // Sorts the Spawn Rate List for randomization later
            list.Sort(
                delegate (KeyValuePair<T, float> firstPair,
                         KeyValuePair<T, float> nextPair)
                {
                    return firstPair.Value.CompareTo(nextPair.Value);
                }
             );

            return list;
        }
    }
}
