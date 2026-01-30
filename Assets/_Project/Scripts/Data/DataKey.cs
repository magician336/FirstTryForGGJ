using UnityEngine;

[CreateAssetMenu(menuName = "Game/Data/Data Key", fileName = "DataKey_")]
public class DataKey : ScriptableObject
{
    [SerializeField] private string id = "";
    [TextArea] [SerializeField] private string description = "";

    public string Id => id;
    public string Description => description;
}
