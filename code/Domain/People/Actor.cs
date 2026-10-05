using System.Collections.Generic;

public class Actor : Person
{
    public string RoleName { get; private set; }
    public List<string> Filmography { get; private set; }

    public Actor(string fullName, int birthYear, string roleName) : base(fullName, birthYear)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Назва ролі не може бути порожньою.");

        RoleName = roleName;
        Filmography = new List<string>();
    }

    public void AddToFilmography(string title)
    {
        Filmography.Add(title);
    }

    public int GetWorksCount()
    {
        return Filmography.Count;
    }

    public bool HasWorkedOn(string title)
    {
        return Filmography.Contains(title);
    }
}