using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Cosockets
{
    class Cosocket
    {
        protected delegate object ResultGetter(SocketAsyncEventArgs e);

        protected Socket socket = null;
        protected HashSet<uint> waiting = null;
        protected uint token;

        protected Cosocket()
        {
            this.waiting = new HashSet<uint>();
            this.token = uint.MinValue;
        }

        public Cosocket(Socket socket)
            : this()
        {
            this.socket = socket;
        }

        public Cosocket(SocketInformation info)
            : this()
        {
            this.socket = new Socket(info);
        }

        public Cosocket(AddressFamily family, SocketType sockType, ProtocolType protType)
            : this()
        {
            this.socket = new Socket(family, sockType, protType);
        }

        // Creates a new SocketAsyncEventArgs object and append the
        // EventHandler to call Cosocket.Callback. A SocketAsyncEventArgs will
        // have own unique token.
        protected SocketAsyncEventArgs EventArgs()
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(this.Callback);
            lock ((object)this.token)
            {
                e.UserToken = this.token++;
            }
            return e;
        }

        protected void Callback(object socket, SocketAsyncEventArgs e)
        {
            uint token = (uint)e.UserToken;
            this.waiting.Remove(token);
        }

        protected IEnumerator Spawn(Func<SocketAsyncEventArgs, bool> sockMethod, SocketAsyncEventArgs e, ResultGetter getResult)
        {
            if (!sockMethod(e))
            {
                // complete immediately
                if (getResult != null)
                {
                    yield return getResult(e);
                }
                yield break;
            }
            uint token = (uint)e.UserToken;
            this.waiting.Add(token);
            while (true)
            {
                if (this.waiting.Contains(token))
                {
                    yield return null;
                }
                else
                {
                    if (getResult != null)
                    {
                        yield return getResult(e);
                    }
                    yield break;
                }
            }
        }

        protected IEnumerator Spawn(Func<SocketAsyncEventArgs, bool> sockMethod, SocketAsyncEventArgs e)
        {
            return this.Spawn(sockMethod, e, null);
        }

        // Socket.Accept wrapper

        public IEnumerator Accept()
        {
            SocketAsyncEventArgs e = this.EventArgs();
            ResultGetter returnAcceptSocket = new ResultGetter(delegate(SocketAsyncEventArgs _e)
            {
                return new Cosocket(_e.AcceptSocket);
            });
            return this.Spawn(this.socket.AcceptAsync, e, returnAcceptSocket);
        }

        // Socket.Connect

        public IEnumerator Connect(EndPoint remoteEP)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.RemoteEndPoint = remoteEP;
            return this.Spawn(this.socket.ConnectAsync, e);
        }

        public IEnumerator Connect(IPAddress address, int port)
        {
            return this.Connect(new IPEndPoint(address, port));
        }

        // Socket.Disconnect

        public IEnumerator Disconnect(bool reuseSocket = false)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.DisconnectReuseSocket = reuseSocket;
            return this.Spawn(this.socket.DisconnectAsync, e);
        }

        // Socket.Send

        public IEnumerator Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags = SocketFlags.None)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.BufferList = buffers;
            e.SocketFlags = socketFlags;
            return this.Spawn(this.socket.SendAsync, e);
        }

        public IEnumerator Send(byte[] buffer, SocketFlags socketFlags = SocketFlags.None)
        {
            return this.Send(buffer, buffer.Length, socketFlags);
        }

        public IEnumerator Send(byte[] buffer, int size, SocketFlags socketFlags = SocketFlags.None)
        {
            return this.Send(buffer, 0, size, socketFlags);
        }

        public IEnumerator Send(byte[] buffer, int offset, int size, SocketFlags socketFlags = SocketFlags.None)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.SetBuffer(buffer, offset, size);
            e.SocketFlags = socketFlags;
            return this.Spawn(this.socket.SendAsync, e);
        }

        // Socket.SendTo

        public IEnumerator SendTo(byte[] buffer, EndPoint remoteEP)
        {
            return this.SendTo(buffer, SocketFlags.None, remoteEP);
        }

        public IEnumerator SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return this.SendTo(buffer, buffer.Length, socketFlags, remoteEP);
        }

        public IEnumerator SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return this.SendTo(buffer, 0, size, socketFlags, remoteEP);
        }

        public IEnumerator SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.SetBuffer(buffer, offset, size);
            e.SocketFlags = socketFlags;
            return this.Spawn(this.socket.SendToAsync, e);
        }

        // Socket.Receive

        public IEnumerator Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags = SocketFlags.None)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.BufferList = buffers;
            e.SocketFlags = socketFlags;
            return this.Spawn(this.socket.ReceiveAsync, e);
        }

        public IEnumerator Receive(byte[] buffer, SocketFlags socketFlags = SocketFlags.None)
        {
            return this.Receive(buffer, buffer.Length, socketFlags);
        }

        public IEnumerator Receive(byte[] buffer, int size, SocketFlags socketFlags = SocketFlags.None)
        {
            return this.Receive(buffer, 0, size, socketFlags);
        }

        public IEnumerator Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags = SocketFlags.None)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.SetBuffer(buffer, offset, size);
            e.SocketFlags = socketFlags;
            return this.Spawn(this.socket.ReceiveAsync, e);
        }

        // Socket.ReceiveFrom

        public IEnumerator ReceiveFrom(byte[] buffer, EndPoint remoteEP)
        {
            return this.ReceiveFrom(buffer, SocketFlags.None, remoteEP);
        }

        public IEnumerator ReceiveFrom(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return this.ReceiveFrom(buffer, buffer.Length, socketFlags, remoteEP);
        }

        public IEnumerator ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return this.ReceiveFrom(buffer, 0, size, socketFlags, remoteEP);
        }

        public IEnumerator ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            SocketAsyncEventArgs e = this.EventArgs();
            e.SetBuffer(buffer, offset, size);
            e.SocketFlags = socketFlags;
            return this.Spawn(this.socket.ReceiveFromAsync, e);
        }

        // pass methods

        public void Bind(EndPoint localEP)
        {
            this.socket.Bind(localEP);
        }

        public void Close(int timeout = 0)
        {
            this.socket.Close(timeout);
        }

        public void Listen(int backlog)
        {
            this.socket.Listen(backlog);
        }

        public void Shutdown(SocketShutdown how)
        {
            this.socket.Shutdown(how);
        }
    }
}