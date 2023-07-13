using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Questioner;
using RNG = UnityEngine.Random;

public class qkQuestionerModule : MonoBehaviour
{

    /*
        Formatters:
        {0} Order num (1st, 2nd, 3rd, etc.) from repo
        {1} Order num (1st, 2nd, 3rd, etc.) from mod selector       
        {2} Sort type (A-Z, Z-A, defuser difficulty (very easy-very hard), defuser diffivulty (vers hard-very easy), expert difficulty (very easy-very hard), expert diffivulty (vers hard-very easy), publish date (newest to oldest), publish date (oldest to newest))
        {3} letter/digit num (1st, 2nd, 3rd, etc.)
        {4} Random module from mod selector
        {5} Disabled num
        {6} Enabled num
        {7} Order num (1st, 2nd, 3rd, etc.) from bomb    
        {8} Random module from the repo
        {9} Module type
        {10} Release date
    */
    private string[] finalQuestions = new string[] { };

    private readonly string[] webQuestions = new string[]
    {
        "What is the {3} letter/digit of the module that is the {0} on the repo sorted by {2}?",
        "Is {8} a {9} module?",
        "How many tutorial videos does {8} have assigned to it?",
        "How many people have worked on {8}?",
        "What is the last digit of the {10} {8} was released on?",
        "What is the {3} digit of the Steam ID of {8}?",
        "Does {8} have its source code available?",
        "What is the Souvenir status of {8}?"
    };

    private readonly string[] selectorQuestions = new string[]
    {
        //"What is the {3} letter/digit of the module that is loaded {1} in game?",
        "Is {4} ultimately disabled by profiles?",
        //"What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules? (A-Z)",
        //"What is the {3} letter/digit of the module that is the {6} one on a list of all enabled modules? (A-Z)"
    };

    private readonly string[] bothQuestions = new string[]
    {
        //"Is the {0} module on the repo sorted by {2} loaded in the game?",
        //"Is the {1} module loaded in game (A-Z) the same module as the {1} module on the repo sorted by {2}?",
        //"What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules sorted by {2} on the repo?",
        //"What is the {3} letter/digit of the module that is the {6} one on a list of all enabled modules sorted by {2} on the repo?",
    };

    private readonly string[] neitherQuestions = new string[]
    {
        "What is the {3} letter/digit of the module that is {7} on the bomb sorted A-Z?",
        "What is the {3} letter/digit of the module that is {7} on the bomb sorted Z-A?"
    };

    private readonly string[] orderTypes = new string[]
    {
        "Sort keys (A-Z)",
        "Sort keys (Z-A)",
        "Defuser difficulty (very easy - very hard)",
        "Defuser difficulty (very hard - very easy)",
        "Expert difficulty (very easy - very hard)",
        "Expert difficulty (very hard - very easy)",
        "Publish date (newest to oldest)",
        "Publish date (oldest to newest)"
    };

    private readonly string[] ModuleTypes = new string[]
    {
        "regular",
        "needy"
    };

    private readonly string[] ReleaseDateTypes = new string[]
    {
        "year",
        "month",
        "day"
    };

    private readonly string[] dependsOnDisableds = new string[]
    {
        //"What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules? (A-Z)",
        //"What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules sorted by {2} on the repo?"
    };

    private LoopingList<string> answerSet = new LoopingList<string>();

    private readonly List<string> boolAns = new List<string>()
    {
        "YES",
        "NO"
    };

    private readonly List<string> letterAns = new List<string>()
    {
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H",
        "I",
        "J",
        "K",
        "L",
        "M",
        "N",
        "O",
        "P",
        "Q",
        "R",
        "S",
        "T",
        "U",
        "V",
        "W",
        "X",
        "Y",
        "Z",
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9"
    };

    private readonly List<string> digitAns = new List<string>()
    {
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9"
    };

    private readonly List<string> souvenirAns = new List<string>()
    {
        "Unexamined",
        "Not a candidate",
        "Considered",
        "Supported"
    };

