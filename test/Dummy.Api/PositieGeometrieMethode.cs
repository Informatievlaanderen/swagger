namespace Dummy.Api
{
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// De Geometrie methode van de positie.
    /// </summary>
    [DataContract(Name = "PositieGeometrieMethode", Namespace = "")]
    public enum PositieGeometrieMethode
    {
        /// <summary>
        /// Aangeduid door de beheerder.
        /// </summary>
        [EnumMember]
        AangeduidDoorBeheerder = 1,

        /// <summary>
        /// Afgeleid van een object.
        /// </summary>
        [EnumMember]
        AfgeleidVanObject = 2,

        /// <summary>
        /// De positie is geïnterpoleerd.
        /// </summary>
        [EnumMember(Value = "Geïnterpoleerd")]
        [XmlEnum(Name = "Geïnterpoleerd")]
        Geinterpoleerd = 3
    }
}
