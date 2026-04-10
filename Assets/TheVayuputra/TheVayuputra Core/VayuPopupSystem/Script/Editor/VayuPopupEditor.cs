using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace TheVayuputra.Core
{
    [CustomEditor(typeof(VayuPopup), true)]
    public class VayuPopupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            VayuPopup targetPopup = (VayuPopup)target;
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open"))
            {
                targetPopup.lPObjects.RemoveAll(x => x == null);
                if (EditorApplication.isPlaying)
                    targetPopup.Open();
                else
                {
                    List<Object> all = new List<Object>();
                    foreach (var item in targetPopup.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), targetPopup.name + " Popup Open");
                    targetPopup.SetToOpen();
                }
            }
            if (GUILayout.Button("Close"))
            {
                targetPopup.lPObjects.RemoveAll(x => x == null);
                if (EditorApplication.isPlaying)
                    targetPopup.Close();
                else
                {
                    List<Object> all = new List<Object>();
                    foreach (var item in targetPopup.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), targetPopup.name + " Popup Close");
                    targetPopup.SetToClose();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            DrawDefaultInspector();


            if (!EditorApplication.isPlaying)
            {
                GUILayout.Space(10);
                if (GUILayout.Button("Add All Child LPObject To List"))
                {
                    if (targetPopup.lPObjects == null)
                        targetPopup.lPObjects = new List<VPObject>();
                    targetPopup.lPObjects.RemoveAll(x => x == null);
                    var temp = targetPopup.GetComponentsInChildren<VPObject>(true);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (!targetPopup.lPObjects.Contains(temp[i]))
                        {
                            targetPopup.lPObjects.Add(temp[i]);
                        }
                    }

                    List<Object> all = new List<Object>();
                    foreach (var item in targetPopup.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), targetPopup.name + " Popup Close");
                    targetPopup.SetToClose();

                }
            }


        }
    }
}