using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class Module
{
    public string Name { get; private set; }
    public string ID { get; private set; }
    public string SortKey { get; private set; }
    public string DefuserDiff { get; private set; }
    public string ExpertDiff { get; private set; }
    public DateTime PublishDate { get; private set; }

    private DateTime getDate(string source)
    {
        string[] splitted = source.Split(new char[] { '-' });
        return new DateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]));
    }

    private string GetSortKey()
    {
        return Regex.Replace(Regex.Replace(Name.ToUpperInvariant(), @"^THE ", ""), @"[^A-Z0-9]", "");
    }

    public Module(Dictionary<string, object> Data)
    {
        Name = (string)Data["Name"];
        ID = (string)Data["ModuleID"];
        SortKey = Data.ContainsKey("SortKey") ? (string)Data["SortKey"] : GetSortKey();
        DefuserDiff = (string)Data["DefuserDifficulty"];
        ExpertDiff = (string)Data["ExpertDifficulty"];
        PublishDate = getDate((string)Data["Published"]);
    }
}
