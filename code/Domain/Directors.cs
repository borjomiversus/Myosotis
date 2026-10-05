using System.Collections.Generic;

public class Director : Person
{
    public string SignatureStyle { get; private set; }
    public List<string> DirectedWorks { get; private set; }

    public Director(string fullName, int birthYear, string signatureStyle) : base(fullName, birthYear)
    {
        SignatureStyle = signatureStyle;
        DirectedWorks = new List<string>();
    }

    public void AddDirectedWork(string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
            DirectedWorks.Add(title);
    }
}