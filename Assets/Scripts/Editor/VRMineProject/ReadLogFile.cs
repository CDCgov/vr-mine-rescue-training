using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ReadLogFile : MonoBehaviour {

    [MenuItem("Logs/Read Log File")]
    public static void Read()
    {
        string dataPath = Directory.GetParent(Application.dataPath).FullName;
        string path = EditorUtility.OpenFilePanel("Select log file", dataPath, "dat");
        Debug.Log(path);
        if (!string.IsNullOrEmpty(path))
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            while(br.BaseStream.Position != br.BaseStream.Length)
            {
                try
                {
                    int length = br.ReadInt32();
                    byte[] data = br.ReadBytes(length);

                    LogPacket packet = LogPacket.DecodePacket(data);
                    /*
                    if (packet.GetType() == typeof(PlayerData))
                    {
                        PlayerData player = packet as PlayerData;
                        if (player != null)
                        {							
                            Debug.Log(player.PlayerID + ", " + player.Position + ", " + player.Rotation + " @ " + player.TimeStamp);
                            //Debug.Log(player.Position);
                            //Debug.Log(player.Rotation);
                        }
                    }
                    */
                    if(packet.GetType() == typeof(StringMessageData)) //You can retrieve and parse the packet easily
                    {
                        StringMessageData message = packet as StringMessageData;
                        if(message != null)
                        {
                            Debug.Log(message.Message + " @ " + message.TimeStamp);
                        }
                    }
                    else
                    {
                        Debug.Log(packet.ToString());//Testing ToString override on the PlayerData and MobileEquipmentData classes
                    }
                }
                catch (System.Exception)
                {
                    
                }
                
                /*//int count = br.ReadInt32();
                PlayerData readPlayer = new PlayerData();
                readPlayer.ReadData(br);
                if(readPlayer != null)
                {
                    Debug.Log(readPlayer.TimeStamp);
                    Debug.Log(readPlayer.PlayerID);
                    Debug.Log(readPlayer.Position);
                    Debug.Log(readPlayer.Rotation);
                }
                */
            }
        }
    }
}
