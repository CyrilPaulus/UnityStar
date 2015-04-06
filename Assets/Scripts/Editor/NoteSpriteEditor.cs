using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NoteSprite))]
public class NoteSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var noteSprite = target as NoteSprite;
        noteSprite.Width = EditorGUILayout.FloatField("Width", noteSprite.Width);
    }
}
