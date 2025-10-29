using SDK;
using SDK.Instances;

namespace Modules;

public class FocusSession : ProtocolInstance
{

	private DateTime _startTime;
	private TimeSpan _duration;
	private TextLabel? _timeLabel;

	public override void Open()
	{
		CanClientClose = false;
		_startTime = DateTime.Now;
		_duration = TimeSpan.FromSeconds(10);
		
		_timeLabel = new();
		Root.AddChild(_timeLabel);
	}

	public override void Loop()
	{
		var elapsed = DateTime.Now - _startTime;
		var timeLeft = _duration - elapsed;
		
		if (timeLeft > TimeSpan.Zero) {
			_timeLabel!.Text = $"Time left: {timeLeft:g}";
			return;
		}
		
		Core.Close(this);
	}

	public override void Close()
	{
		foreach (var client in Server.ConnectedClients) {
			if (client is not INotificationCapability notificationClient) continue;
			notificationClient.ShowNotification("Focus session ended", "Take a break now");
		}
	}

}