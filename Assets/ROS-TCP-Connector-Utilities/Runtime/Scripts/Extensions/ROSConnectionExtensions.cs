using System.Collections;
namespace Unity.Robotics.ROSTCPConnector
{
    public static class ROSConnectionExtensions
    {
        public static void Reconnect(this ROSConnection ros)
        {
            ros.Disconnect();
            ros.Connect();
        }
    }
}