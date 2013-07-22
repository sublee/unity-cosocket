using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Cosockets;

public class ServerBehaviour : MonoBehaviour
{
    IEnumerator Start ()
    {
        Cosocket sock = new Cosocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        sock.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8999));
        sock.Listen(0);

        Cosocket clientSock = null;
        IEnumerator accepting = sock.Accept();
        while (accepting.MoveNext())
        {
            if (accepting.Current == null)
            {
                yield return null;
                continue;
            }
            clientSock = (Cosocket)accepting.Current;
        }

        byte[] buffer = new byte[256];
        yield return StartCoroutine(clientSock.Receive(buffer));
        Debug.Log("Received: " + new ASCIIEncoding().GetString(buffer));
    }
}
