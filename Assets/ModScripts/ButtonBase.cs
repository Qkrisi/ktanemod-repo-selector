using UnityEngine;
using System.Collections;
using Questioner;

public abstract class ButtonBase : ObjectBase
{
    public TPAdd addToTP = TPAdd.No;

    private IEnumerator Start()
    {
        //yield return new WaitWhile(() => Instance == null);       idk why this doesn't work for (and only) the solve button but the following while loop works
        while (Instance == null) yield return null;
        string text = getText();
        switch (addToTP)
        {
            case TPAdd.Add:
                Instance.btnsForTP.Add(text.ToUpperInvariant(), new Tuple<KMSelectable, GameObject>(GetComponent<KMSelectable>(), transform.parent.gameObject));
                break;
            case TPAdd.Disable:
                Instance.tpDisabled.Add(text, new Tuple<KMSelectable, GameObject>(GetComponent<KMSelectable>(), transform.parent.gameObject));
                break;
        }
        GetComponent<KMSelectable>().OnInteract += Click;
        GetComponent<KMSelectable>().OnInteractEnded += OnEnded;
    }

    private string getText()
    {
        var Override = GetComponent<TextOverride>();
        return Override == null ? Instance.inputText.text : Override.Text;
    }

    private bool Click()
    {
        OnClicked(getText());
        return false;
    }

    protected virtual void OnClicked(string t)
    {
        GetComponent<KMSelectable>().AddInteractionPunch(.5f);
        Instance.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
    }

    protected virtual void OnEnded() { }
}
