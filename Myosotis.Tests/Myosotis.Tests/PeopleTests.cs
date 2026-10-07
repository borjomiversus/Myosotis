using System;
using Xunit;

public class PeopleTests
{
    [Fact]
    public void Person_GetAge_CalculatesCorrectly()
    {
        var p = new Person("Jane Doe", 1990);
        Assert.Equal(36, p.GetAge(2026));
    }

    [Fact]
    public void Person_UpdateBiography_ChangesBiography()
    {
        var p = new Person("Jane Doe", 1990);
        p.UpdateBiography("Test Bio");
        Assert.Equal("Test Bio", p.Biography);
    }

    [Fact]
    public void Person_InvalidConstructor_ThrowsExceptions()
    {
        Assert.Throws<ArgumentException>(() => new Person("", 1990));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Person("John", 1800));
    }

    [Fact]
    public void Actor_AddToFilmography_AddsElement()
    {
        var a = new Actor("Test Actor", 1980, "Hero");
        a.AddToFilmography("Movie A");

        Assert.Equal(1, a.GetWorksCount());
        Assert.True(a.HasWorkedOn("Movie A"));
    }

    [Fact]
    public void Director_AddDirectedWork_AddsToList()
    {
        var d = new Director("Test Director", 1970, "Style");
        d.AddDirectedWork("Film X");

        Assert.Contains("Film X", d.DirectedWorks);
        Assert.Equal(56, d.GetAge(2026));
    }
}