    private questionerService Service;
    private IDictionary<string, object> API { get { try { return questionerService.modSelectorAPI; } catch { toggleObject("Error"); return null; } } }
    [HideInInspector]
    public bool _solved = false;
    private List<Module> fetchedModules { get { try { return questionerService.modulesFromWeb; } catch { toggleObject("Error"); return new List<Module>(); } } }
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
    public Dictionary<string, Tuple<KMSelectable, GameObject>> tpDisabled = new Dictionary<string, Tuple<KMSelectable, GameObject>>();

    [HideInInspector]
    public bool grantSolve = false;
    protected bool ableToSet = false;

    private int stage = 0;

    private int index = 0;

    [HideInInspector]
    public TextMesh inputText = null;

    private Material redMat;
    private GameObject statusC;

    private readonly float WaitTime = 0.045f;

    private LoopingList<string> getBaseList(Func<string> bases)
    {
        return new LoopingList<string>(new List<string>(bases().Select(c => c.ToString())));
    }

    [HideInInspector]
    public bool _input = false;

    bool TwitchPlaysActive;

    [HideInInspector]
    public ModuleState State = ModuleState.Main;

    bool RestrictQuestions = false;

    private bool _colorblind
    {
        set
        {
            setColorblind(value);
        }
    }

    private string modifyName(string module)
    {
        string oldModule = module;
        module = module.ToUpperInvariant();
        var l = new List<string>(module.Select(c => c.ToString()));
        var final = new List<string>();
        foreach (string c in l)
        {
            if (letterAns.Contains(c)) final.Add(c);
        }
        return final.Count > 0 ? String.Join("", final.ToArray()) : modifyName(getModuleIDByName(oldModule));
    }

    void Start()
    {
        GetComponent<KMBombModule>().OnActivate += () => StartCoroutine(Starter());
    }

    IEnumerator Starter()
    {
        moduleID = ++moduleIDCounter;
        Service = FindObjectOfType<questionerService>();
        //Debug.Log("Setting dict");
        togglableObjects = new Dictionary<string, GameObject>()
        {
            /*{ "LetteredButtons", findFromRoot("LetteredButtons")},
            { "BooleanButtons", findFromRoot("BooleanButtons")},*/
            { "Error", findFromRoot("Error")}
        };
        //Debug.Log("Setting to true");
        ableToSet = true;
        foreach (KeyValuePair<string, GameObject> kPair in togglableObjects)
        {
            kPair.Value.SetActive(false);
        }
        /*if (Service == null)
        {
            Logger("Couldn't find service. Activating error screen...");
            grantSolve = true;
            toggleObject("Error", true);
            yield break;
        }*/
        findFromRoot("Error").SetActive(false);
        Logger("Waiting for service to finish...");
        yield return new WaitUntil(() => questionerService._done && displayText != null && inputText != null);
        Logger("Service finished, generating question...");
        foreach (string l in questionerService.toLog) Logger(l);
        redMat = findFromRoot("RedOBJ").GetComponent<Renderer>().material;
        statusC = findFromRoot("Display").transform.Find("Sphere").gameObject;
        if(TwitchPlaysActive)
        {
            State = ModuleState.TwitchCheck;
            answerSet = new LoopingList<string>() { "" };
            StartCoroutine(newText("Please press enter\non the module!"));
            yield return new WaitUntil(() => State == ModuleState.Main);
        }
        if (!questionerService.webQuestions && API == null)
        {
            statusC.GetComponent<Renderer>().material = redMat;
            findFromRoot("colorblindText").GetComponent<TextMesh>().text = "R";
            finalQuestions = neitherQuestions.ToArray();
        }
        else if (!questionerService.webQuestions && API != null)
        {
            statusC.GetComponent<Renderer>().material = redMat;
            findFromRoot("colorblindText").GetComponent<TextMesh>().text = "R";
            finalQuestions = selectorQuestions.ToArray().Concat(neitherQuestions).ToArray();
        }
        else if (questionerService.webQuestions && API == null)
        {
            finalQuestions = webQuestions.ToArray().Concat(neitherQuestions).ToArray();
        }
        else
        {
            finalQuestions = webQuestions.ToArray().Concat(selectorQuestions).Concat(neitherQuestions).Concat(bothQuestions).ToArray();
        }
        if (RestrictQuestions) finalQuestions = finalQuestions.Where(x => !selectorQuestions.Contains(x) && !bothQuestions.Contains(x)).ToArray();
        sortModules();
        newStage();
        StartCoroutine(Blinker());
        if (API != null) Logger(String.Format("Light is {0}, Selector modules ordered by their {1}s: {2}", questionerService.webQuestions ? "green" : "red", questionerService.webQuestions ? "sort key" : "ID", String.Join(", ", getSelectorModules().ToArray())));
        _colorblind = GetComponent<KMColorblindMode>().ColorblindModeActive;
    }


