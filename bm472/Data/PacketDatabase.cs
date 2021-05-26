using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bm472.Data
{
    public class PacketDatabase
    {
        readonly SQLiteAsyncConnection database;

        public PacketDatabase(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<PacketModel>().Wait();
        }

        public Task<List<PacketModel>> GetAllPackets()
        {
            return database.Table<PacketModel>().OrderByDescending(i => i.Timestamp).ToListAsync();
        }

        public Task<List<PacketModel>> GetPacketsBySource(string source)
        {
            return database.Table<PacketModel>().Where(i => i.Source.Contains(source)).OrderByDescending(i => i.Timestamp).ToListAsync();
        }

        public Task<List<PacketModel>> GetPacketsByDestPort(string port)
        {
            return database.Table<PacketModel>().Where(i => i.DestPort.Contains(port)).OrderByDescending(i => i.Timestamp).ToListAsync();
        }

        public Task<List<PacketModel>> GetPacketsByTimeRange(DateTime startDate, DateTime endDate)
        {
            return database.Table<PacketModel>().Where(i => startDate >= i.Timestamp && endDate <= i.Timestamp).OrderByDescending(i => i.Timestamp).ToListAsync();
        }

        public Task<int> SavePacketAsync(PacketModel packet)
        {
            // Save a new packet.
            return database.InsertAsync(packet);
        }
    }
}
