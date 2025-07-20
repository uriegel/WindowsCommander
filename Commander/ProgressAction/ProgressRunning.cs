namespace Commander.ProgressAction;

static class ProgressRunning
{
    public static bool IsRunning { get; private set; }

    public static void SetRunning(bool running = true) =>  IsRunning = running;

    public static bool CanClose()
    {
        if (!IsRunning)
            return true;
        else
        {
            Events.SendMenuCommand("SHOW_PROGRESS");
            return false;
        }
    }
}