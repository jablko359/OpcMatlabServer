// Copyright (c) Traeger Industry Components GmbH. All Rights Reserved.

using System;

namespace Server
{
    using Opc.UaFx;
    using Opc.UaFx.Server;

    /// <summary>
    /// This sample demonstrates how to implement a primitive OPC server.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting server: {0}", ServerSettings.Current.ServerAddress);
            try
            {
                OpcCertificateManager.AutoCreateCertificate = true;
                MatlabServiceType matlabType;
                Enum.TryParse(Properties.Settings.Default.Matlab, out matlabType);
                Console.WriteLine("Starting matlab as {0} service", matlabType);
                new OpcServerApplication(ServerSettings.Current.ServerAddress,
                    new MatlabNodeManager(new MatlabService(matlabType))).Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Global exception: {0} \n {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                Console.ReadLine();
            }
            
            
        }
    }
}
