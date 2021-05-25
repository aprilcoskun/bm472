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

            var packetsQuery = database.GetAllPackets();
            packetsQuery.Wait();

            // Initialize packet datagrid
            searchPacketDataGrid.ItemsSource = packetsQuery.Result;
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            var packetsQuery = database.GetPacketsByDestPort(searchPortTextBox.Text);
            packetsQuery.Wait();
            searchPacketDataGrid.ItemsSource = packetsQuery.Result;
            searchPacketDataGrid.Items.Refresh();
        }

        private void searchSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var packetsQuery = database.GetPacketsBySource(searchSourceTextBox.Text);
            packetsQuery.Wait();
            searchPacketDataGrid.ItemsSource = packetsQuery.Result;
            searchPacketDataGrid.Items.Refresh();
        }
    }
}
