using System;

class TimeEntry
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; set; }
    public string WorkDescription { get; }
    public bool Billable { get; }

    public TimeEntry(DateTime startTime, string workDescription, bool billable)
    {
        StartTime = startTime;
        WorkDescription = workDescription;
        Billable = billable;
    }

    public override string ToString()
    {
        return $"{StartTime:yyyy-MM-dd HH:mm},{EndTime:yyyy-MM-dd HH:mm},{WorkDescription},{(Billable ? "Yes" : "No")}";
    }
}
