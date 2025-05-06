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

        EnrichmentNotificationService service = TestData.GetEnrichmentService();

        var result = service.GetEndTomorrow(requests, new DateDto(DateTime.Parse("2025-04-28"))).ToList();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public void Request_ShouldHaveCorrectData_GetBeginTomorrow()
    {
        IEnumerable<Request> requests = TestData.GetRequests();

        EnrichmentNotificationService service = TestData.GetEnrichmentService();

        var result = service.GetBeginTomorrow(requests, new DateDto(DateTime.Parse("2025-04-24"))).ToList();

        result.Should().HaveCount(1);

        result.First().Id.Should().Be(1);
    }

    [Fact]
    public void Request_ShouldHaveCorrectData_GetDateBeginInSomeDays()
    {
        IEnumerable<Request> requests = TestData.GetRequests();

        EnrichmentNotificationService service = TestData.GetEnrichmentService();

        var result = service.GetDateBeginInSomeDays(requests, new DateDto(DateTime.Parse("2025-04-22"))).ToList();

        result.Should().HaveCount(3);

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

        EnrichmentNotificationService service = TestData.GetEnrichmentService();

        var result = service.GetBeginTomorrow(requests, new DateDto(DateTime.Parse("2025-04-19"))).ToList();

        result.Should().HaveCount(0);
    }

    [Fact]
    public void Request_ShouldHaveCorrectCount_GetInSomeDaysLessTarget()
    {
        IEnumerable<Request> requests = TestData.GetInSomeDaysLessTarget().ToList();

        EnrichmentNotificationService service = TestData.GetEnrichmentService();

        var result = service.GetDateBeginInSomeDays(requests, new DateDto(DateTime.Parse("2025-04-21"))).ToList();
        var result2 = service.GetDateBeginInSomeDays(requests, new DateDto(DateTime.Parse("2025-04-22"))).ToList();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(2);
        result2.Should().HaveCount(1);
        result2.First().Id.Should().Be(3);
    }
}