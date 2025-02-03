using RomanTourNotification.Application.Models.Users;
using Telegram.Bot;

namespace RomanTourNotification.Presentation.TelegramBot.ChainOfResponsibilities;

public record HandlerContext(User User, string Message, ITelegramBotClient BotClient);