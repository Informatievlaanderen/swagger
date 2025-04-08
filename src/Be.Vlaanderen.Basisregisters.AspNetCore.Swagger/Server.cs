namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    public class Server
    {
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Server() { }

        public Server(string url, string description)
        {
            Url = url;
            Description = description;
        }
    }
}
