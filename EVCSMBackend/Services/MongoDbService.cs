
using MongoDB.Driver;
using System.Threading.Tasks;
using EVCSMBackend.Models;

namespace EVCSMBackend.Services;
    public class MongoDbService
    {
        private readonly IMongoCollection<ChargingSession> _chargingSessions;

        public MongoDbService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("EVChargingDB");
            _chargingSessions = database.GetCollection<ChargingSession>("ChargingSessions");
        }

        public async Task StoreChargingSession(ChargingSession session)
        {
            await _chargingSessions.InsertOneAsync(session);
        }
        
        public async Task UpdateChargingSession(ChargingSession session)
        {
            var filter = Builders<ChargingSession>.Filter.Eq(s => s.Id, session.Id);
            await _chargingSessions.ReplaceOneAsync(filter, session);
        }

        public async Task<ChargingSession> GetActiveSession()
        {
            var filter = Builders<ChargingSession>.Filter.Eq(s => s.Status, "Charging");
            return await _chargingSessions.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<ChargingSession> GetLatestSession()
        {
            return await _chargingSessions.Find(_ => true)
                .SortByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();
        }
    }

