namespace UserGroupSite.Client.Components;

public partial class UpcomingEvent : ComponentBase, IDisposable
{
    [Parameter] public EventDto SpeakingEvent { get; set; } = new();

    private string _navigateUrl => $"/event/{SpeakingEvent.Slug}";
    private Timer _timer;
    private int _days;
    private int _hours;
    private int _minutes;
    private int _seconds;
    private string _daysText;

    private string TimeTillEvent => $"In {_days} {_daysText} {_hours:00}:{_minutes:00}:{_seconds:00}";

    protected override void OnInitialized()
    {
        CalculateValues();
        _timer = new Timer(async _ =>
        {
            CalculateValues();
            await InvokeAsync(StateHasChanged);
        }, null, 0, 1000);
    }

    private void CalculateValues()
    {
        var utcNow = DateTime.UtcNow;
        var difference = SpeakingEvent.EventDateUtc!.Value - utcNow;
            
        _days = difference.Days;
        _hours = difference.Hours;
        _minutes = difference.Minutes;
        _seconds = difference.Seconds;
            
        _daysText = _days == 1 ? "day" : "days";
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
    }
}