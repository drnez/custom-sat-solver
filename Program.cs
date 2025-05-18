List<LogicalStatement> problem = new List<LogicalStatement>();
List<string> uniqueElements = new List<string>();

Dictionary<string, bool> elementStates = new Dictionary<string, bool>();

// 1. Take input in CNF

while (true)
{
    string input = Console.ReadLine();
    if (input == "") break;

    string[] inputArray = input.Split(' ');

    LogicalStatement newStatement = new LogicalStatement();

    foreach (string s in inputArray)
    {
        SingleElement newElement;
        string elementStr = s[0] == '¬' ? s.Substring(1) : s;

        newElement = new SingleElement(s[0] != '¬', elementStr);

        if (!uniqueElements.Contains(elementStr)) uniqueElements.Add(elementStr); 

        newStatement.AddElement(newElement);
    }

    problem.Add(newStatement);
}

if (dfs(0))
{
    Console.WriteLine("Solvable:");
    foreach (var v in elementStates)
    {
        Console.WriteLine(v.Key + ": " + v.Value);
    }
}
else Console.WriteLine("Not solvable");

bool dfs(int index)
{
    if (index == uniqueElements.Count()) return validStateCheck(); // not -1 since check should occur AFTER final state is set!

    elementStates[uniqueElements[index]] = true;
    if (dfs(index + 1)) return true;

    elementStates[uniqueElements[index]] = false;
    if (dfs(index + 1)) return true;

    return false;
}

bool validStateCheck()
{
    foreach (LogicalStatement statement in problem)
    {
        bool valid = false;

        foreach (SingleElement element in statement.statement)
        {
            if (elementStates[element.id] == element.isTrue)
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

    public LogicalStatement()
    {
        statement = new List<SingleElement>();
    }
}

class SingleElement
{
    // A, not A, B, not B, ... all represented as one object of this class

    public bool isTrue;
    public string id;

    public SingleElement(bool isTrue, string id)
    {
        this.isTrue = isTrue; this.id = id;
    }
}
