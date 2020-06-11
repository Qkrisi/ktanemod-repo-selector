using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RNG = UnityEngine.Random;

public class qkQuestionerModule : MonoBehaviour
{

    /*
        Formatters:
        {0} Order num (1st, 2nd, 3rd, etc.) from repo
        {1} Order num (1st, 2nd, 3rd, etc.) from mod selector       
        {2} Sort type (A-Z, Z-A, defuser difficulty (very easy-very hard), defuser diffivulty (vers hard-very easy), expert difficulty (very easy-very hard), expert diffivulty (vers hard-very easy), publish date (newest to oldest), publish date (oldest to newest))
        {3} Letter num (1st, 2nd, 3rd, etc.)
        {4} Random module from mod selector
        {5} Disabled num
        {6} Enabled num
        {7} Order num (1st, 2nd, 3rd, etc.) from bomb       
    */
    private string[] finalQuestions = new string[] { };

    private readonly string[] webQuestions = new[] 
    {
        "What is the {3} letter of the module that is the {0} on the repo sorted by {2}?",
    };

    private readonly string[] selectorQuestions = new[]
    {
        "What is the {3} letter of the module that is loaded {1} in game?",
        "Is {4} disabled by an enabled profile?",
        "What is the {3} letter of the module that is the {5} one in a list of all disabled modules? (A-Z)",
        "What is the {3} letter of the module that is the {6} one in a list of all enabled modules? (A-Z)"
    };

    private readonly string[] bothQuestions = new[]
    {
        "Is the {0} module on the repo sorted {2} loaded in the game?",
        "Is the {1} module loaded in game (A-Z) the same module as the {1} module on the repo sorted {2}?",
        "What is the {3} letter of the module that is the {5} one in a list of all disabled modules sorted by {2} on the repo?",
        "What is the {3} letter of the module that is the {6} one in a list of all enabled modules sorted by {2} on the repo?",
    };

    private readonly string[] neitherQuestions = new[]
    {
        "What is the {3} letter of the module that is {7} on the bomb sorted A-Z?",
        "What is the {3} letter of the module that is {7} on the bomb sorted Z-A?"
    };

    private readonly string[] orderTypes = new[]
    {
        "A-Z",
        "Z-A",
        "Defuser difficulty (very easy - very hard)",
        "Defuser difficulty (very hard - very easy)",
        "Expert difficulty (very easy - very hard)",
        "Expert difficulty (very hard - very easy)",
        "Publish date (newest to oldest)",
        "Publish date (oldest to newest)"
    };

    private readonly string[] dependsOnDisableds = new string[]
    {
        "What is the {3} letter of the module that is the {5} one in a list of all disabled modules? (A-Z)",
        "What is the {3} letter of the module that is the {5} one in a list of all disabled modules sorted by {2} on the repo?"
    };

    private questionerService Service;
    private IDictionary<string, object> API { get { try { return Service.modSelectorAPI; } catch { toggleObject("Error"); return null; } } }
    [HideInInspector]
    public bool _solved = false;
    private List<Module> fetchedModules { get { try { return Service.modulesFromWeb; } catch { toggleObject("Error"); return new List<Module>(); } } }
    private List<string> MSModules { get { return getSelectorModules(); } }
    private List<string> MSDisabledModules { get { return getDisabledModules(); } }
    private List<string> MSEnabledModules { get { return getEnabledModules(); } }

    private Dictionary<string, List<Module>> sortedModules = new Dictionary<string, List<Module>>();

    private Tuple<string, string> solvePair;

    private int moduleID;
    static int moduleIDCounter;

    [HideInInspector]
    public TextMesh displayText = null;

    private Dictionary<string, GameObject> togglableObjects = new Dictionary<string, GameObject>();
    public Dictionary<string, Tuple<KMSelectable, GameObject>> btnsForTP = new Dictionary<string, Tuple<KMSelectable, GameObject>>();

    [HideInInspector]
    public bool grantSolve = false;
    protected bool ableToSet = false;

    private int stage = 0;

