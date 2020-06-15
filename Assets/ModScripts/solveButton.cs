public class solveButton : ButtonBase
{
    protected override void OnClicked(string t)
    {
        base.OnClicked(t);
        if (!Instance.grantSolve) return;
        Instance._solved = true;
        Instance.GetComponent<KMBombModule>().HandlePass();
    }
}
