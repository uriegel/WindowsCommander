namespace Commander.ProgressAction;

class ProgressRunningControl
{
    public virtual bool IsRunning() => ProgressRunning.IsRunning;

    public virtual void SetRunning(bool running = true) => ProgressRunning.SetRunning(running);
}