using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnumeratedArrayAttribute))]
public class EnumeratedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        int idx = -1;

        if (property.isFixedBuffer)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, property);

            if (property.isExpanded)
            {
                var names = ((EnumeratedArrayAttribute)attribute).names;
                int origIndent = EditorGUI.indentLevel;
                idx = 0;

                SerializedProperty endProperty = property.GetEndProperty();

                while (property.NextVisible(true) && !SerializedProperty.EqualContents(property, endProperty))
                {
                    if (property.name == "size")
                        continue;

                    var name = idx >= 0 && idx < names.Length ? names[idx] : "Unknown (" + idx + ")";
                    idx++;

                    rect.height = EditorGUI.GetPropertyHeight(property);
                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.indentLevel = property.depth;
                    EditorGUI.PropertyField(rect, property, new GUIContent(name));
                }

                EditorGUI.indentLevel = origIndent;
            }
        }
        else
        {
            bool ok = int.TryParse(property.propertyPath.AfterLast("[").BeforeFirst("]"), out idx);
            var names = ((EnumeratedArrayAttribute)attribute).names;
            var name = ok && idx >= 0 && idx < names.Length ? names[idx] : "Unknown (" + idx + ")";
            EditorGUI.PropertyField(rect, property, new GUIContent(name));
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isFixedBuffer)
        {
            if (property.isExpanded)
            {
                return EditorGUI.GetPropertyHeight(property) - EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return EditorGUI.GetPropertyHeight(property);
            }
        }
        else
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
