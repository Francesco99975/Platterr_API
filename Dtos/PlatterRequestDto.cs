namespace platterr_api.Dtos
{
    public class PlatterRequestDto
    {
        public int Id { get; set; }
        public int PlatterId { get; set; }
        public PlatterDto Platter { get; set; }
        public int FormatId { get; set; }
        public PlatterFormatDto Format { get; set; }
        public int Quantity { get; set; }

    }
}