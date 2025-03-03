using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVCSMBackend.Models;


    public class ChargingSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("sessionId")]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("startTime")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        public string Status { get; set; } = "Charging";

        [BsonElement("energyConsumed")]
        public double EnergyConsumed { get; set; } = 0;
    }


