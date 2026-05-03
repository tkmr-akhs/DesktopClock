using DesktopClock.Core.Helpers;

namespace DesktopClock.Tests.xUnit;

public class GoogleHolidayCalendarHelperTests
{
    [Theory]
    [InlineData(
        "ja.japanese#holiday@group.v.calendar.google.com",
        "ja.japanese.official#holiday@group.v.calendar.google.com")]
    [InlineData(
        "en.japanese#holiday@group.v.calendar.google.com",
        "en.japanese.official#holiday@group.v.calendar.google.com")]
    [InlineData(
        "ja.japanese.official#holiday@group.v.calendar.google.com",
        "ja.japanese.official#holiday@group.v.calendar.google.com")]
    [InlineData(
        "primary",
        "primary")]
    public void ToPublicHolidayOnlyCalendarId_WhenPassedCalendarId_ReturnsExpectedId(string calendarId, string expected)
    {
        // act
        var actual = GoogleHolidayCalendarHelper.ToPublicHolidayOnlyCalendarId(calendarId);

        // assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ja.japanese#holiday@group.v.calendar.google.com", true)]
    [InlineData("ja.japanese.official#holiday@group.v.calendar.google.com", true)]
    [InlineData("en.japanese#holiday@group.v.calendar.google.com", true)]
    [InlineData("primary", false)]
    public void IsJapaneseHolidayCalendarId_WhenPassedCalendarId_ReturnsExpectedResult(string calendarId, bool expected)
    {
        // act
        var actual = GoogleHolidayCalendarHelper.IsJapaneseHolidayCalendarId(calendarId);

        // assert
        Assert.Equal(expected, actual);
    }
}
