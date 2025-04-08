namespace Dummy.Api
{
    using System.IO;
    using System.Net.Mime;
    using System.Xml;
    using Asp.Versioning;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [Route("v{version:apiVersion}/xml/")]
    [ApiExplorerSettings(GroupName = "Xml")]
    [Authorize]
    [Produces(AcceptTypes.Xml, MediaTypeNames.Text.Xml)]
    public class XMlController : ApiController
    {
        /// <summary>
        /// Initial entry point of the API.
        /// </summary>
        /// <remarks>Who really cares?</remarks>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(XmlResponseExamples))]
        [Produces(MediaTypeNames.Text.Xml)]
        public IActionResult GetHome()
        {
            using (var writer = new StringWriter())
            {
                new XmlResponseExamples()
                    .GetExampleDocument()
                    .Save(writer);

                return new ContentResult
                {
                    Content = writer.ToString(),
                    ContentType = MediaTypeNames.Text.Xml,
                    StatusCode = StatusCodes.Status200OK
                };

            }

        }
    }

    public class XmlResponseExamples : IExamplesProvider<XmlElement>
    {
        public XmlElement GetExamples()
            => GetExampleDocument().DocumentElement!;

        internal XmlDocument GetExampleDocument()
        {
            var example = new XmlDocument();
            example.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
    <id>https://api.basisregisters.dev-vlaanderen.be/v1/feeds/gebouwen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'gebouwen' en 'gebouweenheden'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resources 'gebouwen' en 'gebouweenheden'.</subtitle>
    <generator uri=""https://basisregisters.dev-vlaanderen.be"" version=""2.2.23.2"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-09-18T06:25:34Z</updated>
    <author>
        <name>agentschap Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/gebouwen"" rel=""self"" />
    <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/gebouwen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/gebouwen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.dev-vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.dev-vlaanderen.be/v1/feeds/gebouwen?from=1&amp;limit=1&amp;embed=event,object"" rel=""next"" />
    <entry>
        <id>0</id>
        <title>BuildingWasRegistered-0</title>
        <updated>2011-05-18T19:59:07+02:00</updated>
        <published>2011-05-18T19:59:07+02:00</published>
        <author>
            <name>Gemeente</name>
        </author>
        <category term=""gebouwen"" />
        <category term=""gebouweenheden"" />
        <content>
            <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><BuildingWasRegistered><BuildingId>b9a05759-f1aa-5d51-a47d-3adaf62a8b8c</BuildingId><Provenance><Timestamp>2011-05-18T17:59:07Z</Timestamp><Organisation>Municipality</Organisation><Reason>Centrale bijhouding CRAB</Reason></Provenance>
    </BuildingWasRegistered>
  </Event><Object><Id>b9a05759-f1aa-5d51-a47d-3adaf62a8b8c</Id><Identificator><Id>https://data.vlaanderen.be/id/gebouw/</Id><Naamruimte>https://data.vlaanderen.be/id/gebouw</Naamruimte><ObjectId /><VersieId>2011-05-18T19:59:07+02:00</VersieId></Identificator><GebouwStatus i:nil=""true"" /><GeometrieMethode i:nil=""true"" /><GeometriePolygoon i:nil=""true"" /><Gebouweenheden /><IsCompleet>false</IsCompleet><Creatie><Tijdstip>2011-05-18T19:59:07+02:00</Tijdstip><Organisatie>Gemeente</Organisatie><Reden>Centrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
</feed>");
            return example;
        }
    }
}