    IEnumerator Start()
    {
        moduleID = ++moduleIDCounter;
        Service = FindObjectOfType<questionerService>();
        //Debug.Log("Setting dict");
        togglableObjects = new Dictionary<string, GameObject>()
        {
            { "LetteredButtons", findFromRoot("LetteredButtons")},
            { "BooleanButtons", findFromRoot("BooleanButtons")},
            { "Error", findFromRoot("Error")}
        };
        //Debug.Log("Setting to true");
        ableToSet = true;
        foreach(KeyValuePair<string, GameObject> kPair in togglableObjects)
        {
            kPair.Value.SetActive(false);
        }
        if(Service==null)
        {
            grantSolve = true;
            toggleObject("Error", true);
            yield break;
        }
        yield return new WaitUntil(() => Service._done && displayText != null);
        if(!Service.webQuestions && API==null)
        {
            finalQuestions = neitherQuestions.ToArray();
        }
        else if(!Service.webQuestions && API!=null)
        {
            finalQuestions = selectorQuestions.ToArray().Concat(neitherQuestions).ToArray();
        }
        else if(Service.webQuestions && API==null)
        {
            finalQuestions = webQuestions.ToArray().Concat(neitherQuestions).ToArray();
        }
        else
        {
            finalQuestions = webQuestions.ToArray().Concat(selectorQuestions).Concat(neitherQuestions).Concat(bothQuestions).ToArray();
        }
        sortModules();
        newStage();
    }

    private GameObject findFromRoot(string name)
    {
        return transform.Find("Objects").Find(name).gameObject;
    }

    private Tuple<string, string> getSolvePair(string[] set) //First: question, Second: answer
    {
        int index = -1;
        do { index = RNG.Range(0, set.Length); } while (dependsOnDisableds.Contains(finalQuestions[index]) && MSDisabledModules.Count == 0);
        int repoIndex = RNG.Range(1, fetchedModules.Count + 1);
        int selectorIndex = RNG.Range(1, MSModules.Count + 1);
        int disabledIndex = RNG.Range(1, MSDisabledModules.Count + 1);
        int enabledIndex = RNG.Range(1, MSEnabledModules.Count + 1);
        string sortType = orderTypes[RNG.Range(0, orderTypes.Length)];
        //Debug.LogFormat("[Questioner module #{0}] Sort type is {1}.", moduleID, sortType);
        string rndModule = API!=null ? getModuleNameByID(MSModules[RNG.Range(0, MSModules.Count)]) : "";
        var bombSort = GetComponent<KMBombInfo>().GetSolvedModuleNames().Concat(GetComponent<KMBombInfo>().GetSolvableModuleNames()).ToList();
        bombSort.Sort();
        int bombIndex = RNG.Range(1, bombSort.Count + 1);
        string activeModule = "";
           switch (set[index])
            {
                case "Is the {1} module loaded in game (A-Z) the same module as the {1} module on the repo sorted {2}?":
                    activeModule = getModuleNameByID(MSModules[selectorIndex - 1]);
                    break;
                case "Is the {0} module on the repo sorted {2} loaded in the game?":
                case "What is the {3} letter of the module that is the {0} on the repo sorted by {2}?":
                    activeModule = sortedModules[sortType][repoIndex - 1].Name;
                    break;
                case "What is the {3} letter of the module that is the {5} one in a list of all disabled modules sorted by {2} on the repo?":
                    //Debug.LogFormat("[Questioner module #{0}] Trying to get index {1} of {2}", moduleID, disabledIndex - 1, getSortageByProperty(sortType, MSDisabledModules).Count);
                    activeModule = getModuleNameByID(getSortageByProperty(sortType, MSDisabledModules)[disabledIndex - 1]);
                    break;
                case "What is the {3} letter of the module that is the {6} one in a list of all enabled modules sorted by {2} on the repo?":
                    //Debug.LogFormat("[Questioner module #{0}] Trying to get index {1} of {2}", moduleID, enabledIndex - 1, getSortageByProperty(sortType, MSEnabledModules).Count);
                    activeModule = getModuleNameByID(getSortageByProperty(sortType, MSEnabledModules)[enabledIndex - 1]);
                    break;
                case "What is the {3} letter of the module that is loaded {1} in game?":
                    activeModule = getModuleNameByID(MSModules[selectorIndex - 1]);
                    break;
                case "Is {4} disabled by an enabled profile?":
                    activeModule = getModuleNameByID(rndModule);
                    break;
                case "What is the {3} letter of the module that is the {5} one in a list of all disabled modules? (A-Z)":
                    activeModule = getModuleNameByID(MSDisabledModules[disabledIndex - 1]);
                    break;
                case "What is the {3} letter of the module that is the {6} one in a list of all enabled modules? (A-Z)":
                    activeModule = getModuleNameByID(MSEnabledModules[enabledIndex - 1]);
                    break;
                case "What is the {3} letter of the module that is {7} on the bomb sorted A-Z?":
                    activeModule = bombSort[bombIndex - 1];
                    break;
                case "What is the {3} letter of the module that is {7} on the bomb sorted Z-A?":
                    bombSort.Reverse();
                    activeModule = bombSort[bombIndex - 1];
                    break;
        }

        int letterIndex = RNG.Range(1, activeModule.Replace(" ","").Length + 1);

        string Letter = activeModule.Replace(" ","")[letterIndex - 1].ToString().ToUpperInvariant();

        string finalRepoIndex = getStringByNum(repoIndex);
        string finalSelectorIndex = getStringByNum(selectorIndex);
        string finalDisabledIndex = getStringByNum(disabledIndex);
        string finalEnabledIndex = getStringByNum(enabledIndex);
        string finalLetterIndex = getStringByNum(letterIndex);

        string Answer = "";
        switch(set[index])
        {
            case "Is the {0} module on the repo sorted {2} loaded in the game?":
                Answer = MSModules.Contains(getModuleIDByName(activeModule)) ? "Yes" : "No";
                toggleObject("BooleanButtons");
                break;
            case "Is the {1} module loaded in game (A-Z) the same module as the {1} module on the repo sorted {2}?":
                Answer = fetchedModules[repoIndex - 1].Name == activeModule ? "Yes" : "No";
                toggleObject("BooleanButtons");
                break;
            case "What is the {3} letter of the module that is the {0} on the repo sorted by {2}?":
            case "What is the {3} letter of the module that is the {5} one in a list of all disabled modules sorted by {2} on the repo?":
            case "What is the {3} letter of the module that is the {6} one in a list of all enabled modules sorted by {2} on the repo?":
            case "What is the {3} letter of the module that is loaded {1} in game?":
            case "What is the {3} letter of the module that is the {5} one in a list of all disabled modules? (A-Z)":
            case "What is the {3} letter of the module that is the {6} one in a list of all enabled modules? (A-Z)":
            case "What is the {3} letter of the module that is {7} on the bomb sorted A-Z?":
            case "What is the {3} letter of the module that is {7} on the bomb sorted Z-A?":
                Answer = Letter;
                toggleObject("LetteredButtons");
                break;
            case "Is {4} disabled by an enabled profile?":
                Answer = MSDisabledModules.Contains(getModuleIDByName(activeModule)) ? "Yes" : "No";
                toggleObject("BooleanButtons");
                break;
        }

        return new Tuple<string, string>(String.Format(set[index], finalRepoIndex, finalSelectorIndex, sortType, finalLetterIndex, getModuleNameByID(rndModule), finalDisabledIndex, finalEnabledIndex, getStringByNum(bombIndex)), Answer);
    }

