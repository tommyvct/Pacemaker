using DisableDevice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacemaker
{
    class Program
    {
        static readonly string homeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pacemaker");
        static string conf = Path.Combine(homeDir, "conf.txt");

        static async Task Main(string[] args)
        {
            if (args.Length >= 1)
            {
                if (args[0] == "NUKE")
                {
                    try
                    {
                        File.Delete(conf);
                        await Dialogue("NUKE", "conf file deleted.");
                    }
                    catch (Exception e)
                    {
                        await Dialogue("Cannot delete conf file", e.ToString());
                        Environment.Exit(1);
                    }

                    Environment.Exit(0);
                }
                else
                {
                    conf = args[1];
                }
            }

            var list = ParseConfFile(conf);

            if (list.Length == 0)
            {
                await Dialogue("Empty conf file", "The conf file seemf empty.\n Consider launch with argument NUKE to delete the conf file,\nthen launch again to create a new file and reconfigure.");
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (var dev in list)
                {
                    if (dev.Length < 2)
                    {
                        continue;
                    }

                    if (dev[0] == "PWM")
                    {
                        // TODO: PWM
                        _ = PWMHelper.Program.PWM(new string[] { dev[1] }); // set freq
                        continue;
                    }
                    else
                    {
                        RestartDevice(dev[0], dev[1]);
                    }
                }
            }
        }

        public static async Task Toast(string title, string content)
        {
            // https://stackoverflow.com/a/34956412
            var notification = new System.Windows.Forms.NotifyIcon()
            {
                Visible = false,
                Icon = null,
                BalloonTipTitle = title,
                BalloonTipText = content,
            };

            await Task.Run(() => notification.ShowBalloonTip(5000));
            
            notification.Dispose();
        }

        static int GetDeviceStatus(string PnPDeviceID)
        {
            // credit: https://www.codeproject.com/Articles/30031/Query-hardware-device-status-in-C
            string query = @"select * from Win32_PnPEntity  where PnPDeviceID='" + PnPDeviceID + @"'";

            ManagementObjectCollection result = new ManagementObjectSearcher(query).Get();

            if (result != null && result.Count == 1)
            {
                foreach (var dev in result)
                {
                    return Convert.ToInt32(dev.GetPropertyValue("ConfigManagerErrorCode").ToString());
                }
            }
            else
            {
                throw new Exception("Why are there more than one device? Or no Device at all?");
            }
            
            return -999;
        }

        static string[][] ParseConfFile(string conf)
        {
            if (!File.Exists(conf))
            {
                //Console.WriteLine(e);
                

                if (!YesNoDialogue("New config", "Would you like to create a new config file?"))
                {
                    Environment.Exit(1);
                }

                try
                {
                    if (!Directory.Exists(homeDir))
                    {
                        Directory.CreateDirectory(homeDir);
                    }

                    File.Create(conf).Dispose();
                    using (StreamWriter outputFile = new StreamWriter(conf))
                    {
                        outputFile.WriteLine(@"####### PWM frequency Configuration #########");
                        outputFile.WriteLine(@"# change 1200 to desired frequency");
                        outputFile.WriteLine(@"# PWM 1200");
                        outputFile.WriteLine(@"#######        Device Restart       #########");
                        outputFile.WriteLine(@"# format: {GUID} instanceID");
                        outputFile.WriteLine(@"# Example: {4d36e972-e325-11ce-bfc1-08002be10318} PCI\VEN_168C&DEV_0042&SUBSYS_403517AA&REV_30\4&1D6086F3&0&00E0");

                    }
                    Process.Start(conf);
                    Task.Run(() => Dialogue("Pacemaker", $"New conf will be at {conf}. Please configure according the example in conf file before use."));

                    //Console.WriteLine("New conf will be at " + conf + ". Please configure according the example in conf file before use.");
                    Environment.Exit(0);
                }
                catch (Exception ee)
                {
                    //Console.WriteLine("ERROR: Cannot create conf file.");
                    //Console.WriteLine(ee);
                    Task.Run(() => Dialogue("Pacemaker", $"cannot create conf file.\n{ee}"));


                    //Console.WriteLine("Abort.");
                    //Console.WriteLine("ENTER to continue...");
                    //Console.ReadLine();
                    Environment.Exit(1);
                }
            }

            string line;
            List<string[]> ret = new List<string[]>();
            StreamReader r = new StreamReader(conf);
            
            while ((line = r.ReadLine()) != null)
            {
                if (line == "")
                {
                    continue;
                }
                if (line[0] == '#')
                {
                    continue;
                }

                //                 WTF is this? NM$L
                ret.Add(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)); 
            }

            return ret.ToArray();
        }

        static void RestartDevice(string classGUID, string instanceID)
        {
            string deviceID = instanceID.Replace(@"\", @"\\");

            int i = 0;
            while (0 != GetDeviceStatus(deviceID))
            {
                if (i >= 3)
                {
                    //Console.WriteLine($"failed to restart device.");
                    Task.Run(() => Dialogue("Pacemaker", $"failed to restart device {deviceID}."));
                    return;
                }

                //Console.WriteLine("device went wrong, fixing...");

                try
                {
                    DeviceHelper.SetDeviceEnabled(new Guid(classGUID), instanceID, false);
                    DeviceHelper.SetDeviceEnabled(new Guid(classGUID), instanceID, true);
                }
                catch (IndexOutOfRangeException)
                {
                    //Console.WriteLine("incorrect GUID or instance ID.");
                    Task.Run(() => Dialogue("Pacemaker", $"incorrect GUID or instance ID {deviceID}."));
                    return;
                }
                
                i++;
            }

            //Console.WriteLine("fixed.");
        }

        static bool YesNoDialogue(string title, string content)
        {
            return MessageBox.Show(content, title, MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        static async Task Dialogue(string title, string content)
        {
            await Task.Run(() => MessageBox.Show(content, title));
        }
    }
}
