using AmongUs.InnerNet.GameDataMessages;
using Hazel;

namespace MalumMenu
{
    internal class Network
    {
        // The PlayerControl::RpcSetScanner function does not send the RPC if visual tasks are off
        // If we want the scan animation to show up even if visual tasks are enabled, then we will need to reimplement it
        public static void SendSetScanner(bool scanning)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RpcCalls.SetScanner,
                SendOption.Reliable,
                -1
            );

            byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

            writer.Write(scanning);
            writer.Write(scanCount);

            AmongUsClient.Instance.FinishRpcImmediately(writer);

            // Render the medbay animation for ourselves
            PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);
        }

        // The PlayerControl::RpcPlayAnimation function does not send the RPC if visual tasks are off
        // If we want the task animation to show up even if visual tasks are enabled, then we will need to reimplement it
        public static void SendPlayAnimation(byte animation)
        {
            if (ShipStatus.Instance == null) return;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RpcCalls.PlayAnimation,
                SendOption.Reliable,
                -1
            );

            writer.Write(animation);

            AmongUsClient.Instance.FinishRpcImmediately(writer);

            // Render the task animation for ourselves
            PlayerControl.LocalPlayer.PlayAnimation(animation);
        }

        public static void SendDataFlag(uint netId, MessageWriter msg, int targetClientId = -1)
        {
            MessageWriter writer = MessageWriter.Get(SendOption.Reliable);

            if (targetClientId == -1)
            {
                writer.StartMessage(InnerNet.Tags.GameData);
                writer.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                writer.StartMessage(InnerNet.Tags.GameDataTo);
                writer.Write(AmongUsClient.Instance.GameId);
                writer.WritePacked(targetClientId);
            }

            writer.StartMessage((byte)GameDataTypes.DataFlag);
            writer.WritePacked(netId);
            writer.Write(msg, false);
            writer.EndMessage();

            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();
        }
    }
}