using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

public class Module
{
    public string Name { get; set; }
    public string ModuleID { get; set; }
    public string SortKey { get; set; }
    public string DefuserDifficulty { get; set; }
    public string ExpertDifficulty { get; set; }
    [JsonIgnore] public DateTime DeserializedPublishDate { get; set; }
    public string Published { get; set; }
    [JsonIgnore] public string[] Authors { get; set; }
    public string Author { get; set; }
    public Dictionary<string, string[]> Contributors { get; set; }
    public string SteamID { get; set; }
    public string Type { get; set; }
    public Dictionary<string, string>[] TutorialVideos { get; set; }
    public Dictionary<string, string> Souvenir { get; set; }
    public string SourceURL { get; set; }

    public string Origin;
    public string TranslationOf;

    private DateTime getDate(string source)
    {
        string[] splitted = source.Split(new char[] { '-' });
        return new DateTime(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]));
    }

    private string GetSortKey()
    {
        return Regex.Replace(Regex.Replace(Name.ToUpperInvariant(), @"^THE ", ""), @"[^A-Z0-9]", "");
    }

    [OnDeserialized]
    public void OnDeserialized(StreamingContext _)
    {
        DeserializedPublishDate = getDate(Published);
        Authors = !string.IsNullOrEmpty(Author)
            ? Author.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
            : new string[0];
        if (Contributors == null)
            Contributors = new Dictionary<string, string[]>();
        if(TutorialVideos == null)
            TutorialVideos = new Dictionary<string, string>[0];
        Type = Type.ToLowerInvariant();
        if(Souvenir == null)
            Souvenir = new Dictionary<string, string>()
            {
                { "Status", "Unexamined" }
            };
        if (Souvenir["Status"] == "NotACandidate")
            Souvenir["Status"] = "Not a candidate";
        if (string.IsNullOrEmpty(SortKey))
            SortKey = GetSortKey();
        Type = Type.ToLowerInvariant();
    }

    /*public Module(Dictionary<string, object> Data)
    {
        Name = (string)Data["Name"];
        ID = (string)Data["ModuleID"];
        SortKey = Data.ContainsKey("SortKey") ? (string)Data["SortKey"] : GetSortKey();
        DefuserDiff = (string)Data["DefuserDifficulty"];
        ExpertDiff = (string)Data["ExpertDifficulty"];
        PublishDate = getDate((string)Data["Published"]);
        if (Data.ContainsKey("Author"))
            Authors = ((string)Data["Author"]).Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        else Authors = new string[0];
        if (Data.ContainsKey("Contributors"))
            Contributors = (Dictionary<string, string[]>)Data["Contributors"];
        else Contributors = new Dictionary<string, string[]>();
        SteamID = (string)Data["SteamID"];
        Type = ((string)Data["Type"]).ToLowerInvariant();
        if (Data.ContainsKey("TutorialVideos"))
            TutorialVideos = (Dictionary<string, string>[])Data["TutorialVideos"];
        if (Data.ContainsKey("Souvenir"))
            Souvenir = (Dictionary<string, string>)Data["Souvenir"];
        else
            Souvenir = new Dictionary<string, string>()
            {
                { "Status", "Unexamined" }
            };
        if (Souvenir["Status"] == "NotACandidate")
            Souvenir["Status"] = "Not a candidate";
        if (Data.ContainsKey("SourceURL"))
            SourceURL = (string)Data["SourceURL"];
    }*/
}
