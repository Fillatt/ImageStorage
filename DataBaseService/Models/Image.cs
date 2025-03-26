namespace DataBaseService.Models
{
    public class Image
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public byte[] ImageBytes { get; set; }
    }
}
