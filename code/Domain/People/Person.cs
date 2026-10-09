using System;

public class Person
{
   public Guid Id { get; private set; } = Guid.NewGuid();
    public string FullName { get; private set; }
    public int BirthYear { get; private set; }
    public string? Biography { get; private set; }
    public string? PhotoUrl { get; private set; }   

    public Person(string fullName, int birthYear)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Ім'я не може бути порожнім.");
        if (birthYear < 1850 || birthYear > DateTime.Now.Year)
            throw new ArgumentOutOfRangeException(nameof(birthYear));

        FullName = fullName;
        BirthYear = birthYear;
    }

    // скільки років
    public int GetAge(int currentYear)
    {
        return currentYear - BirthYear;
    }

    public void UpdateBiography(string bio)
    {
        Biography = bio;
    }

    public string GetBasicInfo()
    {
        return $"{FullName}, народився у {BirthYear}";
    }
}