using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Concept.Helpers
{
    public static class SelfDestroy
    {

        /// <summary>
        /// Destroy GameObject delayed.
        /// </summary>
        /// <param name="gameObject">Target to destroy.</param>
        /// <param name="delay">Time until destroyed.</param>
        /// <returns></returns>
        public static async Task<bool> DestroyIn(this GameObject gameObject, float delay)
        {
            await Task.Delay((int)(delay * 1000)); 

            GameObject.Destroy(gameObject); 

            return true; 
        }
    }
}
