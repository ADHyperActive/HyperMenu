namespace MalumMenu.routines
{
	public abstract class IRoutine
	{
		public virtual string RoutineName { get; set; } = "";
		public virtual bool Enabled { get; set; } = false;

		public virtual void Run() { }
	}
}