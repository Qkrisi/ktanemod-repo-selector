using UnityEngine;

public class DisplayText : ObjectBase 
{
    void Start()
    {
        Instance.displayText = GetComponent<TextMesh>();
    }
}
