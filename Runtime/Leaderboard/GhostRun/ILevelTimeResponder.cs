namespace SpeedrunSim
{
	public interface ILevelTimeResponder
	{
		void UpdateTime(float ms);
		void RewindToStamp(int stamp);
	}
}