    private IEnumerator Blinker()
    {
        while (!_solved)
        {
            statusC.GetComponent<Renderer>().enabled = false;
            yield return new WaitForSeconds(.5f);
            statusC.GetComponent<Renderer>().enabled = true;
            yield return new WaitForSeconds(.5f);
        }
        yield return null;
        statusC.GetComponent<Renderer>().enabled = false;
    }

    private GameObject findFromRoot(string _name)
    {
        return transform.Find("Objects").Find(_name).gameObject;
    }

    void Logger(string l)
    {
        Debug.LogFormat("[Repo Selector #{0}] {1}", moduleID, l);
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
        string moduleType = ModuleTypes[RNG.Range(0, ModuleTypes.Length)];
        string releaseDateType = ReleaseDateTypes[RNG.Range(0, ReleaseDateTypes.Length)];
        //Debug.LogFormat("[Questioner module #{0}] Sort type is {1}.", moduleID, sortType);
        string rndModule = API != null ? getModuleNameByID(MSModules[RNG.Range(0, MSModules.Count)]) : "";
        var bombSort = GetComponent<KMBombInfo>().GetModuleNames().ToList();
        bombSort.Sort();
        int bombIndex = RNG.Range(1, bombSort.Count + 1);
        string activeModule = "";
        var useSteamID = false;
        switch (set[index])
        {
            case "Is the {1} module loaded in game (A-Z) the same module as the {1} module on the repo sorted by {2}?":
                activeModule = getModuleNameByID(MSModules[selectorIndex - 1]);
                break;
            case "Is the {0} module on the repo sorted by {2} loaded in the game?":
            case "What is the {3} letter/digit of the module that is the {0} on the repo sorted by {2}?":
                activeModule = sortedModules[sortType][repoIndex - 1].Name;
                break;
            case "What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules sorted by {2} on the repo?":
                //Debug.LogFormat("[Questioner module #{0}] Trying to get index {1} of {2}", moduleID, disabledIndex - 1, getSortageByProperty(sortType, MSDisabledModules).Count);
                activeModule = getModuleNameByID(getSortageByProperty(sortType, MSDisabledModules)[disabledIndex - 1]);
                break;
            case "What is the {3} letter/digit of the module that is the {6} one on a list of all enabled modules sorted by {2} on the repo?":
                //Debug.LogFormat("[Questioner module #{0}] Trying to get index {1} of {2}", moduleID, enabledIndex - 1, getSortageByProperty(sortType, MSEnabledModules).Count);
                activeModule = getModuleNameByID(getSortageByProperty(sortType, MSEnabledModules)[enabledIndex - 1]);
                break;
            case "What is the {3} letter/digit of the module that is loaded {1} in game?":
                activeModule = getModuleNameByID(MSModules[selectorIndex - 1]);
                break;
            case "Is {4} ultimately disabled by profiles?":
                activeModule = getModuleNameByID(rndModule);
                break;
            case "What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules? (A-Z)":
                activeModule = getModuleNameByID(MSDisabledModules[disabledIndex - 1]);
                break;
            case "What is the {3} letter/digit of the module that is the {6} one on a list of all enabled modules? (A-Z)":
                activeModule = getModuleNameByID(MSEnabledModules[enabledIndex - 1]);
                break;
            case "What is the {3} letter/digit of the module that is {7} on the bomb sorted A-Z?":
                activeModule = bombSort[bombIndex - 1];
                break;
            case "What is the {3} letter/digit of the module that is {7} on the bomb sorted Z-A?":
                bombSort.Reverse();
                activeModule = bombSort[bombIndex - 1];
                break;
            case "Is {8} a {9} module?":
            case "How many tutorial videos does {8} have assigned to it?":
            case "How many people have worked on {8}?":
            case "What is the last digit of the {10} {8} was released on?":
            case "Does {8} have its source code available?":
            case "What is the Souvenir status of {8}?":
                activeModule = fetchedModules[repoIndex - 1].Name;
                break;
            case "What is the {3} digit of the Steam ID of {8}?":
                while(string.IsNullOrEmpty(fetchedModules[repoIndex - 1].SteamID))
                    repoIndex = RNG.Range(1, fetchedModules.Count + 1);
                activeModule = fetchedModules[repoIndex - 1].Name;
                useSteamID = true;
                break;
        }

        var newActive = useSteamID ? fetchedModules[repoIndex - 1].SteamID : modifyName(activeModule);

        int letterIndex = RNG.Range(1, newActive.Length + 1);

        string Letter = newActive[letterIndex - 1].ToString().ToUpperInvariant();

        string finalRepoIndex = getStringByNum(repoIndex);
        string finalSelectorIndex = getStringByNum(selectorIndex);
        string finalDisabledIndex = getStringByNum(disabledIndex);
        string finalEnabledIndex = getStringByNum(enabledIndex);
        string finalLetterIndex = getStringByNum(letterIndex);

        string Answer = "";
        switch (set[index])
        {
            case "Is the {0} module on the repo sorted by {2} loaded in the game?":
                Answer = MSModules.Contains(getModuleIDByName(activeModule)) ? "YES" : "NO";
                //toggleObject("BooleanButtons");
                answerSet = new LoopingList<string>(boolAns);
                break;
            case "Is the {1} module loaded in game (A-Z) the same module as the {1} module on the repo sorted by {2}?":
                Answer = fetchedModules[repoIndex - 1].Name == activeModule ? "YES" : "NO";
                //toggleObject("BooleanButtons");
                answerSet = new LoopingList<string>(boolAns);
                break;
            case "What is the {3} letter/digit of the module that is the {0} on the repo sorted by {2}?":
            case "What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules sorted by {2} on the repo?":
            case "What is the {3} letter/digit of the module that is the {6} one on a list of all enabled modules sorted by {2} on the repo?":
            case "What is the {3} letter/digit of the module that is loaded {1} in game?":
            case "What is the {3} letter/digit of the module that is the {5} one on a list of all disabled modules? (A-Z)":
            case "What is the {3} letter/digit of the module that is the {6} one on a list of all enabled modules? (A-Z)":
            case "What is the {3} letter/digit of the module that is {7} on the bomb sorted A-Z?":
            case "What is the {3} letter/digit of the module that is {7} on the bomb sorted Z-A?":
                Answer = Letter;
                //toggleObject("LetteredButtons");
                answerSet = new LoopingList<string>(letterAns);
                break;
            case "Is {4} ultimately disabled by profiles?":
                Answer = MSDisabledModules.Contains(getModuleIDByName(activeModule)) ? "YES" : "NO";
                //toggleObject("BooleanButtons");
                answerSet = new LoopingList<string>(boolAns);
                break;
            case "Is {8} a {9} module?":
                Answer = fetchedModules[repoIndex - 1].Type == moduleType ? "YES" : "NO";
                answerSet = new LoopingList<string>(boolAns);
                break;
            case "How many tutorial videos does {8} have assigned to it?":
                var tutorialCount = fetchedModules[repoIndex - 1].TutorialVideos.Length;
                Answer = tutorialCount.ToString();
                var fillerList = new List<int>();
                if (tutorialCount > 9)
                    fillerList = Enumerable.Range(10, tutorialCount - 9 + RNG.Range(5, 30)).ToList();
                answerSet = new LoopingList<string>(digitAns.Concat(fillerList.Select(n => n.ToString())).ToList());
                break;
            case "How many people have worked on {8}?":
                var authorCount = fetchedModules[repoIndex - 1].Contributors.Count == 0
                    ? fetchedModules[repoIndex - 1].Authors.Length
                    : fetchedModules[repoIndex - 1]
                        .Contributors
                        .Values.SelectMany(c => c).Distinct().Count();
                Answer = authorCount.ToString();
                var completeList = new List<int>();
                if (authorCount > 9)
                    completeList = Enumerable.Range(10, authorCount - 9 + RNG.Range(5, 30)).ToList();
                answerSet = new LoopingList<string>(digitAns.Concat(completeList.Select(n => n.ToString())).ToList());
                break;
            case "What is the last digit of the {10} {8} was released on?":
                Answer = (releaseDateType == "year" ? fetchedModules[repoIndex - 1].DeserializedPublishDate.Year % 10 :
                    releaseDateType == "month" ? fetchedModules[repoIndex - 1].DeserializedPublishDate.Month % 10 :
                    fetchedModules[repoIndex - 1].DeserializedPublishDate.Day % 10).ToString();
                answerSet = new LoopingList<string>(digitAns);
                break;
            case "What is the {3} digit of the Steam ID of {8}?":
                Answer = Letter;
                answerSet = new LoopingList<string>(digitAns);
                break;
            case "Does {8} have its source code available?":
                Answer = string.IsNullOrEmpty(fetchedModules[repoIndex - 1].SourceURL) ? "NO" : "YES";
                answerSet = new LoopingList<string>(boolAns);
                break;
            case "What is the Souvenir status of {8}?":
                Answer = fetchedModules[repoIndex - 1].Souvenir["Status"];
                answerSet = new LoopingList<string>(souvenirAns);
                break;
        }
        Logger(String.Format("Active module: {0}", activeModule));
        return new Tuple<string, string>(
            String.Format(set[index], finalRepoIndex, finalSelectorIndex, sortType, finalLetterIndex,
                getModuleNameByID(rndModule), finalDisabledIndex, finalEnabledIndex, getStringByNum(bombIndex),
                activeModule, moduleType, releaseDateType), Answer);
    }

