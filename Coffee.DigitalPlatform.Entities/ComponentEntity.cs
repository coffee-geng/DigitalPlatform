using Coffee.DigitalPlatform.Entities.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Coffee.DigitalPlatform.Entities
{
    public class ComponentEntity
    {
        [Column(name: "icon")]
        public string? Icon { get; set; }

        [Column(name: "header")]
        public string? Label { get; set; }

        [Column(name: "target_type")]
        public string? TargetType { get; set; }

        [Column(name: "w")]
        private string _width { get; set; }
        [NotMapped]
        public int Width
        { 
            get => _intConverter.ConvertFromDatabase(_width);
            set => _width = _intConverter.ConvertToDatabase(value);
        }

        [Column(name: "h")]
        private string _height { get; set; }
        [NotMapped]
        public int Height
        {
            get => _intConverter.ConvertFromDatabase(_height);
            set => _height = _intConverter.ConvertToDatabase(value);
        }

        [Column(name: "category")]
        public string? Category
        { 
            get; 
            set; 
        }

        private static readonly IntToStringConverter _intConverter = new();
    }
}
