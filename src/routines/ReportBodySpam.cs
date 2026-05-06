using UnityEngine;

namespace MalumMenu.routines
{
	public class ReportBodySpam : IRoutine
	{
		public ReportBodySpam()
		{
			RoutineName = "ReportBodySpam";
		}

		public float reportDelay = 2.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || !AmongUsClient.Instance.AmHost)
			{
                MalumMenu.notifications.Send("Report Body Spam", "Report body spammer has been disabled as you are not the host or you left the lobby.", 5);
				Enabled = false;
				return;
			}

			if(ShipStatus.Instance == null)
			{
                MalumMenu.notifications.Send("Report Body Spam", "Report body spammer has been disabled as the game must have started first.", 5);
				Enabled = false;
				return;
			}

			timeElapsed += Time.deltaTime;
			if(timeElapsed < reportDelay) return;

			PlayerControl player = Utilities.GetRandomPlayer(false, false, false, false);

			if(MeetingHud.Instance == null)
			{
				Utilities.OpenMeeting(PlayerControl.LocalPlayer, player.Data);
			}

			PlayerControl.LocalPlayer.RpcStartMeeting(player.Data);

			timeElapsed = 0f;
		}
	}
}