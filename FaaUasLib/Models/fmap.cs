using System;
using System.Collections.Generic;
using System.Text;

namespace FaaUasLib.Models
{
    public class FascilityMap
    {
        public string objectIdFieldName { get; set; }
        public Uniqueidfield uniqueIdField { get; set; }
        public string globalIdFieldName { get; set; }
        public Geometryproperties geometryProperties { get; set; }
        public string geometryType { get; set; }
        public Spatialreference spatialReference { get; set; }
        public Field[] fields { get; set; }
        public Feature[] features { get; set; }
    }

    public class Uniqueidfield
    {
        public string name { get; set; }
        public bool isSystemMaintained { get; set; }
    }

    public class Geometryproperties
    {
        public string shapeAreaFieldName { get; set; }
        public string shapeLengthFieldName { get; set; }
        public string units { get; set; }
    }

    public class Spatialreference
    {
        public int wkid { get; set; }
        public int latestWkid { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public string type { get; set; }
        public string alias { get; set; }
        public string sqlType { get; set; }
        public object domain { get; set; }
        public object defaultValue { get; set; }
        public int length { get; set; }
    }

    public class Feature
    {
        public Attributes attributes { get; set; }
        public Geometry geometry { get; set; }
    }

    public class Attributes
    {
        public int OBJECTID { get; set; }
        public int CEILING { get; set; }
        public string UNIT { get; set; }
        public string MAP_EFF { get; set; }
        public string LAST_EDIT { get; set; }
        public float LATITUDE { get; set; }
        public float LONGITUDE { get; set; }
        public string GLOBALID { get; set; }
        public int ARPT_COUNT { get; set; }
        public string APT1_FAAID { get; set; }
        public string APT1_ICAO { get; set; }
        public string APT1_NAME { get; set; }
        public int APT1_LAANC { get; set; }
        public string APT2_FAAID { get; set; }
        public string APT2_ICAO { get; set; }
        public string APT2_NAME { get; set; }
        public int? APT2_LAANC { get; set; }
        public string APT3_FAAID { get; set; }
        public string APT3_ICAO { get; set; }
        public string APT3_NAME { get; set; }
        public object APT3_LAANC { get; set; }
        public string APT4_FAAID { get; set; }
        public string APT4_ICAO { get; set; }
        public string APT4_NAME { get; set; }
        public object APT4_LAANC { get; set; }
        public string APT5_FAAID { get; set; }
        public string APT5_ICAO { get; set; }
        public string APT5_NAME { get; set; }
        public object APT5_LAANC { get; set; }
        public int AIRS_COUNT { get; set; }
        public string AIRSPACE_1 { get; set; }
        public string AIRSPACE_2 { get; set; }
        public string AIRSPACE_3 { get; set; }
        public string AIRSPACE_4 { get; set; }
        public string AIRSPACE_5 { get; set; }
        public string REGION { get; set; }
        public float Shape__Area { get; set; }
        public float Shape__Length { get; set; }
    }

    public class Geometry
    {
        public float[][][] rings { get; set; }
    }
}
