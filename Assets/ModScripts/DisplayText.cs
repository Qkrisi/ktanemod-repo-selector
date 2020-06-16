using UnityEngine;
using System.Reflection;
using Questioner;

public class DisplayText : ObjectBase 
{
    public DisplayType dType;

    void Start()
    {
        typeof(qkQuestionerModule).GetField(string.Format("{0}Text", dType.ToString().ToLowerInvariant()), BindingFlags.Public | BindingFlags.Instance).SetValue(Instance, GetComponent<TextMesh>());
    }
}