    string getStringByNum(int num)
    {
        switch (num)
        {
            case 11: return "11th";
            case 12: return "12th";
            case 13: return "13th";
            default: return String.Format("{0}{1}", num, num % 10 == 1 ? "st" : num % 10 == 2 ? "nd" : num % 10 == 3 ? "rd" : "th");
        }
    }

    private List<string> getSortageByProperty(string sort, List<string> baseSet)
    {
        List<Module> sortedList = sortedModules[sort];
        List<string> ModuleIDs = new List<string>();
        foreach (Module module in sortedList)
        {
            //Debug.LogFormat("Checking module {0}", module.ID);
            if (baseSet.Contains(module.ModuleID)) { ModuleIDs.Add(module.ModuleID); /*Debug.Log("Correct!");*/ }
        }
        //Debug.LogFormat("Got number of sorted modules: {0} from: {1} with sorted being {2} and sortage being {3}", ModuleIDs.Count, baseSet.Count, sortedList.Count, sort);
        return ModuleIDs;
    }

    private void sortModules()
    {
        sortedModules.Clear();
        List<Module> tempList = fetchedModules.ToList();
        Debug.LogFormat("First sort key: {0} ({1})", tempList[0].SortKey, tempList[0].Name);
        sortedModules.Add("Sort keys (A-Z)", tempList.ToList());
        tempList.Reverse();
        sortedModules.Add("Sort keys (Z-A)", tempList.ToList());
        tempList.Reverse();

        List<Module> veryEasy = new List<Module>();
        List<Module> Easy = new List<Module>();
        List<Module> Medium = new List<Module>();
        List<Module> Hard = new List<Module>();
        List<Module> veryHard = new List<Module>();
        foreach (Module module in tempList)
        {
            switch (module.DefuserDifficulty)
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
            switch (module.ExpertDifficulty)
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

        tempList = tempList.OrderBy(x => x.DeserializedPublishDate).ThenByDescending(x => x.SortKey).ToList();
        sortedModules.Add("Publish date (oldest to newest)", tempList.ToList());
        tempList.Reverse();
        sortedModules.Add("Publish date (newest to oldest)", tempList.ToList());
        if (!questionerService.webQuestions) return;
        foreach (KeyValuePair<string, List<Module>> pair in sortedModules)
        {
            Logger(String.Format("Modules sorted by {0}: {1}", pair.Key, String.Join(", ", pair.Value.Select(item => item.ModuleID).ToArray())));
        }
    }

    private void newStage()
    {
        solvePair = getSolvePair(finalQuestions);
        index = 0;
        Logger(String.Format("Question is: '{0}', answer is: '{1}'", solvePair.First, solvePair.Second));
        stage++;
        List<string> question = solvePair.First.Split(new char[] { ' ' }).ToList();
        var lines = new List<string>() { "" };
        int Counter = 0;
        while (question.Count > 0)
        {
            bool _break = false;
            var lastLine = lines[lines.Count - 1];
            var lastLine2 = Counter == 0 ? lastLine + question[0] : lastLine + " " + question[0];
            if (lastLine2.Length > 30)
            {
                lines.Add(question[0]);
                Counter = 0;
            }
            else lines[lines.Count - 1] = lastLine2;
            question.RemoveAt(0);
            Counter++;
        }
        StartCoroutine(newText(string.Join("\n", lines.ToArray())));
        return;
    }

    private IEnumerator newText(string modified)
    {
        yield return writeText(displayText, modified);
        yield return writeText(inputText, answerSet[index]);
    }

    public void registerAns(string answer)
    {
        Logger(String.Format("Submitted answer: {0}, expected answer: {1}", answer, solvePair.Second));
        if (answer == solvePair.Second)
        {
            Logger("That's correct!");
            if (stage == 2)
            {
                Logger("Module solved!");
                GetComponent<KMBombModule>().HandlePass();
                _solved = true;
                index = 0;
                answerSet = new LoopingList<string>() { "GG!" };
                StartCoroutine(newText("Module solved :D"));
                return;
            }
            newStage();
        }
        else
        {
            Logger("That's incorrect! Strike!");
            GetComponent<KMBombModule>().HandleStrike();
            stage = 0;
            newStage();
        }
        return;
    }

    public void StartMove(Move move)
    {
        index += move == Move.Left ? -1 : 1;
        if (index == answerSet.Count) index = 0;
        if (index == -1) index = answerSet.Count - 1;
        StartCoroutine(writeText(inputText, answerSet[index]));
    }

    IEnumerator writeText(TextMesh display, string q)
    {
        yield return stringWriter(() => display.text, (s) => display.text = s);
        yield return stringWriter(() => q, (s) => display.text = s, Modifier.Add);
    }

    public IEnumerator stringWriter(Func<string> bases, Action<string> modify, Modifier modifier = Modifier.Remove)
    {
        _input = false;
        if (modifier == Modifier.Remove)
        {
            while (bases().Length > 0)
            {
                LoopingList<string> cList = getBaseList(bases);
                cList.RemoveAt(cList.Count - 1);
                modify(String.Join("", cList.ToArray()));
                yield return new WaitForSeconds(WaitTime);
            }
            _input = true;
            yield break;
        }
        LoopingList<string> chList = getBaseList(bases);
        List<string> final = new List<string>();
        while (chList.Count > 0)
        {
            final.Add(chList[0]);
            chList.RemoveAt(0);
            modify(String.Join("", final.ToArray()));
            yield return new WaitForSeconds(WaitTime);
        }
        _input = true;
    }

    private void toggleObject(string objName, bool force = false)
    {
        if (objName == "Error" && !force) return;
        /*togglableObjects["LetteredButtons"].SetActive(false);
        togglableObjects["BooleanButtons"].SetActive(false);*/
        togglableObjects[objName].SetActive(true);
        return;
    }

    private string getModuleNameByID(string ID)
    {
        foreach (var Module in fetchedModules)
        {
            if (Module.ModuleID == ID) return Module.Name;
        }
        return ID;
    }

    private string getModuleIDByName(string Name)
    {
        foreach (var Module in fetchedModules)
        {
            if (Module.Name == Name) return Module.ModuleID;
        }
        return Name;
    }

    private string getModuleSortKeyByID(string ID)
    {
        foreach (var Module in fetchedModules)
        {
            if (Module.ModuleID == ID) return Module.SortKey;
        }
        return ID;
    }

    private List<string> getSelectorModules()
    {
        if (API == null) return new List<string>();
        var got = ((IEnumerable<string>)API["AllSolvableModules"]).Concat((IEnumerable<string>)API["AllNeedyModules"]).ToList();
        if (!questionerService.webQuestions)
        {
            got.Sort();
            return got;
        }
        return pairModules(got);
    }
    private List<string> getDisabledModules()
    {
        if (API == null) return new List<string>();
        var got = ((IEnumerable<string>)API["DisabledSolvableModules"]).Concat((IEnumerable<string>)API["DisabledNeedyModules"]).ToList();
        if (!questionerService.webQuestions)
        {
            got.Sort();
            return got;
        }
        return pairModules(got);
    }
    private List<string> getEnabledModules()
    {
        if (API == null) return new List<string>();
        var got = getSelectorModules().Except(getDisabledModules()).ToList();
        if (!questionerService.webQuestions)
        {
            got.Sort();
            return got;
        }
        return pairModules(got);
    }

    private List<string> pairModules(List<string> bases)
    {
        var modulePairs = new List<Tuple<string, string>>();
        foreach (string mID in bases) modulePairs.Add(new Tuple<string, string>(mID, getModuleSortKeyByID(mID)));
        modulePairs = modulePairs.OrderBy(item => item.Second).ToList();
        var final = new List<string>();
        foreach (var _m in modulePairs)
        {
            final.Add(_m.First);
        }
        return final;
    }

    private void setColorblind(bool v)
    {
        if (v) findFromRoot("colorblindText").GetComponent<Renderer>().enabled = true;
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (grantSolve)
        {
            yield return ProcessTwitchCommand("press solve");
            yield break;
        }
        if(State == ModuleState.TwitchCheck)
        {
            yield return ProcessTwitchCommand("go");
            yield return null;
            yield return new WaitUntil(() => _input);
        }
        while (!_solved)
        {
            yield return ProcessTwitchCommand(String.Format("submit {0}", solvePair.Second));
            yield return true;
        }
    }

#pragma warning disable 414
    [HideInInspector]
    public string TwitchHelpMessage = "Use '!{0} go' to start the module! '!{0} submit <answer>' to submit an answer! Use '!{0} press solve' to solve the module if the error screen is present! Use '!{0} colorblind' to enable colorblind mode!";
#pragma warning restore 414
    public IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant();
        if(State==ModuleState.TwitchCheck)
        {
            if(command=="GO")
            {
                RestrictQuestions = true;
                yield return null;
                findFromRoot("Display").transform.Find("enter").GetComponent<KMSelectable>().OnInteract();
                yield break;
            }
            yield return null;
            yield return "sendtochaterror While the module is in TwitchCheck mode, the command can only be 'GO'";
            yield break;
        }
        if (command == "COLORBLIND")
        {
            yield return null;
            _colorblind = true;
            yield break;
        }
        if (command.StartsWith("PRESS "))
        {
            command = command.Replace("PRESS ", "");
            if (!btnsForTP.ContainsKey(command) || !btnsForTP[command].Second.activeInHierarchy)
            {
                yield return null;
                yield return "sendtochaterror Looks like the button you entered is either invalid or inactive!";
                yield break;
            }
            yield return null;
            btnsForTP[command].First.OnInteract();
            yield break;
        }
        if (!command.StartsWith("SUBMIT "))
        {
            yield return null;
            yield return "sendtochaterror Commands must start with 'press', 'submit' or 'colorblind' after the checking state!";
            yield break;
        }
        if (grantSolve)
        {
            yield return null;
            yield return "sendtochaterror The error screen is present, please use the 'press solve' command to solve the module!";
            yield break;
        }
        command = command.Replace("SUBMIT ", "");
        yield return new WaitUntil(() => _input);
        if (!answerSet.Contains(command))
        {
            yield return null;
            yield return "sendtochaterror Answer is invalid!";
            yield break;
        }
        while (inputText.text != command)
        {
            yield return null;
            tpDisabled["RightButton"].First.OnInteract();
            yield return new WaitUntil(() => _input);
        }
        findFromRoot("Display").transform.Find("enter").GetComponent<KMSelectable>().OnInteract();
    }
}
