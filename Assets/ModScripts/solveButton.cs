public class solveButton : ButtonBase
{
    protected override bool OnClicked(string t)
    {
        base.OnClicked(t);
        if (!Instance.grantSolve) return false;
        Instance._solved = true;
        Instance.GetComponent<KMBombModule>().HandlePass();
        return false;
    }
}
