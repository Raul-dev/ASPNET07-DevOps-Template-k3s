namespace CatalogApi.Data.Models
{
    public class BaseAuditableEntity
    {
        public DateTime Added { get; internal set; }
        public DateTime Modified { get; internal set; }
    }
}
