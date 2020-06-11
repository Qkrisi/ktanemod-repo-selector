using UnityEngine;

public class DisplayText : MonoBehaviour {
    qkQuestionerModule Instance { get { return transform.parent.parent.parent.GetComponent<qkQuestionerModule>(); } }

    void Start()
    {
        Instance.displayText = GetComponent<TextMesh>();
    }
}
