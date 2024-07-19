using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LightDeploy.Server.Core;

public class DeployContext:INotifyPropertyChanged
{
    public bool IsRunning { get; set; }

    public bool CanCanel { get; set; } = true;
        
    public CancellationTokenSource? CancellationTokenSource=new CancellationTokenSource();

    public void Reset()
    {
        IsRunning = false;
        CanCanel = true;
        ResetCancelToken();
    }
    void ResetCancelToken()
    {
        if (CancellationTokenSource == null) CancellationTokenSource = new CancellationTokenSource();
        else
        {
            var success=CancellationTokenSource.TryReset();
            if (!success)
            {
                CancellationTokenSource.Dispose();
                CancellationTokenSource = new CancellationTokenSource();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}