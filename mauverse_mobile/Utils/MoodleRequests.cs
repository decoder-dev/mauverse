using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using mau.DTOModels;
using mau.Models;

namespace mau.Utils;

public sealed class MoodleRequests
{
    private const string ServiceName = "moodle_mobile_app";
    private const string WebServiceEndpoint = "webservice/rest/server.php";
    private const string LoginEndpoint = "login/token.php";

    private static readonly Uri BaseUri = new("https://eios.mauniver.ru/moodle/");
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };
    private readonly HttpClient _transport = Client;

    public async Task<UserToken?> GetUserToken(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var response = await PostFormAsync<UserToken>(
            LoginEndpoint,
            new Dictionary<string, string>
            {
                ["username"] = userName,
                ["password"] = password,
                ["service"] = ServiceName
            },
            cancellationToken);
        return string.IsNullOrWhiteSpace(response?.Error) ? response : null;
    }

    public Task<User?> GetUserInfo(
        string token,
        CancellationToken cancellationToken = default) =>
        PostMoodleAsync<User>(
            token,
            "core_webservice_get_site_info",
            cancellationToken: cancellationToken);

    public async Task<List<Message>> GetNotifications(
        string token,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["type"] = "notifications",
            ["useridto"] = userId.ToString(CultureInfo.InvariantCulture),
            ["useridfrom"] = "0"
        };

        try
        {
            using var response = await PostMoodleAsync<JsonDocument>(
                token,
                "core_message_get_messages",
                parameters,
                cancellationToken);
            if (response is null)
                return [];

            var root = response.RootElement;
            if (!root.TryGetProperty("messages", out var messagesElement))
            {
                await ShowMoodleErrorAsync(root);
                return [];
            }

            var messages = messagesElement.Deserialize<List<Message>>() ?? [];
            var rawMessages = messages
                .Where(message => message.EventType != "newlogin")
                .OrderByDescending(message => message.TimeCreated);
            var notifications = new List<Message>();
            foreach (var message in rawMessages)
            {
                message.ColorName = message.UserIdFrom switch
                {
                    -10 => "#F1C187",
                    -20 => "#0B4F8A",
                    _ => "#A052BA44"
                };

                if (!string.IsNullOrEmpty(message.SmallMessage))
                {
                    var cleanedMessage = message.SmallMessage
                        .Replace("<br/>", string.Empty, StringComparison.Ordinal)
                        .Replace("<br>", string.Empty, StringComparison.Ordinal)
                        .Replace("</br>", string.Empty, StringComparison.Ordinal)
                        .Replace('\n', ' ');
                    message.SmallMessage = cleanedMessage.Length > 40
                        ? $"{cleanedMessage[..40]}..."
                        : cleanedMessage;
                }

                var createdAt = FromUnixTimeSecondsOrNow(message.TimeCreated);
                message.TimeCreatedString = $"от {createdAt.Date.ToString("d", CultureInfo.CurrentCulture)}";
                notifications.Add(message);
            }

            return notifications;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await AppShell.DisplaySnackbarAsync("Не удалось загрузить уведомления. Попробуйте позднее");
            return [];
        }
    }

    public async Task<List<RecentMoodleMessages>> GetMessages(
        string token,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["userid"] = userId.ToString(CultureInfo.InvariantCulture),
            ["limitnum"] = "5"
        };

        try
        {
            using var response = await PostMoodleAsync<JsonDocument>(
                token,
                "core_message_get_conversations",
                parameters,
                cancellationToken);
            if (response is null)
                return [];

            var root = response.RootElement;
            if (!root.TryGetProperty("conversations", out var conversationsElement))
            {
                await ShowMoodleErrorAsync(root);
                return [];
            }

            return conversationsElement.Deserialize<List<RecentMoodleMessages>>() ?? [];
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            await AppShell.DisplaySnackbarAsync(
                "Не удалось загрузить список чатов. Проверьте интернет-соединение и попробуйте еще раз");
            return [];
        }
    }

    public async Task<UserChatDTO?> GetContactToChat(
        string token,
        int userId,
        string name,
        CancellationToken cancellationToken = default)
    {
        _ = userId;
        var response = await PostMoodleAsync<List<UserChatDTO>>(
            token,
            "core_message_search_contacts",
            new Dictionary<string, string> { ["searchtext"] = name },
            cancellationToken);
        return response is { Count: > 0 } ? response[0] : null;
    }

    public async Task<List<MessageDTO>> GetChatMessages(
        string token,
        int userId,
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var response = await PostMoodleAsync<ChatMessagesDTO>(
            token,
            "core_message_get_conversation_messages",
            new Dictionary<string, string>
            {
                ["convid"] = conversationId.ToString(CultureInfo.InvariantCulture),
                ["currentuserid"] = userId.ToString(CultureInfo.InvariantCulture)
            },
            cancellationToken);
        return response?.Messages ?? [];
    }

    public async Task<MessageDTO?> SendMessage(
        string token,
        int conversationId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var response = await PostMoodleAsync<List<MessageDTO>>(
            token,
            "core_message_send_messages_to_conversation",
            new Dictionary<string, string>
            {
                ["conversationid"] = conversationId.ToString(CultureInfo.InvariantCulture),
                ["messages[0][text]"] = text
            },
            cancellationToken);
        return response is { Count: > 0 } ? response[0] : null;
    }

    private static async Task ShowMoodleErrorAsync(JsonElement root)
    {
        var message = root.TryGetProperty("message", out var messageElement)
            ? messageElement.GetString()
            : null;
        if (string.IsNullOrWhiteSpace(message) && root.TryGetProperty("errorcode", out var codeElement))
            message = codeElement.GetString();

        if (!string.IsNullOrWhiteSpace(message))
            await AppShell.DisplaySnackbarAsync($"{message}\nПерезайдите в учетную запись");
    }

    private Task<T?> PostMoodleAsync<T>(
        string token,
        string function,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        var form = parameters is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(parameters);
        form["wstoken"] = token;
        form["wsfunction"] = function;
        form["moodlewsrestformat"] = "json";
        return PostFormAsync<T>(WebServiceEndpoint, form, cancellationToken);
    }

    private async Task<T?> PostFormAsync<T>(
        string endpoint,
        Dictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(form);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(BaseUri, endpoint))
        {
            Content = content
        };
        using var response = await _transport.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private static DateTimeOffset FromUnixTimeSecondsOrNow(long value) =>
        value is >= -62_135_596_800 and <= 253_402_300_799
            ? DateTimeOffset.FromUnixTimeSeconds(value)
            : DateTimeOffset.Now;
}
