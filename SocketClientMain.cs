using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft;
using UnityEngine.UI;

public class SocketClientMain : SocketClient {
    // parte di connessione
    public bool connettiti = false;
    public float[] array = new float[4];
    public bool connectionSucceeded = false;
    public GameObject cube;

    //float[] righthand= new float[76];

    public float[] hand_right = new float[60];
    //float[] lefthand = new float[76];

    //parte di tracciamento delle dita
    // creo dei vettori 3D sia per la mano dx che sx
    // 5 per mano (R-L) per ogni giunto (tips, distal, ...)
    Vector3[] tipsR = new Vector3[5];
    Vector3[] tipsL = new Vector3[5];

    Vector3[] distalR = new Vector3[5];
    Vector3[] distalL = new Vector3[5];

    // 4 perché il pollice non ha il giunto "middle"
    Vector3[] middleR = new Vector3[4];
    Vector3[] middleL = new Vector3[4];

    // 4 perché il pollice non ha il giunto "knuckle"
    Vector3[] knuckleR = new Vector3[4];
    Vector3[] knuckleL = new Vector3[4];

    Vector3 proximalR = Vector3.zero;   // = Vector3(0, 0, 0)
    Vector3 proximalL = Vector3.zero;   // = Vector3(0, 0, 0)

    Vector3[] metacarpalR = new Vector3[5];
    Vector3[] metacarpalL = new Vector3[5];

    Vector3 wristR = Vector3.zero;	// = Vector3(0, 0, 0)
    Vector3 wristL = Vector3.zero;	// = Vector3(0, 0, 0)

    // istanzio una lista di oggetti per la mano dx e sx
    List<GameObject> tipsObjectsR = new List<GameObject>();
    List<GameObject> tipsObjectsL = new List<GameObject>();

    List<GameObject> distalObjectsR = new List<GameObject>();
    List<GameObject> distalObjectsL = new List<GameObject>();

    List<GameObject> middleObjectsR = new List<GameObject>();
    List<GameObject> middleObjectsL = new List<GameObject>();

    List<GameObject> knuckleObjectsR = new List<GameObject>();
    List<GameObject> knuckleObjectsL = new List<GameObject>();

    // istanzio due oggetti, uno per il pollice dx e uno per il pollice sx
    GameObject proximalObjectR;
    GameObject proximalObjectL;

    List<GameObject> metacarpalObjectsR = new List<GameObject>();
    List<GameObject> metacarpalObjectsL = new List<GameObject>();

    // istanzio due oggetti, uno per il polso dx e uno per il polso sx
    GameObject wristObjectR;
    GameObject wristObjectL;

    // posizione e rotazione possono essere richiesti come MixedRealityPose
    MixedRealityPose jointPose;

    //array con i nomi delle gestures
    public string[] Gestures_names = new string[] { "ONE", "TWO", "THREE", "FOUR", "OK", "MENU", "POINTING", "LEFT", "RIGHT", "CIRCLE", "V", "CROSS", "GRAB", "EXPAND", "PINCH", "TAP", "DENY", "KNOB", "nongesture" };

    public Text Gesture_attuale_name;

    void Start() {
        //apro connessione con py
        Debug.Log("Eseguo conn a py " + gameObject.name);
        connectionSucceeded = ConnectToServer();

        if(connectionSucceeded)
            cube.GetComponent<Renderer>().material.color = new Color(0, 204, 102);

        Gesture_attuale_name.text = Gestures_names[18];
    }

    // Update is called once per frame
    void Update() {
        //debugger per la connessione
        if (connettiti == true) {
            Debug.Log("Python pronto?!?" + gameObject.name);
            //float[] prediction = ServerRequest(array);
            // prova.StartClient();
            connettiti = false;
            Debug.Log("fine" + gameObject.name);
            Gesture_attuale_name.text = Gestures_names[18];
        }
        //ogni tot chiamo server request ma deve essere courotine probabilmente e non un flusso di dati
        //StartCoroutine(SendJointPosToPython());
        if (connectionSucceeded) {
            StartCoroutine(SendJointPosToPython());
        }/*
        array[0] = 0f;
        for (int i = 0; i < 3; i++)
        {
            array[i+1] = tipsR[0][i];
        }
            

        float[] prediction = SendToPython(array);*/
    }

