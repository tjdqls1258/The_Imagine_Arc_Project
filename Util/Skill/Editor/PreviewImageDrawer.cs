#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PreviewImageAttribute))]
public class PreviewImageDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 원본 변수(오브젝트 필드)의 높이
        float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);

        // 이미지가 할당되어 있다면 프리뷰 높이만큼 공간을 더 확보
        if (property.objectReferenceValue != null)
        {
            PreviewImageAttribute attr = (PreviewImageAttribute)attribute;
            return baseHeight + attr.PreviewHeight + 5f; // 5f는 여백
        }

        return baseHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, property, label, true);

        if (property.objectReferenceValue != null)
        {
            PreviewImageAttribute attr = (PreviewImageAttribute)attribute;

            Rect previewRect = new Rect(
                position.x + EditorGUIUtility.labelWidth, // 라벨 이름 텍스트 우측에 정렬
                position.y + EditorGUIUtility.singleLineHeight + 2f,
                attr.PreviewHeight,
                attr.PreviewHeight
            );

            Texture2D previewTexture = AssetPreview.GetAssetPreview(property.objectReferenceValue);
            if (previewTexture != null)
            {
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.LabelField(previewRect, "No Preview");
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif