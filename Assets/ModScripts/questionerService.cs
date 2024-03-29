﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class questionerService : MonoBehaviour
{
    class ktaneData
    {
        public List<Module> KtaneModules { get; set; }
    }

    WWW Fetch = null;

    internal static List<Module> modulesFromWeb = new List<Module>();

    [HideInInspector]
    internal static List<string> toLog = new List<string>();

    [HideInInspector]
    internal static bool webQuestions = false;

    [HideInInspector]
    internal static bool _done = false;

    internal static IDictionary<string, object> modSelectorAPI = null;

    private bool _changed = false;

    private readonly string[] IgnoreType = new string[]
    {
        "widget",
        "holdable"
    };

    private readonly List<string> IgnoreModules = new List<string>
    {
    };
    
    private readonly Dictionary<string, string> Swap = new Dictionary<string, string>
    {
        {"+", "...?"}
    };

    IEnumerator Start()
    {
        ChangeStateChanger();
        yield return null;
        StateChange(KMGameInfo.State.Setup);
        yield break;
    }

    IEnumerator FetchModules()
    {
        _done = false;
        toLog.Clear();
        GameObject modSelectorObject = GameObject.Find("ModSelector_Info");
        if (modSelectorObject == null)
        {
            toLog.Add("ModSelector is not loaded");
        }
        else { modSelectorAPI = modSelectorObject.GetComponent<IDictionary<string, object>>(); }
        Fetch = new WWW("https://ktane.timwi.de/json/raw");
        yield return Fetch;
        if (Fetch.error == null)
        {
            modulesFromWeb = Processjson(Fetch.text);
            webQuestions = modulesFromWeb.Count > 0;
            toLog.Add("Modules successfully fetched!");
        }
        else
        {
            toLog.Add(String.Format("An error has occurred while fetching modules from the repository: {0}", Fetch.error));
            _done = true;
            yield break;
        }
        List<string> allMods = new List<string>();
        if (modSelectorAPI != null)
        {
            do { allMods = getSelectorModules(); yield return null; } while (allMods.Count == 0);
        }
        //Debug.LogFormat("allMods length: {0}", allMods.Count);
       /* List<string> toAdd = new List<string>();
        foreach(string mID in allMods)
        {
            //Debug.LogFormat("Checking module by id: {0}", mID);
            if (!doesExist(mID)) { toAdd.Add(mID); }
        }
        DateTime dateTime = DateTime.Now.Date;
        foreach (string ID in toAdd)
        {
            string sKey = !ID.ToLowerInvariant().StartsWith("the") ? ID.ToUpperInvariant() : ID.ToUpperInvariant().Substring(3);
            toLog.Add(String.Format("Found unuploaded module: {0}", ID));
            modulesFromWeb.Add(new Module
            {
                Name = ID,
                ID = ID,
                SortKey = sKey.Replace(" ", ""),
                DefuserDiff = "Easy",
                ExpertDiff = "Medium",
                PublishDate = dateTime.ToString("yyyy-MM-dd")
            });
        }*/
        modulesFromWeb = modulesFromWeb.OrderBy(x => x.SortKey).ToList();
        var SortKeys = modulesFromWeb.Select(x => x.SortKey).ToArray();
        foreach(var swapPair in Swap)
        {
            var item1 = Array.IndexOf(SortKeys, swapPair.Key);
            var item2 = Array.IndexOf(SortKeys, swapPair.Value);
            if(item1 != -1 && item2 != -1)
            {
                var original = modulesFromWeb[item1];
                modulesFromWeb[item1] = modulesFromWeb[item2];
                modulesFromWeb[item2] = original;
            }
        }
        //Debug.LogFormat("Name of the last module: {0}", modulesFromWeb[modulesFromWeb.Count-1].Name);
        _done = true;
    }

    void OnEnable()
    {
        ChangeStateChanger();
        StateChange(KMGameInfo.State.Setup);
    }
    
    void OnDisable()
    {
		_done = true;
	}

    void ChangeStateChanger()
    {
        if (_changed) return;
        GetComponent<KMGameInfo>().OnStateChange += StateChange;
        _changed = true;
    }

    void StateChange(KMGameInfo.State state)
    {
        if(state==KMGameInfo.State.Setup)
        {
            StopAllCoroutines();
            StartCoroutine(FetchModules());
        }
    }

    List<Module> Processjson(string fetched)
    {
        ktaneData Deserialized = JsonConvert.DeserializeObject<ktaneData>(fetched);
        List<Module> Modules = new List<Module>();
        Debug.LogFormat("Fetched module count #1: {0}", Deserialized.KtaneModules.Count);
        foreach(var item in Deserialized.KtaneModules)
        {
            var id = item.ModuleID;
            var ignore = IgnoreModules.Contains(id);
            if (!ignore && !IgnoreType.Contains(item.Type) && string.IsNullOrEmpty(item.TranslationOf) && item.Origin!="Vanilla") Modules.Add(item);
            else if(!ignore) IgnoreModules.Add(id);
        }
        Debug.LogFormat("Fetched module count #2: {0}", Modules.Count);
        return Modules;
    }

    private List<string> getSelectorModules()
    {
        return ((IEnumerable<string>)modSelectorAPI["AllSolvableModules"]).Concat((IEnumerable<string>)modSelectorAPI["AllNeedyModules"]).ToList();
    }

    private bool doesExist(string id)
    {
        return IgnoreModules.Contains(id) || modulesFromWeb.Any(m => m.ModuleID == id);
    }
}

