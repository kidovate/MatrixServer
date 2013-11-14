using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MMOController.Properties;
using log4net;

namespace MMOController
{
    /// <summary>
    /// 
    /// </summary>
    public static class MmoAws
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MmoAws));
		public static IAmazonS3 AmazonS3 { get; private set; }
        private static bool initialized = false;
        
        static MmoAws()
        {
            if (initialized) return;
            initialized = true;
            log.Info("Connecting to Amazon S3...");
            try
            {
                AmazonS3 = AWSClientFactory.CreateAmazonS3Client(Settings.Default.AWSAccessKey, Settings.Default.AWSSecretKey, RegionEndpoint.USWest1);
            }catch(Exception ex)
            {
                log.Error("Failed to connect to Amazon, error: "+ex.Message);
            }
        }
    }
}
