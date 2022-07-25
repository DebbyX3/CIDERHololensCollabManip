using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Index;
using System.Range;

public class CaretakerProva : MonoBehaviour
{
    private List<MementoProva> _mementos = new List<MementoProva>();

    private GameObjController _originator = null;

    public CaretakerProva(GameObjController originator) {
        this._originator = originator;
    }

    public void Backup() {
        Debug.Log("\nCaretaker: Saving Originator (GObj)'s state...");
        this._mementos.Add(this._originator.Save());
    }

    public void Undo() {
        if (this._mementos.Count == 0) {
            return;
        }

        var memento = _mementos[_mementos.Count-1]; // get last element!
        this._mementos.Remove(memento);

        Console.WriteLine("Caretaker: Restoring state to: " + memento.GetName());

        try {
            this._originator.Restore(memento);
        } catch (Exception) {
            this.Undo();
        }
    }

    public void ShowHistory() {
        Console.WriteLine("Caretaker: Here's the list of mementos:");

        foreach (var memento in this._mementos) {
            Console.WriteLine(memento.GetName());
        }
    }
    
}
