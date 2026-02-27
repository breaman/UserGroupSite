using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserGroupSite.Shared.Services;

namespace UserGroupSite.Server.Services;

/// <summary>
/// Publishes events to Meetup.com using the GraphQL API at <c>https://api.meetup.com/gql-ext</c>.
/// Requires <c>MEETUP_API_TOKEN</c> and <c>MEETUP_GROUP_URLNAME</c> environment variables.
/// </summary>
public sealed class MeetupService : IMeetupService
{
    private static readonly Uri MeetupGraphQlEndpoint = new("https://api.meetup.com/gql-ext");

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MeetupService> _logger;
    private readonly string? _apiToken;
    private readonly string? _groupUrlname;

    /// <summary>Initializes a new instance of the <see cref="MeetupService"/> class.</summary>
    /// <param name="httpClientFactory">Factory for creating HTTP client instances.</param>
    /// <param name="configuration">Application configuration used to read Meetup settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public MeetupService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MeetupService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _apiToken = configuration["MEETUP_API_TOKEN"];
        _groupUrlname = configuration["MEETUP_GROUP_URLNAME"];
    }

    /// <inheritdoc />
    public async Task<string?> CreateEventAsync(
        string title,
        string description,
        DateTime startDateTimeUtc,
        string location)
    {
        if (string.IsNullOrWhiteSpace(_apiToken) || string.IsNullOrWhiteSpace(_groupUrlname))
        {
            _logger.LogWarning(
                "Meetup integration is not configured. Set MEETUP_API_TOKEN and MEETUP_GROUP_URLNAME environment variables.");
            return null;
        }

        try
        {
            var mutation = """
                mutation($input: CreateEventInput!) {
                  createEvent(input: $input) {
                    event {
                      id
                    }
                    errors {
                      message
                      code
                      field
                    }
                  }
                }
                """;

            // Meetup expects ISO 8601 date/time with offset
            var startDateTime = startDateTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var variables = new
            {
                input = new
                {
                    groupUrlname = _groupUrlname,
                    title,
                    description,
                    startDateTime,
                    publishStatus = "PUBLISHED"
                }
            };

            var payload = new
            {
                query = mutation,
                variables
            };

            var jsonContent = JsonSerializer.Serialize(payload, MeetupJsonContext.Options);

            using var httpClient = _httpClientFactory.CreateClient("Meetup");
            using var request = new HttpRequestMessage(HttpMethod.Post, MeetupGraphQlEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

            using var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Meetup API returned HTTP {StatusCode}: {ResponseBody}",
                    (int)response.StatusCode,
                    responseBody);
                return null;
            }

            var result = JsonSerializer.Deserialize<MeetupGraphQlResponse>(
                responseBody, MeetupJsonContext.Options);

            if (result?.Data?.CreateEvent?.Errors is { Count: > 0 } errors)
            {
                foreach (var error in errors)
                {
                    _logger.LogError(
                        "Meetup createEvent error — Code: {Code}, Field: {Field}, Message: {Message}",
                        error.Code,
                        error.Field,
                        error.Message);
                }

                return null;
            }

            var meetupEventId = result?.Data?.CreateEvent?.Event?.Id;

            if (string.IsNullOrWhiteSpace(meetupEventId))
            {
                _logger.LogError("Meetup createEvent returned no event ID. Response: {ResponseBody}", responseBody);
                return null;
            }

            _logger.LogInformation("Event published to Meetup with ID {MeetupEventId}.", meetupEventId);
            return meetupEventId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to Meetup.com.");
            return null;
        }
    }

    #region GraphQL Response Models

    private sealed class MeetupGraphQlResponse
    {
        [JsonPropertyName("data")]
        public MeetupData? Data { get; set; }
    }

    private sealed class MeetupData
    {
        [JsonPropertyName("createEvent")]
        public CreateEventPayload? CreateEvent { get; set; }
    }

    private sealed class CreateEventPayload
    {
        [JsonPropertyName("event")]
        public MeetupEvent? Event { get; set; }

        [JsonPropertyName("errors")]
        public List<MeetupError>? Errors { get; set; }
    }

    private sealed class MeetupEvent
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    private sealed class MeetupError
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("field")]
        public string? Field { get; set; }
    }

    #endregion
}

/// <summary>JSON serializer context for Meetup API payloads.</summary>
internal static class MeetupJsonContext
{
    /// <summary>Shared JSON serializer options for Meetup API calls.</summary>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
