using System.Collections;
using UnityEngine;

public class solveButton : MonoBehaviour
{
    qkQuestionerModule Instance { get { return transform.parent.parent.GetComponent<qkQuestionerModule>(); } }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => Instance != null);
        Instance.btnsForTP.Add("SOLVE", new Tuple<KMSelectable, GameObject>(GetComponent<KMSelectable>(), transform.parent.gameObject));
        GetComponent<KMSelectable>().OnInteract += OnClick;
    }

    private bool OnClick()
    {
        if (!Instance.grantSolve) return false;
        Instance._solved = true;
        Instance.GetComponent<KMBombModule>().HandlePass();
        return false;
    }
}
