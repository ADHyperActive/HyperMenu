using UnityEngine;

namespace MalumMenu.routines
{
	public class AutoTriggerSporesRoutine : IRoutine
	{
		public AutoTriggerSporesRoutine()
		{
			RoutineName = "AutoTriggerSpores";
		}

		public readonly float SPORE_TRIGGER_LENGTH = 5.0f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null)
			{
				Enabled = false;
                MalumMenu.notifications.Send("Trigger Spores", "Auto-Trigger Spores was disabled as you left the game.", 10);

				return;
			}

			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Enabled = false;
                MalumMenu.notifications.Send("Trigger Spores", "Auto-Trigger Spores was disabled as this option only works in The Fungle.", 10);

				return;
			}

			timeElapsed += Time.deltaTime;
			if(timeElapsed < SPORE_TRIGGER_LENGTH) return;
			timeElapsed = 0f;

			FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();
			foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
			{
				PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
			}
		}
	}
}