using SDK;
using SDK.Instances;

namespace Modules;

public class FocusSession : ProtocolInstance
{

	private DateTime _startTime;
	private TimeSpan _duration;
	private TextLabel _timeLabel;

	public override void Open()
	{
		CanClientClose = false;
		_startTime = DateTime.Now;
		_duration = TimeSpan.FromMinutes(1);
		
		_timeLabel = new();
		Root.AddChild(_timeLabel);
	}

	public override void Loop()
	{
		var elapsed = DateTime.Now - _startTime;
		var timeLeft = _duration - elapsed;
		_timeLabel.Text = $"Time left: {timeLeft:g}";
		
		if (timeLeft <= TimeSpan.Zero) Core.Close(this);
	}

}