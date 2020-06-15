public class answerButton : ButtonBase 
{
    protected override void OnClicked(string t)
    {
        base.OnClicked(t);
        if (Instance._solved || !Instance._input) return;
        Instance.registerAns(t);
    }
}
