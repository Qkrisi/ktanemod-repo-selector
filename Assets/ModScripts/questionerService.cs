using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class questionerService : MonoBehaviour
{
    class ktaneData
    {
        public List<Dictionary<string, object>> KtaneModules { get; set; }
    }

    WWW Fetch = null;

    public List<Module> modulesFromWeb = new List<Module>();

    [HideInInspector]
    public List<string> toLog = new List<string>();

    [HideInInspector]
    public bool webQuestions = false;

    [HideInInspector]
    public bool _done = false;

    public IDictionary<string, object> modSelectorAPI = null;

    private bool _changed = false;

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
            webQuestions = true;
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
        List<string> toAdd = new List<string>();
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
            modulesFromWeb.Add(new Module(
                new Dictionary<string, object>()
                {
                    {"Name", ID},
                    {"ModuleID", ID},
                    {"SortKey", sKey.Replace(" ","")},
                    {"DefuserDifficulty", "Easy"},
                    {"ExpertDifficulty", "Medium"},
                    {"Published", dateTime.ToString("yyyy-MM-dd")}
                }
                ));
        }
        modulesFromWeb = modulesFromWeb.OrderBy(x => x.SortKey).ToList();
        //Debug.LogFormat("Name of the last module: {0}", modulesFromWeb[modulesFromWeb.Count-1].Name);
        _done = true;
    }

    void OnEnable()
    {
        ChangeStateChanger();
        StateChange(KMGameInfo.State.Setup);
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
        foreach(var item in Deserialized.KtaneModules)
        {
            if ((string)item["Type"] != "Widget" && (string)item["Origin"]!="Vanilla") Modules.Add(new Module(item));
        }
        return Modules;
    }

    private List<string> getSelectorModules()
    {
        return ((IEnumerable<string>)modSelectorAPI["AllSolvableModules"]).Concat((IEnumerable<string>)modSelectorAPI["AllNeedyModules"]).ToList();
    }

    private bool doesExist(string id)
    {
        foreach(Module module in modulesFromWeb)
        {
            if (module.ID == id) return true;
        }
        return false;
    }
}

