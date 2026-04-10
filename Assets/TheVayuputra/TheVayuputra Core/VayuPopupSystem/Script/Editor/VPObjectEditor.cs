using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace TheVayuputra.Core
{

    [CustomEditor(typeof(VPObject), true)]
    [CanEditMultipleObjects]
    public class VPObjectEditor : Editor
    {
        VayuPopup parentPopup;
        public override void OnInspectorGUI()
        {
            VPObject targetLPObject = (VPObject)target;
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set To Open"))
                {
                    List<Object> all = new List<Object>();
                    foreach (var item in targetLPObject.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), targetLPObject.name + " Popup Open");
                    targetLPObject.SetToOpen();

                }
                if (GUILayout.Button("Set To Close"))
                {
                    List<Object> all = new List<Object>();
                    foreach (var item in targetLPObject.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), targetLPObject.name + " Popup Close");
                    targetLPObject.SetToClose();
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            DrawDefaultInspector();
            GUILayout.Space(10);
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Current As Open"))
                {
                    List<Object> all = new List<Object>();
                    foreach (var item in targetLPObject.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), targetLPObject.name + " Popup Open");
                    targetLPObject.SetCurrentAsOpen();

                }
                if (GUILayout.Button("Set Current As Close"))
                {
                    List<Object> all = new List<Object>();
                    foreach (var item in targetLPObject.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), targetLPObject.name + " Popup Close");
                    targetLPObject.SetCurrentAsClose();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

}