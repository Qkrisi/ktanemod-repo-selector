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
    public bool webQuestions = false;

    [HideInInspector]
    public bool _done = false;

    public IDictionary<string, object> modSelectorAPI = null;

    IEnumerator Start()
    {
        GetComponent<KMGameInfo>().OnStateChange += StateChange;
        yield return null;
        StartCoroutine(FetchModules());
        yield break;
    }

    IEnumerator FetchModules()
    {
        _done = false;
        GameObject modSelectorObject = GameObject.Find("ModSelector_Info");
        if (modSelectorObject == null)
        {
            Debug.Log("ModSelector is not loaded");
        }
        else { modSelectorAPI = modSelectorObject.GetComponent<IDictionary<string, object>>(); }
        Fetch = new WWW("https://ktane.timwi.de/json/raw");
        yield return Fetch;
        if (Fetch.error == null)
        {
            modulesFromWeb = Processjson(Fetch.text);
            webQuestions = true;
            Debug.Log("Modules successfully fetched!");
        }
        else
        {
            Debug.LogFormat("An error has occurred while fetching modules from the repository: {0}", Fetch.error);
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
        DateTime dateTime = DateTime.UtcNow.Date;
        foreach (string ID in toAdd)
        {
            Debug.LogFormat("Found unuploaded module: {0}", ID);
            modulesFromWeb.Add(new Module(
                new Dictionary<string, object>()
                {
                    {"Name", ID},
                    {"ModuleID", ID},
                    {"DefuserDifficulty", "Easy"},
                    {"ExpertDifficulty", "Medium"},
                    {"Published", dateTime.ToString("yyyy-MM-dd")}
                }
                ));
        }
        modulesFromWeb = modulesFromWeb.OrderBy(x => x.Name).ToList();
        //Debug.LogFormat("Name of the last module: {0}", modulesFromWeb[modulesFromWeb.Count-1].Name);
        _done = true;
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
            if ((string)item["Type"] != "Widget" && (string)item[") Modules.Add(new Module(item));
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

