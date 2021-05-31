using bm472.Data;
using System.Windows;
using bm472.Network;
using System.ComponentModel;
using ServiceStack.Text;
using System.Collections.Generic;

namespace bm472
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly PacketCapturer packetCapturer = new PacketCapturer();
        List<PacketModel> packets { get; set; } = new List<PacketModel>();

        public MainWindow()
        {
            // center window
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            // Initialize device combobox
            foreach (var d in PacketCapturer.AllDevices)
            {
                devicesComboBox.Items.Add($"{d.Description} ({d.Name})");
            }
            devicesComboBox.SelectedIndex = 0;

            // Initialize packet datagrid
            packetDataGrid.ItemsSource = packets;
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if(startButton.Content.ToString() == "Start")
            {
                // Validate Port number
                string portStr = portTextBox.Text.ToString();
                bool isNumeric = int.TryParse(portStr, out int port);
                if (!isNumeric)
                {
                    MessageBox.Show("Port must be Numeric", "OK");
                    return;
                } else if (port < 0 || port > 65535)
                {
                    MessageBox.Show("Port must be between 0 and 65535", "OK");
                    return;
                }

                // Start capturing packets on a background worker
                packetCapturer.StartCapture(devicesComboBox.SelectedItem.ToString(), portStr, (object o, ProgressChangedEventArgs args) => 
                {
                    // Deserialize packet and insert it to first row of packets list
                    packets.Insert(0, JsonSerializer.DeserializeFromString<PacketModel>(args.UserState.ToString()));

                    // Show max 10000 packets on datagrid
                    if (packets.Count >= 10000)
                    {
                        packets.RemoveAt(10000);
                    }

                    // update datargid ui
                    packetDataGrid.Items.Refresh();
                });

                startButton.Content =  "Stop";
            } else
            {
                // kill background worker
                packetCapturer.KillWorker();

                startButton.Content = "Start";
            }
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            packets.Clear();
            // update datargid ui
            packetDataGrid.Items.Refresh();
        }

        private void RecordsButtonClick(object sender, RoutedEventArgs e)
        {
            RecordsWindow recordsWindow = new RecordsWindow();
            recordsWindow.Show();
        }
    }
}
