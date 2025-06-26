using RomanTourNotification.Application.Contracts.EnrichmentNotification;
using RomanTourNotification.Application.Models.EnrichmentNotification;
using RomanTourNotification.Application.Models.Gateway;

namespace RomanTourNotification.Application.EnrichmentNotification;

public class FilterEnrichmentNotificationService : IFilterEnrichmentNotificationService
{
    private DateDto _dateDto = new(DateTime.Today);
    private List<Request> _requests = [];

    public void SetData(DateDto dateDto, IEnumerable<Request> dateRequests)
    {
        _dateDto = dateDto;

        _requests = dateRequests
            .Where(x => x.Status is not RequestStatus.Cancelled)
            .ToList();
    }

    public IEnumerable<Request> GetDateBeginInSomeDays()
    {
        DateTime targetDate = _dateDto.From.AddDays(_dateDto.Days).Date;

        return _requests
            .Where(r =>
                r.DateBeginAsDate == targetDate ||
                (r.DateBeginAsDate < targetDate && r.DateRequestAsDate?.AddDays(1) == _dateDto.From) ||
                (r.DateBeginAsDate < targetDate && r.DateRequestAsDate?.AddDays(3) == _dateDto.From && r.DateRequestAsDate?.DayOfWeek is DayOfWeek.Friday) ||
                (r.DateBeginAsDate < targetDate && r.DateRequestAsDate?.AddDays(2) == _dateDto.From && r.DateRequestAsDate?.DayOfWeek is DayOfWeek.Saturday))
            .DistinctBy(r => r.IdSystem);
    }

    public IEnumerable<Request> GetBeginTomorrow()
    {
        DateTime tomorrow = _dateDto.From.AddDays(1).Date;
        DateTime blockOfSeatsDate = _dateDto.From.AddDays(2).Date;

        IEnumerable<Request> result = _requests
            .Where(r => r.DateBeginAsDate == blockOfSeatsDate &&
                        r.Services?
                            .Any(s =>
                                s.InformationServiceType == InformationServiceType.AirTicket &&
                                s.Flights
                                    .Any(f =>
                                        f.FlightsType == FlightsType.BlockOfSeats)) == true);

        return _requests
            .Where(r => r.DateBeginAsDate == tomorrow &&
                        r.Services?
                            .Any(s =>
                                s.InformationServiceType == InformationServiceType.AirTicket &&
                                s.Flights
                                    .Any(f => f.FlightsType == FlightsType.Charter)) == true)
            .Concat(result)
            .DistinctBy(r => r.IdSystem);
    }

    public IEnumerable<Request> GetEndTomorrow()
    {
        DateTime tomorrow = _dateDto.From.AddDays(1).Date;

        return _requests
            .Where(r => r.Services.
                Any(s => s is { InformationServiceType: InformationServiceType.AirTicket } &&
                         s.Flights.Any(f => f.FlightsType == FlightsType.Charter)))
            .Where(r =>
            {
                InformationServices airTicketService = r.Services.
                    First(s => s.InformationServiceType == InformationServiceType.AirTicket);

                IEnumerable<Flights> charterFlights = airTicketService.Flights
                    .Where(f => f.FlightsType == FlightsType.Charter);

                IEnumerable<Flights> flightsWithDates = charterFlights
                    .Where(f => f.DateBeginAsDate is not null)
                    .ToList();

                var flightsWithLaterDates = flightsWithDates
                    .Where(f1 =>
                        flightsWithDates
                            .Any(f2 => f2.DateBeginAsDate?.AddDays(2) < f1.DateBeginAsDate))
                    .ToList();

                if (flightsWithLaterDates.Count == 0) return false;

                Flights? minLaterFlight = flightsWithLaterDates.MinBy(f => f.DateBeginAsDate);

                return minLaterFlight?.DateBeginAsDate == tomorrow;
            });
    }
}