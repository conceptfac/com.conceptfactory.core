using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Concept.UI
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class DrawScriptableAttribute : PropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class InfoAttribute : PropertyAttribute
    {
        public string message;

        public InfoAttribute(string message)
        {
            this.message = message;
        }
    }


    /// <summary>
    /// Controls if a field is visible based on another boolean field/property's current value.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class HideIfAttribute : PropertyAttribute
    {
        public string condition;

        /// <summary>
        ///  Controls if a field is visible based on another boolean field/property's current value.
        /// </summary>
        /// <param name="condition">The name of a boolean field or property to use for visibility</param>
        public HideIfAttribute(string condition)
        {
            this.condition = condition;
        }
    }


    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]

    public class ShowIfAttribute: PropertyAttribute
    {
        public string condition;

        public ShowIfAttribute(string condition)
        {
            this.condition = condition;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]

    public class SubPanelAttribute: PropertyAttribute
    {
        public string caption;
        public Color color;
        public Color backgroundColor;
       public SubPanelAttribute(string caption, string color = "#000000", string backgroundColor = "#E0E0E0")
        {
            this.caption = caption;
            ColorUtility.TryParseHtmlString(color, out this.color);
            ColorUtility.TryParseHtmlString(backgroundColor, out this.backgroundColor);
        }

    }


    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class VectorLabelsAttribute : PropertyAttribute
    {
        public readonly string[] Labels;

        public VectorLabelsAttribute(params string[] labels)
        {
            Labels = labels;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CustomButtonAttribute : PropertyAttribute
    {
        public string caption;

        public CustomButtonAttribute(string caption)
        {
            this.caption = caption;
        }
    }
}


