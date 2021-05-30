using bm472.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace bm472
{
    /// <summary>
    /// Interaction logic for RecordsWindow.xaml
    /// </summary>
    public partial class RecordsWindow : Window
    {

        readonly PacketDatabase database = new PacketDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "packets.db"));

        public RecordsWindow()
        {
            // center window
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            searchComboBox.Items.Add("Source");
            searchComboBox.Items.Add("Destination Port");
            searchComboBox.Items.Add("Protocol");
            searchComboBox.SelectedIndex = 0;

            startDatePicker.DisplayDateEnd = DateTime.Now;
            endDatePicker.DisplayDateEnd = DateTime.Now;

            var packetsQuery = database.GetAllPackets();
            packetsQuery.Wait();

            // Initialize packet datagrid
            searchPacketDataGrid.ItemsSource = packetsQuery.Result;
        }

        private void searchButtonClick(object sender, RoutedEventArgs e)
        {
            Task<List<PacketModel>> packetsQuery;

            if ((startDatePicker.SelectedDate == null && endDatePicker.SelectedDate != null)
                || (startDatePicker.SelectedDate != null && endDatePicker.SelectedDate == null))
            {
                MessageBox.Show("Date Range is invalid", "OK");
                return;
            }

            if (startDatePicker.SelectedDate != null && endDatePicker.SelectedDate != null)
            {
                switch (searchComboBox.SelectedItem)
                {
                    case "Source":
                        packetsQuery = database.GetPacketsByProtocolAndTimeRange(searchTextBox.Text, startDatePicker.SelectedDate.Value, endDatePicker.SelectedDate.Value);
                        break;
                    case "Destination Port":
                        packetsQuery = database.GetPacketsByDestPortAndTimeRange(searchTextBox.Text, startDatePicker.SelectedDate.Value, endDatePicker.SelectedDate.Value);
                        break;
                    case "Protocol":
                        packetsQuery = database.GetPacketsByProtocolAndTimeRange(searchTextBox.Text, startDatePicker.SelectedDate.Value, endDatePicker.SelectedDate.Value);
                        break;
                    default:
                        packetsQuery = database.GetPacketsByTimeRange(startDatePicker.SelectedDate.Value, endDatePicker.SelectedDate.Value);
                        break;
                }
            }
            else {
                switch (searchComboBox.SelectedItem)
                {
                    case "Source":
                        packetsQuery = database.GetPacketsBySource(searchTextBox.Text);
                        break;
                    case "Destination Port":
                        packetsQuery = database.GetPacketsByDestPort(searchTextBox.Text);
                        break;
                    case "Protocol":
                        packetsQuery = database.GetPacketsByProtocol(searchTextBox.Text);
                        break;
                    default:
                        packetsQuery = database.GetAllPackets();
                        break;
                }
            }

            

            packetsQuery.Wait();
            searchPacketDataGrid.ItemsSource = packetsQuery.Result;
            searchPacketDataGrid.Items.Refresh();
        }


       private void StartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (startDatePicker.SelectedDate != null)
            {
                DateTime maxEndDate = startDatePicker.SelectedDate.Value.AddDays(1);
                if (endDatePicker.SelectedDate != null && endDatePicker.SelectedDate < maxEndDate)
                {
                    endDatePicker.SelectedDate = maxEndDate;
                }
                endDatePicker.DisplayDateStart = maxEndDate;
            }

        }
    }
}
