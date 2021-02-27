using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Phone.Net.NetworkInformation;

namespace TableGoal
{
    static class NetworkInterfaceHelper
    {
        public static bool IsConnectedToWiFi()
        {
            NetworkInterfaceList NetworkList = new NetworkInterfaceList();
            bool wifiAvailable = false;

            foreach (NetworkInterfaceInfo netInterfaceInfo in NetworkList)
            {
                if (netInterfaceInfo.InterfaceSubtype == NetworkInterfaceSubType.WiFi)
                    wifiAvailable = true;
            }

            return wifiAvailable;
        }

        public static bool IsInternetAvailable()
        {
            NetworkInterfaceList NetworkList = new NetworkInterfaceList();
            bool internetAvailable = false;
            int goodCounter = 0;
            int badCounter = 0;

            foreach (NetworkInterfaceInfo netInterfaceInfo in NetworkList)
            {
                switch (netInterfaceInfo.InterfaceSubtype)
                {
                    case NetworkInterfaceSubType.Cellular_1XRTT:
                        goodCounter++;
                        break;
                    case NetworkInterfaceSubType.Cellular_3G:
                        goodCounter++;
                        break;
                    case NetworkInterfaceSubType.Cellular_EDGE:
                        goodCounter++;
                        break;
                    case NetworkInterfaceSubType.Cellular_EVDO:
                        goodCounter++;
                        break;
                    case NetworkInterfaceSubType.Cellular_EVDV:
                        goodCounter++;
                        break;
                    case NetworkInterfaceSubType.Cellular_GPRS:
                        goodCounter++;
                        break;
                    case NetworkInterfaceSubType.Cellular_HSPA:
                        goodCounter++;
                        break;
                    case NetworkInterfaceSubType.Desktop_PassThru:
                        badCounter++;
                        break;
                    case NetworkInterfaceSubType.Unknown:
                        badCounter++;
                        break;
                    case NetworkInterfaceSubType.WiFi:
                        goodCounter++;
                        break;
                    default:
                        break;
                }
                if (goodCounter > 0)
                {
                    internetAvailable = true; ;
                }
                else
                {
                    if (badCounter > 0)
                    {
                        internetAvailable = false;
                    }
                }
            }
            return internetAvailable;
        }
    }
}
