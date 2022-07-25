using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MementoProva : MonoBehaviour
{

    private string _state;
    private DateTime _date;

    public MementoProva(string state) {
        this._state = state;
        this._date = DateTime.Now;
    }

    // The Originator uses this method when restoring its state.
    public string GetState() {
        return this._state;
    }

    // The rest of the methods are used by the Caretaker to display
    // metadata.
    public string GetName() {
        return $"{this._date} / ({this._state.Substring(0, 9)})...";
    }

    public DateTime GetDate() {
        return this._date;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
