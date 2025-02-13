namespace RomanTourNotification.Application.Models.EnrichmentNotification;

public record DateDto(DateTime From, int Days = 3, bool DepartureTomorrow = true, bool ArrivalTomorrow = true);