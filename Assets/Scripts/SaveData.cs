using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    public static SaveData Instance { get; private set; }

    private StringBuilder UserDataString = new StringBuilder();

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        StartCoroutine(LogUserData());
    }

    /*
     File structure, each line has the following info, separated by a semicolon:
     datetime (HH:mm:ss)
     sequence/progressive number
     current layer (global/local)
     gaze direction
     gaze origin
     gaze targeted object guid, if available. Otherwise, is the empty guid (Guid.Empty)
     gaze targeted object name, if the user is looking at something. If they're not, just print "none"
     head position
     head direction
     head rotation
     current manipulated object guid, if available. Otherwise, is the empty guid (Guid.Empty)
     current manipulated object name, if the user is manipulating an object that has a GameObjController attached. If they're not, just print "none"

     Example of a log file (3 lines here):
     10-01-2023_16:28:08;39;LocalLayer;(0.9, -0.2, 0.4);(0.0, 0.0, 0.0);33db2280-fa63-4dc7-865f-c00c5b15baca;blue vase_apt(0.0, 0.0, 0.0);(0.9, -0.2, 0.4);(0.1, 0.5, 0.0, 0.8);33db2280-fa63-4dc7-865f-c00c5b15baca;blue vase_apt
     10-01-2023_16:28:09;40;LocalLayer;(0.7, -0.7, 0.3);(0.0, 0.0, 0.0);00000000-0000-0000-0000-000000000000;Quad (UnityEngine.GameObject)(0.0, 0.0, 0.0);(0.7, -0.7, 0.3);(0.3, 0.5, -0.2, 0.8);00000000-0000-0000-0000-000000000000;none
     10-01-2023_16:26:10;41;GlobalLayer;(0.1, -0.4, 0.9);(0.0, 0.0, 0.0);00000000-0000-0000-0000-000000000000;none(0.0, 0.0, 0.0);(0.1, -0.5, 0.9);(0.2, 0.0, 0.0, 1.0);00000000-0000-0000-0000-000000000000;none     
    */

    private IEnumerator LogUserData()
    {
        GameObject gazeTarget;
        Guid gazeTargetGuid;
        GameObjController gazeTargetedGObjContr;

        Guid manipulatedObjectGuid;
        GameObjController manipulatedObjectGObjContr;

        string currentString = "";

        int elapsedSeconds = 0;

        while (true)
        {
            yield return new WaitForSeconds(2);

            elapsedSeconds++;

            // default values
            gazeTargetedGObjContr = null;
            gazeTarget = null;
            gazeTargetGuid = Guid.Empty;

            manipulatedObjectGuid = Guid.Empty;
            manipulatedObjectGObjContr = CaretakerScene.Instance.CurrentManipolatedGObj;

            if (CoreServices.InputSystem.GazeProvider.GazeTarget)
            {
                gazeTarget = CoreServices.InputSystem.GazeProvider.GazeTarget;
                gazeTargetedGObjContr = gazeTarget.GetComponent<GameObjController>();

                if (gazeTargetedGObjContr != null)
                    gazeTargetGuid = gazeTargetedGObjContr.Guid;
            }

            if (manipulatedObjectGObjContr != null)
                manipulatedObjectGuid = manipulatedObjectGObjContr.Guid;

            // Datetime
            UserDataString.Append(DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss") + ";");
            // Progressive number
            UserDataString.Append(elapsedSeconds + ";");
            // Current layer
            UserDataString.Append(CaretakerScene.Instance.SceneState + ";");
            // Gaze direction
            UserDataString.Append(CoreServices.InputSystem.GazeProvider.GazeDirection + ";");
            // Gaze origin
            UserDataString.Append(CoreServices.InputSystem.GazeProvider.GazeOrigin + ";");
            // Gaze targeted object guid 
            UserDataString.Append(gazeTargetGuid + ";");
            // Gaze targeted object name
            UserDataString.Append(gazeTargetedGObjContr?.ToString() ?? (gazeTarget?.ToString() ?? "none"));  // If the gObjContr is null and the target is null, print "none".
                                                                                                               // Otherwise, prints the objects' ToString()
            // Head position 
            UserDataString.Append(Camera.main.transform.position + ";");
            // Head direction
            UserDataString.Append(Camera.main.transform.forward + ";");
            // Head rotation 
            UserDataString.Append(Camera.main.transform.rotation + ";");
            // Current manipulated object guid
            UserDataString.Append(manipulatedObjectGuid + ";");
            // Current manipulated object name
            UserDataString.AppendLine(manipulatedObjectGObjContr?.ToString() ?? "none");

            ObjectsFiles.SaveData(UserDataString);
        }
    }

    public void SaveUserLogData()
    {
        ObjectsFiles.SaveData(UserDataString);
    }

    public void SaveGlobalDict()
    {
        ObjectsFiles.SaveData(CaretakerScene.Instance.GlobalListMementos);
    }
}
