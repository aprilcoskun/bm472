using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using bm472.Data;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using ServiceStack.Text;

namespace bm472.Network
{
    class PacketCapturer
    {
        // initialize database
        readonly PacketDatabase database = new PacketDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "packets.db"));

        // run capturer on background worker otherwise it will block ui thread
        private BackgroundWorker worker = new BackgroundWorker();

        public static IList<LivePacketDevice> AllDevices { get; } = LivePacketDevice.AllLocalMachine;

        public void KillWorker()
        {
            if (worker.IsBusy)
            {
                // this is a bit hardcore but sometimes just canceling would not release all resources used by worker
                worker.CancelAsync();
                worker.Dispose();
                worker = null;
                GC.Collect();
                worker = new BackgroundWorker();
            }
        }
        public void StartCapture(string selectedDeviceString, string port, ProgressChangedEventHandler callback)
        {
            // enable worker cancellation and progress report
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            // the code that will run in worker
            worker.DoWork += new DoWorkEventHandler((object o, DoWorkEventArgs e) =>
            {
                BackgroundWorker bw = o as BackgroundWorker;

                // find selected device by device name & description
                LivePacketDevice SelectedDevice = AllDevices[0];
                foreach (var d in AllDevices)
                {
                    if (selectedDeviceString == $"{d.Description} ({d.Name})")
                    {
                        SelectedDevice = d;
                        break;
                    }
                }
                PacketCommunicator communicator = SelectedDevice.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);

                // apply port filter
                BerkeleyPacketFilter filter = communicator.CreateFilter("dst port " + port);
                communicator.SetFilter(filter);

                // start listening device
                Trace.WriteLine("Listening Device: " + SelectedDevice.Description + " on PORT: " + port);
                communicator.ReceivePackets(0, packetData =>
                {
                    // break communicator loop if background worker killed
                    if (bw.CancellationPending)
                    {
                        communicator.Break();
                        Trace.WriteLine("Stopped listening Device: " + SelectedDevice.Description + " on PORT: " + port);
                        return;
                    }

                    // get packet data
                    PacketModel packet = new PacketModel();
                    packet.Length = packetData.Length;
                    packet.Timestamp = packetData.Timestamp;
                    switch (packetData.DataLink.Kind)
                    {
                        case DataLinkKind.Ethernet:
                            IpV4Layer ethernetIpv4Layer = (IpV4Layer)packetData.Ethernet.IpV4.ExtractLayer();
                            packet.Destination = ethernetIpv4Layer.Destination.ToString();
                            packet.Source = ethernetIpv4Layer.Source.ToString();
                            switch (packetData.Ethernet.IpV4.Protocol)
                            {
                                case IpV4Protocol.Tcp:
                                    packet.Protocol = "TCP";
                                    packet.SrcPort = packetData.Ethernet.IpV4.Tcp.SourcePort.ToString();
                                    packet.DestPort = packetData.Ethernet.IpV4.Tcp.DestinationPort.ToString();
                                    break;
                                case IpV4Protocol.Udp:
                                    packet.Protocol = "UDP";
                                    packet.SrcPort = packetData.Ethernet.IpV4.Udp.SourcePort.ToString();
                                    packet.DestPort = packetData.Ethernet.IpV4.Udp.DestinationPort.ToString();
                                    break;
                            }
                            break;
                        case DataLinkKind.IpV4:
                            IpV4Layer ipv4Layer = (IpV4Layer)packetData.IpV4.ExtractLayer();
                            packet.Destination = ipv4Layer.Destination.ToString();
                            packet.Source = ipv4Layer.Source.ToString();
                            switch (packetData.IpV4.Protocol)
                            {
                                case IpV4Protocol.Tcp:
                                    packet.Protocol = "TCP";
                                    packet.SrcPort = packetData.IpV4.Tcp.SourcePort.ToString();
                                    packet.DestPort = packetData.IpV4.Tcp.DestinationPort.ToString();
                                    break;
                                case IpV4Protocol.Udp:
                                    packet.Protocol = "UDP";
                                    packet.SrcPort = packetData.IpV4.Udp.SourcePort.ToString();
                                    packet.DestPort = packetData.IpV4.Udp.DestinationPort.ToString();
                                    break;
                            }
                            break;
                    }

                    // Save packet to sqlite database
                    database.SavePacketAsync(packet);

                    /*
                        Background worker can't send messages to main thread but there is a ReportProgress method
                        which can send and progress integer and progress state string. We used userState string 
                        to send packets to main thread and since worker can only send string, we had to serialize
                        packets as json string and deserialize them in main thread
                    */
                    bw.ReportProgress(0, JsonSerializer.SerializeToString(packet));
                });

            });
            // register ReportProgress callback from main thread
            worker.ProgressChanged += new ProgressChangedEventHandler(callback);

            worker.RunWorkerAsync();
        }
    }
}
