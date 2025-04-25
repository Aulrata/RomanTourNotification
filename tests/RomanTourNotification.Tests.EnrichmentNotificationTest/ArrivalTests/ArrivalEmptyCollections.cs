using FluentAssertions;
using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Tests.EnrichmentNotificationTest.ArrivalTests;

public class ArrivalEmptyCollections
{
    [Fact]
    public void Requests_EmptyCollection_ShouldHandleGracefully()
    {
        var requests = new List<Request>();

        requests.Should().BeEmpty();
    }

    [Fact]
    public void Requests_RequestWithoutServices_ShouldHandleCorrectly()
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
            new List<InformationServices>());

        var requests = new List<Request> { requestWithoutServices };

        requests.First().Services.Should().BeEmpty();
    }

    [Fact]
    public void Requests_ServiceWithoutFlights_ShouldHandleCorrectly()
    {
        var serviceWithoutFlights = new InformationServices(
            1,
            1,
            "2023-02-01",
            "2023-02-02",
            11,
            new List<Flights>()); // Insurance

        var request = new Request(
            2,
            2,
            string.Empty,
            "2023-02-01",
            "Supplier2",
            "LastName2",
            "FirstName2",
            "MiddleName2",
            "email2@example.com",
            "2023-02-02",
            "2",
            new List<InformationServices> { serviceWithoutFlights });

        var requests = new List<Request> { request };

        InformationServices service = requests.First().Services.First();
        service.Flights.Should().BeEmpty();
    }

    [Fact]
    public void Requests_InvalidFlightsTypeId_ShouldThrowException()
    {
        Action createFlightWithInvalidTypeId = () => new Flights(
            1,
            "2023-03-01",
            "2023-03-02",
            "invalid");

        createFlightWithInvalidTypeId.Should().Throw<FormatException>();
    }

    [Fact]
    public void Requests_UnknownServiceTypeId_ShouldSetInformationServiceTypeCorrectly()
    {
        var serviceWithUnknownType = new InformationServices(
            2,
            2,
            "2023-04-01",
            "2023-04-02",
            999,
            new List<Flights>());

        var request = new Request(
            3,
            3,
            string.Empty,
            "2023-04-01",
            "Supplier3",
            "LastName3",
            "FirstName3",
            "MiddleName3",
            "email3@example.com",
            "2023-04-02",
            "3",
            new List<InformationServices> { serviceWithUnknownType });

        var requests = new List<Request> { request };

        InformationServices service = requests.First().Services.First();
        service.InformationServiceType.Should().Be((InformationServiceType)999);
    }

    [Fact]
    public void Requests_MultipleServicesOfSameType_ShouldHandleCorrectly()
    {
        var service1 = new InformationServices(
            3,
            3,
            "2023-05-01",
            "2023-05-02",
            1,
            new List<Flights>());
        var service2 = new InformationServices(
            4,
            3,
            "2023-05-03",
            "2023-05-04",
            1,
            new List<Flights>());

        var request = new Request(
            4,
            4,
            string.Empty,
            "2023-05-01",
            "Supplier4",
            "LastName4",
            "FirstName4",
            "MiddleName4",
            "email4@example.com",
            "2023-05-04",
            "4",
            new List<InformationServices> { service1, service2 });

        var requests = new List<Request> { request };

        IEnumerable<InformationServices> services = requests.First().Services.ToList();
        services.Should().HaveCount(2);
        services.All(s => s.InformationServiceType == InformationServiceType.Hotel).Should().BeTrue();
    }

    [Fact]
    public void Requests_FlightsWithSameDates_ShouldHandleCorrectly()
    {
        var flight1 = new Flights(1, "2023-06-01", "2023-06-02", "1");
        var flight2 = new Flights(2, "2023-06-01", "2023-06-02", "2");

        var service = new InformationServices(
            5,
            5,
            "2023-06-01",
            "2023-06-02",
            6,
            new List<Flights> { flight1, flight2 });

        var request = new Request(
            5,
            5,
            string.Empty,
            "2023-06-01",
            "Supplier5",
            "LastName5",
            "FirstName5",
            "MiddleName5",
            "email5@example.com",
            "2023-06-02",
            "5",
            new List<InformationServices> { service });

        var requests = new List<Request> { request };

        var flights = requests.First().Services.First().Flights?.ToList();
        flights.Should().HaveCount(2);
        flights.All(f => f is { DateBegin: "2023-06-01", DateEnd: "2023-06-02" }).Should().BeTrue();
    }
}