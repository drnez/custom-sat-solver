List<LogicalStatement> problem = new List<LogicalStatement>();
List<string> uniqueElements = new List<string>();

Dictionary<string, bool> elementStates = new Dictionary<string, bool>();
Dictionary<string, bool> finalElementStates = new Dictionary<string, bool>();

while (true) // Take input in CNF
{
    string input = Console.ReadLine();
    if (input == "") break;

    string[] inputArray = input.Split(' ');

    LogicalStatement newStatement = new LogicalStatement();

    foreach (string s in inputArray)
    {
        if (s == "0") break; // to conform with DIMACS

        SingleElement newElement;
        string elementStr = s[0] == '-' ? s.Substring(1) : s;

        newElement = new SingleElement(s[0] != '-', elementStr);

        if (!uniqueElements.Contains(elementStr)) uniqueElements.Add(elementStr); 

        newStatement.AddElement(newElement);
    }

    if (newStatement.statement.Count() > 0) problem.Add(newStatement);
}

if (problem.Count() == 0)
{
    Console.WriteLine("Solvable");
    Environment.Exit(0);
}

var watch = System.Diagnostics.Stopwatch.StartNew();
bool result = dfs(0, elementStates, problem, new SingleElement(true, "!"));
watch.Stop();
var elapsedMs = watch.ElapsedMilliseconds;

if (result)
{
    Console.WriteLine("Solvable:");
    foreach (KeyValuePair<string, bool> v in finalElementStates)
    {
        Console.WriteLine(v.Key + ": " + v.Value);
    }
}
else Console.WriteLine("Not solvable");

Console.WriteLine(elapsedMs + "ms elapsed");


bool dfs(int index, Dictionary<string, bool> elementStates, List<LogicalStatement> problem, SingleElement justChanged)
{
    if (simplify(problem, justChanged))
    {
        finalElementStates = elementStates;
        return true;
    }

    int propResult = unitPropogate(elementStates, problem);
    if (propResult == 0) return false; // backtrack if conflict
    if (propResult == 1)
    {
        finalElementStates = elementStates;
        return true;
    }

    while (index < uniqueElements.Count() && elementStates.ContainsKey(uniqueElements[index])) index++;

    if (index == uniqueElements.Count()) return false;

    elementStates[uniqueElements[index]] = true;
    if (dfs( index+1, new Dictionary<string, bool>(elementStates), problem.Select(statement => statement.Clone()).ToList(), new SingleElement(true, uniqueElements[index]) )) return true;

    elementStates[uniqueElements[index]] = false;
    if (dfs( index+1, new Dictionary<string, bool>(elementStates), problem.Select(statement => statement.Clone()).ToList(), new SingleElement(false, uniqueElements[index]) )) return true;

    return false;
}

bool simplify(List<LogicalStatement> problem, SingleElement justChanged) // true if satisfied!
{
    if (justChanged.id == "!") return false;

    for (int i = 0; i < problem.Count(); i++)
    {
        LogicalStatement statement = problem[i];

        for (int j = 0; j < statement.statement.Count(); j++)
        {
            SingleElement element = statement.statement[j];

            if (element.id == justChanged.id)
            {
                if (element.truth == justChanged.truth)
                {
                    problem.RemoveAt(i);
                    i--;
                    goto OUTERCONTINUE;
                } // else truth is different is implicit
                statement.statement.RemoveAt(j);
                j--;
            }
        }
        OUTERCONTINUE:;
    }

    if (problem.Count() == 0) return true;
    return false;
}

int unitPropogate(Dictionary<string, bool> elementStates, List<LogicalStatement> problem) // 0 if conflict, 1 if SAT, 2 else
{
    for (int i = 0; i < problem.Count(); i++)
    {
        LogicalStatement statement = problem[i];

        if (statement.statement.Count() == 0) return 0;
        if (statement.statement.Count() == 1)
        {
            string statementId = statement.statement[0].id;
            bool statementTruth = statement.statement[0].truth;

            elementStates[statementId] = statementTruth;
            
            if (simplify(problem, new SingleElement(statementTruth, statementId) )) return 1;
        }
    }
    return 2;
}

bool validStateCheck(List<LogicalStatement> problem)
{
    foreach (LogicalStatement statement in problem)
    {
        bool valid = false;

        foreach (SingleElement element in statement.statement)
        {
            if (elementStates[element.id] == element.truth)
            {
                valid = true;
                break;
            }
        }

        if (!valid) return false;
    }

    return true;
}

class LogicalStatement
{
    // One statement of ORs in list to be ANDed

    public List<SingleElement> statement {get; private set;}

    public void AddElement(SingleElement element) => statement.Add(element);

    public LogicalStatement Clone()
    {
        LogicalStatement clone = new LogicalStatement();
        foreach (SingleElement element in statement)
        {
            clone.AddElement(element.Clone());
        }
        return clone;
    }

    public LogicalStatement()
    {
        statement = new List<SingleElement>();
    }
}

class SingleElement
{
    // A, not A, B, not B, ... all represented as one object of this class

    public bool truth;
    public string id;

    public SingleElement Clone() => new SingleElement(truth, id);

    public bool Equals(SingleElement other) => this.id == other.id && this.truth == other.truth;

    public SingleElement(bool truth, string id)
    {
        this.truth = truth; this.id = id;
    }
}
