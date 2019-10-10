using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.Threading;

namespace AwesomeApp.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            Cryoprison.Ex.Env env = new Cryoprison.Ex.Env();
            Cryoprison.iOS.JailbreakDetector jbDetect = new Cryoprison.iOS.JailbreakDetector(env, true);
            var isJailbroken = jbDetect.IsJailbroken;
            if (!isJailbroken)
            {
                UIApplication.Main(args, null, "AppDelegate");
            }
            else
            {
                Thread.CurrentThread.Abort();
            }
        }
    }
}
