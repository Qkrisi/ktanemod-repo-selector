public class answerButton : ButtonBase 
{
    protected override bool OnClicked(string t)
    {
        base.OnClicked(t);
        if (Instance._solved) return false;
        Instance.registerAns(t);
        return false;
    }
}
