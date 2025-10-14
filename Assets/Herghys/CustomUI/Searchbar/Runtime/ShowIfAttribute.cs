using UnityEngine;

namespace Herghys.Utility
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public string conditionPropertyName;
        public object compareValue;

        public ShowIfAttribute(string conditionPropertyName, object compareValue)
        {
            this.conditionPropertyName = conditionPropertyName;
            this.compareValue = compareValue;
        }
    }
}