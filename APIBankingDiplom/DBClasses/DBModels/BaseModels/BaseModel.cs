using APIBankingDiplom.CommonResources.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIBankingDiplom.DBClasses.DBModels.BaseClasses
{
    public abstract class BaseModel
    {
        [Key]
        public int Id { get; set; }

        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateUtility.GetCurrentTime();

        [JsonIgnore]
        public DateTime UpdatedAt { get; set; } = DateUtility.GetCurrentTime();

        // GUIDs caused issues with generating card numbers due to their immense size
        // We will use sequential IDs because of that

        // Id { get; set } = Guid.NewGuid() generates an empty Guid
        // This is a workaround
        //public BaseModel()
        //{
        //    Id = Guid.NewGuid();
        //}
    }
}
