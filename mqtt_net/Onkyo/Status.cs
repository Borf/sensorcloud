namespace Onkyo
{
	public partial class Receiver
	{
		public struct Status
		{
			public PlayStatus playStatus { get; private set; }
			public RepeatStatus repeatStatus { get; private set; }
			public ShuffleStatus shuffleStatus { get; private set; }

			public Status(string data)
			{
				playStatus = PlayStatus.Stopped;
				repeatStatus = RepeatStatus.None;
				shuffleStatus = ShuffleStatus.No;
				switch (data[0])
				{
					case 'p': playStatus = PlayStatus.Paused; break;
					case 'P': playStatus = PlayStatus.Playing; break;
					case '-': playStatus = PlayStatus.Stopped; break;
				}
				switch (data[1])
				{
					case '-': repeatStatus = RepeatStatus.None; break;
					case '1': repeatStatus = RepeatStatus.One; break;
					case 'R': repeatStatus = RepeatStatus.All; break;
				}
				switch (data[2])
				{
					case '-': shuffleStatus = ShuffleStatus.No; break;
					case 'S': shuffleStatus = ShuffleStatus.Yes; break;
				}
			}
		}
	}
}
