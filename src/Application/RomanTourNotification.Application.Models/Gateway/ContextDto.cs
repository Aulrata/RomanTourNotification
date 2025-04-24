using System.Net;

namespace RomanTourNotification.Application.Models.Gateway;

public record ContextDto(string Stream, HttpStatusCode StatusCode);