    IEnumerator SendJointPosToPython() {
        //ottieni posizioni se mano visibile
        if (ManoDestraVisibile()) {
            Debug.Log("Ottenute posizioni joint dx" + gameObject.name);
            //manda a pyth

            float[] prediction = SendToPython(hand_right);
            int a = (int)prediction[0];
            Gesture_attuale_name.text = Gestures_names[a];
            Debug.Log("stampo" + Gestures_names[a]);

            Debug.Log("inviati i dati a py" + gameObject.name);
        }
        yield return null;
    }

    private bool ManoDestraVisibile() {
        // ================== MANO DESTRA ==================

        // ===================== tips =====================

        // cerca di ottenere la posa del giunto richiesto 
        // TrackedHandJoint.ThumbTip è il giunto richiesto (punta del pollice)
        // Handedness.Right è la mano specifica di interesse
        // out nointPose sono i dati di posa di output
        // ritorna un booleano
        int j = 0;
        Vector3 pos = new Vector3();
        //se vedo palmo vedo tutto al momento
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        //pollice

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbMetacarpalJoint, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;


        //indice
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMetacarpal, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;


        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMiddleJoint, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        // medio
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMetacarpal, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;


        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        // anualre
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingKnuckle, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMetacarpal, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;


        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMiddleJoint, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        //mignolo
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMetacarpal, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;


        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMiddleJoint, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Right, out jointPose)) {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            pos = jointPose.Position;
            for (int i = 0; i < 3; i++) {
                hand_right[j] = pos[i];
                j++;
            }
        } else
            return false;

        return true;
    }

    /*
    private bool ManoDestraVisibile2()
    {
        // ================== MANO DESTRA ==================

        // ===================== tips =====================

        // cerca di ottenere la posa del giunto richiesto 
        // TrackedHandJoint.ThumbTip è il giunto richiesto (punta del pollice)
        // Handedness.Right è la mano specifica di interesse
        // out nointPose sono i dati di posa di output
        // ritorna un booleano
        int j = 0;
        int k = 0;




        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out jointPose))
        {
            // assegno la posizione del giunto al vettore 3D creato all'inizio
            tipsR[0] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = tipsR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // punta dell'indice
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out jointPose))
        {
            tipsR[1] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = tipsR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // punta del medio
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Right, out jointPose))
        {
            tipsR[2] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = tipsR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // punta dell'anulare
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Right, out jointPose))
        {
            tipsR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = tipsR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // punta del mignolo
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Right, out jointPose))
        {
            tipsR[4] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = tipsR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // =============== distal ====================
        k = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbDistalJoint, Handedness.Right, out jointPose))
        {
            distalR[0] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexDistalJoint, Handedness.Right, out jointPose))
        {
            distalR[1] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleDistalJoint, Handedness.Right, out jointPose))
        {
            distalR[2] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingDistalJoint, Handedness.Right, out jointPose))
        {
            distalR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyDistalJoint, Handedness.Right, out jointPose))
        {
            distalR[4] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // =============== middle ====================
        k = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMiddleJoint, Handedness.Right, out jointPose))
        {
            middleR[0] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, Handedness.Right, out jointPose))
        {
            middleR[1] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMiddleJoint, Handedness.Right, out jointPose))
        {
            middleR[2] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMiddleJoint, Handedness.Right, out jointPose))
        {
            middleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = distalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // =============== knuckle ====================
        k = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, Handedness.Right, out jointPose))
        {
            knuckleR[0] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = knuckleR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Right, out jointPose))
        {
            knuckleR[1] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = knuckleR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingKnuckle, Handedness.Right, out jointPose))
        {
            knuckleR[2] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = knuckleR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, Handedness.Right, out jointPose))
        {
            knuckleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = knuckleR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // ===================== proximal =====================
        k = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, Handedness.Right, out jointPose))
        {
            proximalR = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = proximalR[i];
                j++;
            }
        }
        else
            return false;
        // =============== metacarpal ====================
        k = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbMetacarpalJoint, Handedness.Right, out jointPose))
        {
            metacarpalR[0] = jointPose.Position;
            knuckleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = metacarpalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMetacarpal, Handedness.Right, out jointPose))
        {
            metacarpalR[1] = jointPose.Position;
            knuckleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = metacarpalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMetacarpal, Handedness.Right, out jointPose))
        {
            metacarpalR[2] = jointPose.Position;
            knuckleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = metacarpalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMetacarpal, Handedness.Right, out jointPose))
        {
            metacarpalR[3] = jointPose.Position;
            knuckleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = metacarpalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMetacarpal, Handedness.Right, out jointPose))
        {
            metacarpalR[4] = jointPose.Position;
            knuckleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = metacarpalR[k][i];
                j++;
            }
            k++;
        }
        else
            return false;
        // ===================== polso =====================
        k = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Right, out jointPose))
        {
            wristR = jointPose.Position;
            knuckleR[3] = jointPose.Position;
            for (int i = 0; i < 3; i++)
            {
                righthand[j] = wristR[i];
                j++;
            }
            k++;
        }
        else
            return false;
        //se passo tutti i check mando i dati
        return true;
    }

    */



    private bool ManoSinistraVisibile() {
        // ================== MANO SINISTRA ==================

        // ===================== tips =====================

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Left, out jointPose)) {
            tipsL[0] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out jointPose)) {
            tipsL[1] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, Handedness.Left, out jointPose)) {
            tipsL[2] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, Handedness.Left, out jointPose)) {
            tipsL[3] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, Handedness.Left, out jointPose)) {
            tipsL[4] = jointPose.Position;
        } else
            return false;
        // =============== distal ====================

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbDistalJoint, Handedness.Left, out jointPose)) {
            distalL[0] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexDistalJoint, Handedness.Left, out jointPose)) {
            distalL[1] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleDistalJoint, Handedness.Left, out jointPose)) {
            distalL[2] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingDistalJoint, Handedness.Left, out jointPose)) {
            distalL[3] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyDistalJoint, Handedness.Left, out jointPose)) {
            distalL[4] = jointPose.Position;
        } else
            return false;
        // =============== middle ====================

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMiddleJoint, Handedness.Left, out jointPose)) {
            middleL[0] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, Handedness.Left, out jointPose)) {
            middleL[1] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMiddleJoint, Handedness.Left, out jointPose)) {
            middleL[2] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMiddleJoint, Handedness.Left, out jointPose)) {
            middleL[3] = jointPose.Position;
        } else
            return false;
        // =============== knuckle ====================

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, Handedness.Left, out jointPose)) {
            knuckleL[0] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, Handedness.Left, out jointPose)) {
            knuckleL[1] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingKnuckle, Handedness.Left, out jointPose)) {
            knuckleL[2] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, Handedness.Left, out jointPose)) {
            knuckleL[3] = jointPose.Position;
        } else
            return false;
        // ===================== proximal =====================

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, Handedness.Left, out jointPose)) {
            proximalL = jointPose.Position;
        } else
            return false;
        // =============== metacarpal ====================

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbMetacarpalJoint, Handedness.Left, out jointPose)) {
            metacarpalL[0] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMetacarpal, Handedness.Left, out jointPose)) {
            metacarpalL[1] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMetacarpal, Handedness.Left, out jointPose)) {
            metacarpalL[2] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMetacarpal, Handedness.Left, out jointPose)) {
            metacarpalL[3] = jointPose.Position;
        } else
            return false;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMetacarpal, Handedness.Left, out jointPose)) {
            metacarpalL[4] = jointPose.Position;
        } else
            return false;
        // ===================== polso =====================

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Left, out jointPose)) {
            wristL = jointPose.Position;
            wristObjectL.GetComponent<Renderer>().enabled = true;
        } else
            return false;

        //se passo tutti i check mando i dati
        return true;

    }

}
