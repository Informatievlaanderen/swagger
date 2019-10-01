namespace Be.Vlaanderen.Basisregisters.AspNetCore.Swagger
{
    public class Server
    {
        public string Url { get; set; }
        public string Description { get; set; }

        public Server() { }

        public Server(string url, string description)
        {
            Url = url;
            Description = description;
        }
    }
}
