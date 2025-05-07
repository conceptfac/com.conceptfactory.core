using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Concept.Helpers
{

public static class JsonExtensions
{
        public static bool IsValidJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();
            try
            {
                JToken.Parse(input); 
                return true; 
            }
            catch (JsonReaderException)
            {
                return false; 
            }
        }

        public static bool IsValidJson(byte[] data)
        {
            string jsonString = Encoding.UTF8.GetString(data);

            return IsValidJson(jsonString); 
        }

    }

}