    string getStringByNum(int num)
    {
        return num == 1 ? "1st" : num == 2 ? "2nd" : num==3 ? "3rd" :  num.ToString() + "th";
    }

    private List<string> getSortageByProperty(string sort, List<string> baseSet)
    {
        List<Module> sortedList = sortedModules[sort];
        List<string> ModuleIDs = new List<string>();
        foreach(Module module in sortedList)
        {
            //Debug.LogFormat("Checking module {0}", module.ID);
            if (baseSet.Contains(module.ID)) { ModuleIDs.Add(module.ID); /*Debug.Log("Correct!");*/ }
        }
        //Debug.LogFormat("Got number of sorted modules: {0} from: {1} with sorted being {2} and sortage being {3}", ModuleIDs.Count, baseSet.Count, sortedList.Count, sort);
        return ModuleIDs;
    }

    private void sortModules()
    {
        sortedModules.Clear();
        List<Module> tempList = fetchedModules;
        sortedModules.Add("A-Z", tempList);
        tempList.Reverse();
        sortedModules.Add("Z-A", tempList);
        tempList.Reverse();

        List<Module> veryEasy = new List<Module>();
        List<Module> Easy = new List<Module>();
        List<Module> Medium = new List<Module>();
        List<Module> Hard = new List<Module>();
        List<Module> veryHard = new List<Module>();
        foreach (Module module in tempList)
        {
            switch(module.DefuserDiff)
            {
                case "VeryEasy":
                    veryEasy.Add(module);
                    break;
                case "Easy":
                    Easy.Add(module);
                    break;
                case "Medium":
                    Medium.Add(module);
                    break;
                case "Hard":
                    Hard.Add(module);
                    break;
                case "VeryHard":
                    veryHard.Add(module);
                    break;
            }
        }
        sortedModules.Add("Defuser difficulty (very easy - very hard)", veryEasy.Concat(Easy).Concat(Medium).Concat(Hard).Concat(veryHard).ToList());
        veryEasy.Reverse();
        Easy.Reverse();
        Medium.Reverse();
        Hard.Reverse();
        veryHard.Reverse();
        sortedModules.Add("Defuser difficulty (very hard - very easy)", veryHard.Concat(Hard).Concat(Medium).Concat(Easy).Concat(veryEasy).ToList());

        veryEasy = new List<Module>();
        Easy = new List<Module>();
        Medium = new List<Module>();
        Hard = new List<Module>();
        veryHard = new List<Module>();
        foreach (Module module in tempList)
        {
            switch (module.ExpertDiff)
            {
                case "VeryEasy":
                    veryEasy.Add(module);
                    break;
                case "Easy":
                    Easy.Add(module);
                    break;
                case "Medium":
                    Medium.Add(module);
                    break;
                case "Hard":
                    Hard.Add(module);
                    break;
                case "VeryHard":
                    veryHard.Add(module);
                    break;
            }
        }
        sortedModules.Add("Expert difficulty (very easy - very hard)", veryEasy.Concat(Easy).Concat(Medium).Concat(Hard).Concat(veryHard).ToList());
        veryEasy.Reverse();
        Easy.Reverse();
        Medium.Reverse();
        Hard.Reverse();
        veryHard.Reverse();
        sortedModules.Add("Expert difficulty (very hard - very easy)", veryHard.Concat(Hard).Concat(Medium).Concat(Easy).Concat(veryEasy).ToList());

        tempList.Sort((a, b) => b.PublishDate.CompareTo(a.PublishDate));
        sortedModules.Add("Publish date (newest to oldest)", tempList);
        tempList = fetchedModules;
        tempList.Sort((a, b) => a.PublishDate.CompareTo(b.PublishDate));
        sortedModules.Add("Publish date (oldest to newest)", tempList);
    }

