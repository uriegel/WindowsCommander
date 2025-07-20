namespace Commander.ProgressAction;

class UacProgressRunningControl : ProgressRunningControl
{
    public override bool IsRunning() => false;

    public override void SetRunning(bool running = true) => ProgressRunning.SetRunning(running);
}
