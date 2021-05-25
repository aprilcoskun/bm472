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
            //Get all packets.
            return database.Table<PacketModel>().ToListAsync();
        }

        public Task<List<PacketModel>> GetPacketsBySource(string source)
        {
            return database.Table<PacketModel>().Where(i => i.Source.Contains(source)).ToListAsync();
        }

        public Task<List<PacketModel>> GetPacketsByDestPort(string port)
        {
            return database.Table<PacketModel>().Where(i => i.DestPort.Contains(port)).ToListAsync();
        }

        public Task<List<PacketModel>> GetPacketsByTimeRange(DateTime startDate, DateTime endDate)
        {
            return database.Table<PacketModel>().Where(i => startDate >= i.Timestamp && endDate <= i.Timestamp).ToListAsync();
        }

        public Task<int> SavePacketAsync(PacketModel packet)
        {
            // Save a new packet.
            return database.InsertAsync(packet);
        }
    }
}
