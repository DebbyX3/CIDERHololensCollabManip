using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    public static SaveData Instance { get; private set; }

    private StringBuilder UserDataAndOperationsString = new StringBuilder();

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

        StringBuilder currentString = new StringBuilder();

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

            // Gaze targeted object name: If the gObjContr is null and the target is null, print "none".
            // Otherwise, prints the objects' ToString()
            currentString.Append(
                DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss") + ";" +                        // Datetime
                elapsedSeconds + ";" +                                                      // Progressive number
                CaretakerScene.Instance.SceneState + ";" +                                  // Current layer
                CoreServices.InputSystem.GazeProvider.GazeDirection + ";" +                 // Gaze direction
                CoreServices.InputSystem.GazeProvider.GazeOrigin + ";" +                    // Gaze origin
                gazeTargetGuid + ";" +                                                      // Gaze targeted object guid 
                gazeTargetedGObjContr?.ToString() ?? (gazeTarget?.ToString() ?? "none") +   // Gaze targeted object name
                Camera.main.transform.position + ";" +                                      // Head position 
                Camera.main.transform.forward + ";" +                                       // Head direction
                Camera.main.transform.rotation + ";" +                                      // Head rotation 
                manipulatedObjectGuid + ";" +                                               // Current manipulated object guid
                manipulatedObjectGObjContr?.ToString() ?? "none"                            // Current manipulated object name
            );
  
            UserDataAndOperationsString.AppendLine(currentString.ToString());
        }
    }

    /*
     At the beginning of each of these rows, there is a char thats marks the operation:
     Star (*)       for forced commits
     Percentage (%) for request commits
     Excl mark(!)   for request commit accepted
     Dollar ($)     for request commit rejected
     Amphersand (&) for forced deletion
     Quest mark (?) for request deletion
     Plus (+)       for request deletion accepted
     Equal (=)      for request deletion rejected

     Using method Insert()
     */
    public void LogUserOperations(  GameObjController gObjContr,
                                    CommitType commitType = CommitType.None,                                     
                                    DeletionType deletionType = DeletionType.None,
                                    AcceptType acceptType = AcceptType.None,
                                    DeclineType declineType = DeclineType.None)
    {
        StringBuilder currentString = new StringBuilder(DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss") + ";");

        // Plesae keep the order of the conditionals and the else ifs!
        if (!acceptType.Equals(AcceptType.None))
        {
            if (acceptType.Equals(AcceptType.AcceptCommit))
                currentString.Insert(0, "!");
            else if (acceptType.Equals(AcceptType.AcceptDeletion))
                currentString.Insert(0, "+");

            currentString.Append(acceptType.ToString());
        }
        else if (!commitType.Equals(CommitType.None))
        {
            if (commitType.Equals(CommitType.ForcedCommit))
                currentString.Insert(0, "*");
            else if (commitType.Equals(CommitType.RequestCommit))
                currentString.Insert(0, "%");

            currentString.Append(commitType.ToString());
        }
        else if (!deletionType.Equals(DeletionType.None))
        {
            if (deletionType.Equals(DeletionType.ForcedGlobalDeletion))
                currentString.Insert(0, "&");
            else if (deletionType.Equals(DeletionType.RequestGlobalDeletion))
                currentString.Insert(0, "?");

            currentString.Append(deletionType.ToString());
        }
        else if (!declineType.Equals(DeclineType.None))
        {
            if (declineType.Equals(DeclineType.DeclineCommit))
                currentString.Insert(0, "$");
            else if (declineType.Equals(DeclineType.DeclineDeletion))
                currentString.Insert(0, "=");

            currentString.Append(declineType.ToString());
        }

        currentString.Append(";" + gObjContr.Guid + ";" + gObjContr.ToString());

        UserDataAndOperationsString.AppendLine(currentString.ToString());
    }

    public void SaveUserLogData()
    {
        ObjectsFiles.SaveData(UserDataAndOperationsString);
    }

    public void SaveGlobalDict()
    {
        ObjectsFiles.SaveData(CaretakerScene.Instance.GlobalListMementos);
    }
}
