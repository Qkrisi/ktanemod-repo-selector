using System.Collections;
using UnityEngine;

public class answerButton : MonoBehaviour {

    private string text;

    qkQuestionerModule Instance { get { return transform.parent.parent.GetComponent<qkQuestionerModule>(); } }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => Instance != null);
        text = transform.Find("btn_text").GetComponent<TextMesh>().text;
        Instance.btnsForTP.Add(text.ToUpperInvariant(), new Tuple<KMSelectable, GameObject>(GetComponent<KMSelectable>(), transform.parent.gameObject));
        GetComponent<KMSelectable>().OnInteract += () => OnClicked(text);
    }

    private bool OnClicked(string t)
    {
        if (Instance._solved) return false;
        Instance.registerAns(t);
        return false;
    }
}
