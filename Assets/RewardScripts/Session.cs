using UnityEngine;

[CreateAssetMenu(fileName = "Session", menuName = "ScriptableObject/Session")]
public class Session : ScriptableObject {
    public string[] ids;
    public int clientNum;
}