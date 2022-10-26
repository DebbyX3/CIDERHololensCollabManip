using UnityEngine;
using UnityEngine.UI;

public interface ISlateManager
{
    public void PopulateSlate(string prefabName);
    public void AddButton(string prefabName, Texture2D image);
}
