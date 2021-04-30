using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDeleteProShared
{
    public static class Utils
    {
        // Get and return current epoch since DateTime.Now.ToUnixTimeSeconds() wasn't introduced until 4.6
        // Epoch is easiest way to ensure compatability with external scripts as well
        public static int getCurrentEpoch()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
