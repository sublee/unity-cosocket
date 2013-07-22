using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Cosockets;

public class ClientBehaviour : MonoBehaviour
{
    IEnumerator Start ()
    {
        Cosocket sock = new Cosocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        yield return StartCoroutine(sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8999)));
        yield return StartCoroutine(sock.Send(new ASCIIEncoding().GetBytes("Hello, world")));
    }
}
