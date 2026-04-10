using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TheVayuputra.Core
{
    [CustomPropertyDrawer(typeof(ObservableValue<>), true)]
    public class ObservableValueDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> supportCache = new();

        private bool IsSupported(SerializedProperty property)
        {
            string key = property.propertyPath;

            if (supportCache.TryGetValue(key, out bool supported))
                return supported;

            SerializedProperty valueProp = property.FindPropertyRelative("_value");
            supported = valueProp != null;

            supportCache[key] = supported;
            return supported;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsSupported(property))
                return;

            SerializedProperty valueProp = property.FindPropertyRelative("_value");

            // 🔒 READ-ONLY START
            GUI.enabled = false;
            EditorGUI.PropertyField(position, valueProp, label, true);
            GUI.enabled = true;
            // 🔒 READ-ONLY END
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsSupported(property))
                return 0f;

            return EditorGUI.GetPropertyHeight(
                property.FindPropertyRelative("_value"),
                label,
                true
            );
        }
    }
}
