using UnityEngine;

public abstract class ObjectBase : MonoBehaviour
{
    protected virtual qkQuestionerModule Instance { get { return transform.parent.parent.parent.GetComponent<qkQuestionerModule>(); } }
}