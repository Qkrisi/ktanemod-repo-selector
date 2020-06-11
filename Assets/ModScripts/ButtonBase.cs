using UnityEngine;
using System.Collections;

public abstract class ButtonBase : MonoBehaviour
{
    protected qkQuestionerModule Instance { get { return transform.parent.parent.parent.GetComponent<qkQuestionerModule>(); } }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => Instance != null);
        string text = getText();
        Instance.btnsForTP.Add(text.ToUpperInvariant(), new Tuple<KMSelectable, GameObject>(GetComponent<KMSelectable>(), transform.parent.gameObject));
        GetComponent<KMSelectable>().OnInteract += () => OnClicked(text);
    }

    private string getText()
    {
        var Override = GetComponent<TextOverride>();
        return Override == null ? transform.Find("btn_text").GetComponent<TextMesh>().text : Override.Text;
    }

    protected virtual bool OnClicked(string t)
    {
        GetComponent<KMSelectable>().AddInteractionPunch(.5f);
        Instance.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        return false;
    }
}