    private void newStage()
    {
        solvePair = getSolvePair(finalQuestions);
        Debug.LogFormat("[Questioner module #{0}] Question is: {1}, answer is: {2}", moduleID, solvePair.First, solvePair.Second);
        stage++;
        List<string> question = solvePair.First.Split(new char[] { ' ' }).ToList();
        string modified = "";
        int Counter = 0;
        while(question.Count>0)
        {
            modified = Counter==0 ? modified + question[0] : modified + " " + question[0];
            question.RemoveAt(0);
            Counter++;
            if(Counter==4)
            {
                modified = modified + "\n";
                Counter = 0;
            }
        }
        displayText.text = modified;
        return;
    }

    public void registerAns(string answer)
    {
        if (answer == solvePair.Second)
        {
            if (stage == 3) { GetComponent<KMBombModule>().HandlePass(); _solved = true; displayText.text = ""; return; }
            newStage();
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            stage = 0;
            newStage();
        }
        return;
    }

    private void toggleObject(string objName, bool force = false)
    {
        if (objName == "Error" && !force) return;
        togglableObjects["LetteredButtons"].SetActive(false);
        togglableObjects["BooleanButtons"].SetActive(false);
        togglableObjects[objName].SetActive(true);
        return;
    }

    private string getModuleNameByID(string ID)
    {
        foreach(var Module in fetchedModules)
        {
            if (Module.ID == ID) return Module.Name;
        }
        return ID;
    }

    private string getModuleIDByName(string Name)
    {
        foreach (var Module in fetchedModules)
        {
            if (Module.Name == Name) return Module.ID;
        }
        return Name;
    }

    private List<string> getSelectorModules()
    {
        if (API == null) return new List<string>();
        return ((IEnumerable<string>)API["AllSolvableModules"]).Concat((IEnumerable<string>)API["AllNeedyModules"]).ToList();
    }
    private List<string> getDisabledModules()
    {
        if (API == null) return new List<string>();
        return ((IEnumerable<string>)API["DisabledSolvableModules"]).Concat((IEnumerable<string>)API["DisabledNeedyModules"]).ToList();
    }
    private List<string> getEnabledModules()
    {
        if (API == null) return new List<string>();
        return getSelectorModules().Except(getDisabledModules()).ToList();
    }

#pragma warning disable 414
    [HideInInspector]
    public string TwitchHelpMessage = "Use '!{0} press <button>' to press a button (Can either be a letter (A-Z), yes, no, solve (If the error screen is present))";
#pragma warning restore 414
    public IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        if(!command.StartsWith("PRESS"))
        {
            yield return null;
            yield return "sendtochaterror Commands have to start with 'press'!";
            yield break;
        }
        command = command.Replace("PRESS ", "");
        if(!btnsForTP.ContainsKey(command) || !btnsForTP[command].Second.activeInHierarchy)
        {
            yield return null;
            yield return "sendtochaterror Looks like the button you entered is either invalid or inactive!";
            yield break;
        }
        yield return null;
        btnsForTP[command].First.OnInteract();
        yield break;
    }
}
