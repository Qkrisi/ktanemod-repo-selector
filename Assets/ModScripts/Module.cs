using System.Collections.Generic;
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

    public Module(Dictionary<string, object> Data)
    {
        Name = (string)Data["Name"];
        ID = (string)Data["ModuleID"];
        SortKey = (string)Data["SortKey"];
        DefuserDiff = (string)Data["DefuserDifficulty"];
        ExpertDiff = (string)Data["ExpertDifficulty"];
        PublishDate = getDate((string)Data["Published"]);
    }
}