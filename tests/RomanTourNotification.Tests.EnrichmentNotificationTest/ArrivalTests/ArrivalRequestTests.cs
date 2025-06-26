using FluentAssertions;
using RomanTourNotification.Application.EnrichmentNotification;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;
using RomanTourNotification.Tests.EnrichmentNotificationTest.Data;

namespace RomanTourNotification.Tests.EnrichmentNotificationTest.ArrivalTests;

public class ArrivalRequestTests
{
    [Fact]
    public void Request_ShouldHaveCorrectCount()
    {
        IEnumerable<Request> requests = TestData.GetRequests();

        requests.Should().HaveCount(3);
    }

    [Fact]
    public void Request_ShouldHaveCorrectData_GetEndTomorrow()
    {
        IEnumerable<Request> requests = TestData.GetRequests();

        FilterEnrichmentNotificationService filter = TestData.GetFilterEnrichmentService();
        filter.SetData(new DateDto(DateTime.Parse("2025-04-28")), requests);

        var result = filter.GetEndTomorrow().ToList();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public void Request_ShouldHaveCorrectData_GetBeginTomorrow()
    {
        IEnumerable<Request> requests = TestData.GetRequests();

        FilterEnrichmentNotificationService filter = TestData.GetFilterEnrichmentService();
        filter.SetData(new DateDto(DateTime.Parse("2025-04-24")), requests);
        var result = filter.GetBeginTomorrow().ToList();

        result.Should().HaveCount(1);

        result.First().Id.Should().Be(1);
    }

    [Fact]
    public void Request_ShouldHaveCorrectData_GetDateBeginInSomeDays()
    {
        IEnumerable<Request> requests = TestData.GetRequests();

        FilterEnrichmentNotificationService filter = TestData.GetFilterEnrichmentService();
        filter.SetData(new DateDto(DateTime.Parse("2025-04-22")), requests);

        var result = filter.GetDateBeginInSomeDays().ToList();

        result.Should().HaveCount(2);

        // result.First().Id.Should().Be(1);
    }

    [Fact]
    public void Request_ShouldHaveCorrectCount_GetBeginTomorrow()
    {
        var requestWithoutServices = new Request(
            1,
            1,
            string.Empty,
            "2023-01-01",
            "Supplier1",
            "LastName1",
            "FirstName1",
            "MiddleName1",
            "email1@example.com",
            "2023-01-02",
            "1",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var requests = new List<Request> { requestWithoutServices };

        FilterEnrichmentNotificationService filter = TestData.GetFilterEnrichmentService();
        filter.SetData(new DateDto(DateTime.Parse("2025-04-19")), requests);

        var result = filter.GetBeginTomorrow().ToList();

        result.Should().HaveCount(0);
    }

    [Fact]
    public void Request_ShouldHaveCorrectCount_GetInSomeDaysLessTarget()
    {
        IEnumerable<Request> requests = TestData.GetInSomeDaysLessTarget().ToList();

        FilterEnrichmentNotificationService filter = TestData.GetFilterEnrichmentService();
        filter.SetData(new DateDto(DateTime.Parse("2025-04-21")), requests);

        var result = filter.GetDateBeginInSomeDays().ToList();

        filter.SetData(new DateDto(DateTime.Parse("2025-04-22")), requests);
        var result2 = filter.GetDateBeginInSomeDays().ToList();

        result.Should().HaveCount(2);
        result2.Should().HaveCount(1);
        result2.First().Id.Should().Be(3);
    }

    [Fact]
    public void Request_ShouldHaveBlockOfSeats()
    {
        IEnumerable<Request> requests = TestData.GetBlockOfSeats().ToList();

        FilterEnrichmentNotificationService filter = TestData.GetFilterEnrichmentService();
        filter.SetData(new DateDto(DateTime.Parse("2025-04-23")), requests);

        var result = filter.GetBeginTomorrow().ToList();

        result.Should().HaveCount(2);
    }
}