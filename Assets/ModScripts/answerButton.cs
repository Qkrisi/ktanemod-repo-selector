using Questioner;

public class answerButton : ButtonBase 
{
    protected override void OnClicked(string t)
    {
        base.OnClicked(t);
        if (Instance._solved || !Instance._input) return;
        if(Instance.State == ModuleState.TwitchCheck)
        {
            Instance.State = ModuleState.Main;
            return;
        }
        Instance.registerAns(t);
    }
}
