using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace TheVayuputra.Core
{
    public static class CreateVayuPopupCommand
    {
        [MenuItem("Component/UI/VayuPopup/Create Vayu Move Popup", true)]
        [MenuItem("Component/UI/VayuPopup/Create Vayu Scale Popup", true)]
        [MenuItem("Component/UI/VayuPopup/Create Vayu CanavasGroup Popup", true)]
        public static bool CreateMovePopupValidation()
        {
            return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
        }
        [MenuItem("Component/UI/VayuPopup/Create Vayu Move Popup")]
        public static void CreateMovePopup()
        {
            AddLTObject<VPMoveObject>();
        }
        [MenuItem("Component/UI/VayuPopup/Create Vayu Scale Popup")]
        public static void CreateScalePopup()
        {
            AddLTObject<VPScaleObject>();
        }
        [MenuItem("Component/UI/VayuPopup/Create Vayu CanavasGroup Popup")]
        public static void CreateCanvasGroupPopup()
        {
            AddLTObject<VPCanvasGroupObject>();
        }
        public static void AddLTObject<T>() where T : VPObject
        {
            var selectedGameObject = Selection.gameObjects;
            foreach (var go in selectedGameObject)
            {

                var popup = go.GetComponent<VayuPopup>();
                if (popup == null)
                {
                    popup = Undo.AddComponent<VayuPopup>(go);
                }
                var ltObject = go.GetComponent<T>();
                if (ltObject == null)
                {
                    ltObject = Undo.AddComponent<T>(go);
                }
                if (popup.lPObjects == null)
                {
                    popup.lPObjects = new List<VPObject>();
                }
                if (!popup.lPObjects.Contains(ltObject))
                {
                    List<Object> all = new List<Object>();
                    foreach (var item in go.GetComponents<Component>())
                    {
                        all.Add(item);
                    }
                    Undo.RecordObjects(all.ToArray(), go.name + " VPObject Create");
                    popup.lPObjects.Add(ltObject);
                }
            }
        }
    }
}