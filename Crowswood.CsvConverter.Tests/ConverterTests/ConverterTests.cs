using System.Xml.Serialization;

namespace Crowswood.CsvConverter.Tests.ConverterTests
{
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void TypelessJustTrackTest()
        {
            var text = @"
Properties,Connection,Name,            End1, End2, Exit1,Exit2,Group,IsRouteEndpoint
Values,    Connection,""Piece of Track"",""TE1"",""TE2"",0,    0,    """",   false

Properties,TrackNode,Id,   Name,  NodeType,  Address,Group,Orientation,PositionX,PositionY,IsTrapPoint
Values,    TrackNode,""TE1"",""TE 1"",""TrackEnd"","""",     """",   """",         0,        0,        false
Values,    TrackNode,""TE2"",""TE 2"",""TrackEnd"","""",     """",   """",         0,        0,        false
";
            var options =
                new Options()
                    .ForType("Connection", "Name", "End1", "End2", "Exit1", "Exit2", "Group", "IsRouteEndpoint")
                    .ForType("TrackNode", "Id", "Name", "NodeType", "Address", "Group", "Orientation", "PositionX", "PosistionY", "IsTrapPoint");
            var converter = new Converter(options);

            var data = converter.Deserialize(text);

            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void TypedJustTrackTest()
        {
            var text = @"
Properties,Connection,Name,            End1, End2, Exit1,Exit2,Group,IsRouteEndpoint
Values,    Connection,""Piece of Track"",""TE1"",""TE2"",0,    0,    """",   false

Properties,TrackNode,Id,   Name,  NodeType,  Address,Group,Orientation,PositionX,PositionY,IsTrapPoint
Values,    TrackNode,""TE1"",""TE 1"",""TrackEnd"","""",     """",   """",         0,        0,        false
Values,    TrackNode,""TE2"",""TE 2"",""TrackEnd"","""",     """",   """",         0,        0,        false
";
            var options =
                new Options()
                    .ForType<Connection>()
                    .ForType<FeatureNode>()
                    .ForType<TrackNode>();
            var converter = new Converter(options);

            var data = converter.Deserialize<BaseEntity>(text);

            Assert.IsNotNull(data);
        }

        #region Model class

        public class BaseEntity
        {
            [XmlElement(nameof(Group))]
            public string Group { get; set; } = string.Empty;

            [XmlElement(nameof(Name))]
            public string Name { get; set; } = string.Empty;
        }

        public class Connection : BaseEntity
        {
            [XmlElement(nameof(End1))]
            public string End1 { get; set; } = string.Empty;

            [XmlElement(nameof(End2))]
            public string End2 { get; set; } = string.Empty;

            [XmlElement(nameof(EnforceEndpoint))]
            public bool EnforceEndpoint { get; set; }

            [XmlElement(nameof(Exit1))]
            public int? Exit1 { get; set; }

            [XmlElement(nameof(Exit2))]
            public int? Exit2 { get; set; }

            [XmlElement(nameof(IsRouteEndpoint))]
            public bool IsRouteEndpoint { get; set; }
        }

        public class BaseEntityNode : BaseEntity
        {
            [XmlElement(nameof(NodeType))]
            public string NodeType { get; set; } = string.Empty;

            [XmlElement(nameof(Id))]
            public string Id { get; set; } = string.Empty;

            [XmlElement(nameof(Address))]
            public string Address { get; set; } = string.Empty;

            [XmlElement(nameof(Orientation))]
            public string Orientation { get; set; } = string.Empty;

            [XmlElement(nameof(PositionX))]
            public int PositionX { get; set; }

            [XmlElement(nameof(PositionY))]
            public int PositionY { get; set; }
        }

        public class FeatureNode : BaseEntityNode
        {
            [XmlElement(nameof(SubType))]
            public string SubType { get; set; } = string.Empty;
        }

        public class TrackNode : BaseEntityNode
        {
            public static TrackNode Undefined { get; } = new();

            [XmlElement(nameof(IsTrapPoint))]
            public bool IsTrapPoint { get; set; } = false;
        }

        #endregion
    }
}
