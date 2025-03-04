namespace Webinex.ActiveRecord;

public static class ActivityRecordTypeAnalyzerSettingsExtension
{
    public static ActiveRecordTypeAnalyzerSettings IgnoreMethod(
        this ActiveRecordTypeAnalyzerSettings settings,
        Delegate @delegate)
    {
        return settings.IgnoreMethod(method => method == @delegate.Method);
    }
}