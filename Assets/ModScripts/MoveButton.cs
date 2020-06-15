using Questioner;

public class MoveButton : ButtonBase
{
    public Move move;

    protected override qkQuestionerModule Instance
    {
        get
        {
            return transform.parent.parent.GetComponent<qkQuestionerModule>();
        }
    }

    protected override void OnClicked(string t)
    {
        base.OnClicked(t);
        if (Instance._solved || !Instance._input) return;
        Instance.StartMove(move);
    }
}