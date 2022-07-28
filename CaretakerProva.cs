using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// mi posso permettere di usare un dizionario e non una lista perchè non mi server uno storico dell'oggetto,
//visto che la 'storia' sarebbe 
//forse non serve tutta la classe static? boh
public static class CaretakerProva
{
    //keeps mementos of global scene
    private static Dictionary<Guid, Memento> globalListMementos { get; } = new Dictionary<Guid, Memento>();

    //keeps mementos of local scene
    private static Dictionary<Guid, Memento> localListMementos { get; } = new Dictionary<Guid, Memento>();

    // o forse solo un metodo dove passo l'enum globale/locale? boh
    public static void SaveGlobalState(GameObjController gObj) {
        //salva
        if (globalListMementos.ContainsKey(gObj.Guid)) {
            globalListMementos[gObj.Guid] = gObj.Save();
        } 
        else {
            globalListMementos.Add(gObj.Guid, gObj.Save());
        }
    }

    public static void SaveLocalState(GameObjController gObj) {
        //salva
    }


    /*
    public void Backup() {
        Debug.Log("\nCaretaker: Saving Originator (GObj)'s state...");
        this.globalListMementos.Add(this.Originator.Save());
    }

    public void Undo() {
        if (this.globalListMementos.Count == 0) {
            return;
        }

        var memento = globalListMementos[globalListMementos.Count-1]; // get last element!
        
        // ma non lo devo rimuovere! mi sa che devo ripensare al salvataggio mhm
        //this.mementos.Remove(memento);

        Debug.Log("Caretaker: Restoring state to: " + memento.GetName());

        try {
            this.Originator.Restore(memento);
        } catch (Exception) {
            this.Undo();
        }
    }

    public void ShowHistory() {
        Console.WriteLine("Caretaker: Here's the list of mementos:");

        foreach (var memento in this.globalListMementos) {
            Debug.Log(memento.GetName());
        }
    }*/
}
