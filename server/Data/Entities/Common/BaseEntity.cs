using System;
using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Entities.Common
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Timestamp]
        public byte[] TimeStamp { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}