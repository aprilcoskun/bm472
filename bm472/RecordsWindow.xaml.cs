using bm472.Data;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Threading.Tasks;

namespace bm472
{
    /// <summary>
    /// Interaction logic for RecordsWindow.xaml
    /// </summary>
    public partial class RecordsWindow : Window
    {
        // initialize database(it's thread safe, so we can use different database instance on different windows)
        readonly PacketDatabase database = new PacketDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "packets.db"));

        public RecordsWindow()
        {
            // center window
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();


            // initialize search type combobox
            searchComboBox.Items.Add("Source");
            searchComboBox.Items.Add("Destination Port");
            searchComboBox.Items.Add("Protocol");
            searchComboBox.SelectedIndex = 0;

            // Set max date selection to current date
            startDatePicker.DisplayDateEnd = DateTime.Now;
            endDatePicker.DisplayDateEnd = DateTime.Now.AddDays(1);

            // get all packet records from database
            Task<List<PacketModel>> packetsQuery = database.GetAllPackets();
            packetsQuery.Wait();

            // initialize packet datagrid
            searchPacketDataGrid.ItemsSource = packetsQuery.Result;
        }

        private void searchButtonClick(object sender, RoutedEventArgs e)
        {
            Nullable<DateTime> selectedStartDate = startDatePicker.SelectedDate;
            Nullable<DateTime> selectedEndDate = endDatePicker.SelectedDate;

            // Throw error on invalid date range
            if ((selectedStartDate == null && selectedEndDate != null) || (selectedStartDate != null && selectedEndDate == null))
            {
                MessageBox.Show("Date Range is invalid", "OK");
                return;
            }

            Task<List<PacketModel>> packetsQuery;

            //Pick right sql query for search
            if (selectedStartDate != null && selectedEndDate != null)
            {
                switch (searchComboBox.SelectedItem)
                {
                    case "Source":
                        packetsQuery = database.GetPacketsBySourceAndTimeRange(searchTextBox.Text, selectedStartDate.Value, selectedEndDate.Value);
                        break;
                    case "Destination Port":
                        packetsQuery = database.GetPacketsByDestPortAndTimeRange(searchTextBox.Text, selectedStartDate.Value, selectedEndDate.Value);
                        break;
                    case "Protocol":
                        packetsQuery = database.GetPacketsByProtocolAndTimeRange(searchTextBox.Text, selectedStartDate.Value, selectedEndDate.Value);
                        break;
                    default:
                        packetsQuery = database.GetPacketsByTimeRange(selectedStartDate.Value, selectedEndDate.Value);
                        break;
                }
            }
            else
            {
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

            // execute query task
            packetsQuery.Wait();

            // update data grid
            searchPacketDataGrid.ItemsSource = packetsQuery.Result;
            searchPacketDataGrid.Items.Refresh();
        }


       private void StartDateChanged(object sender, SelectionChangedEventArgs e)
        {

            // change max end date based on start date selection
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
