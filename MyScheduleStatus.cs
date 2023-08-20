namespace YellowHouse.N3rgy;

public record TimerScheduleStatus(
    DateTime Last,
    DateTime Next,
    DateTime LastUpdated);
