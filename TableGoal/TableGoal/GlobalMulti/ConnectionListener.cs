using System;
using System.Net;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client;
using System.Diagnostics;

namespace TableGoal
{
    public class ConnectionListener : com.shephertz.app42.gaming.multiplayer.client.listener.ConnectionRequestListener
    {
        public delegate void OnConnectionDoneEventHandler();
        public event OnConnectionDoneEventHandler OnConnectionDone;

        public delegate void RecoverableConnectionProblemEventHandler();
        public event RecoverableConnectionProblemEventHandler RecoverableConnectionProblem;

        public delegate void ConnectionRecoveredEventHandler();
        public event ConnectionRecoveredEventHandler ConnectionRecovered;

        public delegate void SeriousConnectionProblemEventHandler();
        public event SeriousConnectionProblemEventHandler SeriousConnectionProblem;

        public ConnectionListener()
        { }

        public void onConnectDone(ConnectEvent eventObj)
        {
            switch (eventObj.getResult())
            {
                case WarpResponseResultCode.SUCCESS:
                    Debug.WriteLine(String.Format("Connect done - code {0}", eventObj.getResult()));
                    GlobalMultiProvider.IsConnected = true;
                    OnConnectionDoneEventHandler handler = this.OnConnectionDone;
                    if (handler != null)
                    {
                        handler();
                    }
                    break;
                case WarpResponseResultCode.CONNECTION_ERROR_RECOVERABLE:
                    Debug.WriteLine("Connection error recoverable");
                    RecoverableConnectionProblemEventHandler handlerRecoverableProblem = this.RecoverableConnectionProblem;
                    if (handlerRecoverableProblem != null)
                    {
                        handlerRecoverableProblem();
                    }
                    GlobalMultiProvider.IsConnected = false;
                    WarpClient.GetInstance().RecoverConnection();
                    break;
                case WarpResponseResultCode.SUCCESS_RECOVERED:
                    Debug.WriteLine("Success recovered");
                    ConnectionRecoveredEventHandler handlerConnectionRecovered = this.ConnectionRecovered;
                    if (handlerConnectionRecovered != null)
                    {
                        handlerConnectionRecovered();
                    }
                    GlobalMultiProvider.IsConnected = true;
                    break;
                default:
                    Debug.WriteLine(String.Format("onConnectDone PROBLEM - code {0}", eventObj.getResult()));
                    GlobalMultiProvider.IsConnected = false;
                    SeriousConnectionProblemEventHandler handlerSeriousProblem = this.SeriousConnectionProblem;
                    if (handlerSeriousProblem != null)
                    {
                        handlerSeriousProblem();
                    }
                    break;
            }
        }

        public void onDisconnectDone(ConnectEvent eventObj)
        {
            switch (eventObj.getResult())
            {
                case WarpResponseResultCode.SUCCESS:
                    Debug.WriteLine(String.Format("Disconnect done - code {0}", eventObj.getResult()));
                    GlobalMultiProvider.IsConnected = false;
                    break;
                default:
                    Debug.WriteLine(String.Format("onDisconnectDone - Connection error {0}", eventObj.getResult()));
                    GlobalMultiProvider.IsConnected = false;
                    break;
            }
        }

        public void onInitUDPDone(byte resultCode)
        { }
    }
}
