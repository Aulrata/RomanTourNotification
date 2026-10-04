using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.EnrichmentNotification;

/// <summary>Stateless implementation of <see cref="IFilterEnrichmentNotificationService"/>.</summary>
public class FilterEnrichmentNotificationService : IFilterEnrichmentNotificationService
{
    /// <inheritdoc/>
    public IEnumerable<Request> GetDateBeginInSomeDays(
        DateDto dateDto,
        IEnumerable<Request> requests,
        string managerFullname)
    {
        IEnumerable<Request> filtered = ApplyBaseFilter(requests, managerFullname);
        IEnumerable<Request> notIssued = filtered.Where(x => x.Status is not RequestStatus.DocumentsIssued);
        DateTime targetDate = dateDto.From.AddDays(dateDto.Days).Date;

        return notIssued
            .Where(r =>
                r.DateBeginAsDate == targetDate ||
                (r.DateBeginAsDate < targetDate && r.DateRequestAsDate?.AddDays(1) == dateDto.From) ||
                (r.DateBeginAsDate < targetDate && r.DateRequestAsDate?.AddDays(3) == dateDto.From && r.DateRequestAsDate?.DayOfWeek is DayOfWeek.Friday) ||
                (r.DateBeginAsDate < targetDate && r.DateRequestAsDate?.AddDays(2) == dateDto.From && r.DateRequestAsDate?.DayOfWeek is DayOfWeek.Saturday))
            .DistinctBy(r => r.IdSystem);
    }

    /// <inheritdoc/>
    public IEnumerable<Request> GetBeginTomorrow(
        DateDto dateDto,
        IEnumerable<Request> requests,
        string managerFullname)
    {
        IEnumerable<Request> filtered = ApplyBaseFilter(requests, managerFullname);
        DateTime tomorrow = dateDto.From.AddDays(1).Date;
        DateTime blockOfSeatsDate = dateDto.From.AddDays(2).Date;

        IEnumerable<Request> blockOfSeats = filtered
            .Where(r => r.DateBeginAsDate == blockOfSeatsDate &&
                        r.Services?
                            .Any(s =>
                                s.InformationServiceType == InformationServiceType.AirTicket &&
                                s.Flights
                                    .Any(f =>
                                        f.FlightsType == FlightsType.BlockOfSeats &&
                                        f.DateBeginAsDate == blockOfSeatsDate)) == true);

        IEnumerable<Request> charters = filtered
            .Where(r => r.DateBeginAsDate == tomorrow &&
                        r.Services?
                            .Any(s =>
                                s.InformationServiceType == InformationServiceType.AirTicket &&
                                s.Flights
                                    .Any(f => f.FlightsType == FlightsType.Charter &&
                                              f.DateBeginAsDate == tomorrow)) == true);

        return charters.Concat(blockOfSeats).DistinctBy(r => r.IdSystem);
    }

    /// <inheritdoc/>
    public IEnumerable<Request> GetEndTomorrow(
        DateDto dateDto,
        IEnumerable<Request> requests,
        string managerFullname)
    {
        IEnumerable<Request> filtered = ApplyBaseFilter(requests, managerFullname);
        DateTime tomorrow = dateDto.From.AddDays(1).Date;
        DateTime blockOfSeatsDate = dateDto.From.AddDays(2).Date;

        var blockOfSeatsRequests = new List<Request>();

        foreach (Request request in filtered)
        {
            if (request.Services.All(s => s.InformationServiceType is not InformationServiceType.AirTicket))
                continue;

            IEnumerable<InformationServices> airServices = request.Services
                .Where(s => s.InformationServiceType == InformationServiceType.AirTicket);

            InformationServices? currentFlight = airServices
                .Where(f => f.Flights.Any(f2 => f2.DateEndAsDate?.Date > blockOfSeatsDate))
                .MinBy(f => f.Flights.FirstOrDefault()?.DateBeginAsDate);

            var flightsList = currentFlight?.Flights.ToList();
            int? countOfRoutes = flightsList?.Count;

            switch (countOfRoutes)
            {
                case 2:
                    if (flightsList?[1].DateBeginAsDate == blockOfSeatsDate &&
                        flightsList[1].FlightsType == FlightsType.BlockOfSeats)
                    {
                        blockOfSeatsRequests.Add(request);
                    }

                    break;
                case 4:
                    if (flightsList?[2].DateBeginAsDate == blockOfSeatsDate &&
                        flightsList[2].FlightsType == FlightsType.BlockOfSeats)
                    {
                        blockOfSeatsRequests.Add(request);
                    }

                    break;
            }
        }

        IEnumerable<Request> charterRequests = filtered
            .Where(r => r.Services.Any(s => s is { InformationServiceType: InformationServiceType.AirTicket } &&
                                            s.Flights.Any(f => f.FlightsType == FlightsType.Charter)))
            .Where(r =>
            {
                InformationServices airTicketService =
                    r.Services.First(s => s.InformationServiceType == InformationServiceType.AirTicket);

                IEnumerable<Flights> charterFlights = airTicketService.Flights
                    .Where(f => f.FlightsType == FlightsType.Charter);

                var flightsWithDates = charterFlights
                    .Where(f => f.DateBeginAsDate is not null)
                    .ToList();

                var flightsWithLaterDates = flightsWithDates
                    .Where(f1 => flightsWithDates.Any(f2 => f2.DateBeginAsDate?.AddDays(2) < f1.DateBeginAsDate))
                    .ToList();

                if (flightsWithLaterDates.Count == 0) return false;

                Flights? minLaterFlight = flightsWithLaterDates.MinBy(f => f.DateBeginAsDate);
                return minLaterFlight?.DateBeginAsDate == tomorrow;
            });

        return blockOfSeatsRequests.Concat(charterRequests);
    }

    private static IEnumerable<Request> ApplyBaseFilter(IEnumerable<Request> requests, string managerFullname)
    {
        IEnumerable<Request> query = requests.Where(x => x.Status is not RequestStatus.Cancelled);

        if (!string.IsNullOrEmpty(managerFullname))
            query = query.Where(x => x.ManagerFullName == managerFullname);

        return query;
    }
}
