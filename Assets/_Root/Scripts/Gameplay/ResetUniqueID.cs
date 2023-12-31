using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ResetUniqueID : MonoBehaviour
{
    [SerializeField] private List<SaveDataElement> saveDataElements;

    public void ResetAllUniqueID()
    {
        saveDataElements = GetComponentsInChildren<SaveDataElement>().ToList();

        foreach (var element in saveDataElements.Where(element => element is not (Deco or Road)))
        {
            element.ResetUniqueID();
        }
    }
}

[CustomEditor(typeof(ResetUniqueID))]
public class ResetUniqueIDEditor : Editor
{
    private ResetUniqueID resetUniqueID;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        resetUniqueID = (ResetUniqueID)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Reset All UniqueID"))
        {
            resetUniqueID.ResetAllUniqueID();
        }
    }
}
