
namespace HealthCheck.Models
{
    public abstract class Service
    {
        public string Name { get; set; }
        public abstract string Type { get; }
    }
}
