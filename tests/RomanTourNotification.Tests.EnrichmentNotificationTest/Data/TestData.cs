using Microsoft.Extensions.Logging;
using RomanTourNotification.Application.Contracts.DownloadData;
using RomanTourNotification.Application.Contracts.Gateway;
using RomanTourNotification.Application.DownloadData;
using RomanTourNotification.Application.EnrichmentNotification;
using RomanTourNotification.Application.Gateway;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Tests.EnrichmentNotificationTest.Data;

public static class TestData
{
    public static EnrichmentNotificationService GetEnrichmentService()
    {
        IEnumerable<ApiSettings> api = [new()];
        IGatewayService gateway = new GatewayService(new HttpClient(), new Logger<GatewayService>(new LoggerFactory()));

        ILoadDataService loadDataService = new LoadDataService(gateway, new Logger<LoadDataService>(new LoggerFactory()), api);

        return new EnrichmentNotificationService(new Logger<EnrichmentNotificationService>(new LoggerFactory()), loadDataService);
    }

    public static IEnumerable<Request> GetRequests()
    {
        var flight1 = new Flights(1, "2025-04-25", "2025-04-25", "2");
        var flight2 = new Flights(2, "2025-04-25", "2025-04-26", "2");
        var flight3 = new Flights(3, "2025-04-29", "2025-04-29", "2");
        var flight4 = new Flights(4, "2025-04-29", "2025-04-30", "2");

        var service1 = new InformationServices(
            1,
            1,
            string.Empty,
            string.Empty,
            6,
            [flight1, flight2, flight3, flight4]);

        var service2 = new InformationServices(
            1,
            1,
            "2025-04-26",
            "2025-04-29",
            1,
            new List<Flights>());

        var request1 = new Request(
            1,
            1,
            string.Empty,
            "2025-04-25",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-04-30",
            "2",
            new List<InformationServices> { service1, service2 },
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var request2 = new Request(
            2,
            2,
            string.Empty,
            "2025-04-25 10:05",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-04-27 22:05",
            "2",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var request3 = new Request(
            3,
            3,
            string.Empty,
            "2025-04-25 16:15",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-05-05 22:00",
            "4",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        return new List<Request> { request1, request2, request3 };
    }

    public static IEnumerable<Request> GetInSomeDaysLessTarget()
    {
        var request1 = new Request(
            1,
            1,
            "2025-04-19 10:05",
            "2025-04-23 10:55",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-04-27 22:05",
            "2",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var request2 = new Request(
            2,
            2,
            "2025-04-20 10:05",
            "2025-04-23 10:55",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-04-27 22:05",
            "2",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var request3 = new Request(
            3,
            3,
            "2025-04-21 10:05",
            "2025-04-23 10:55",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-04-27 22:05",
            "2",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var request4 = new Request(
            4,
            4,
            "2025-04-22 10:05",
            "2025-04-23 10:55",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-04-27 22:05",
            "2",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        var request5 = new Request(
            5,
            5,
            "2025-04-23 10:05",
            "2025-04-23 10:55",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "2025-04-27 22:05",
            "2",
            new List<InformationServices>(),
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        return new List<Request> { request1, request2, request3, request4, request5 };
    }
}