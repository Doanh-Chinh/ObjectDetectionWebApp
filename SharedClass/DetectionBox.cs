using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClass
{    
    public class DetectionBox
    {        
        [Key]
        public int DetectionBoxId { get; set; }
        public string Label { get; set; }
        public float Confidence { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public int ImageId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        // hanle bug Data is null. If you are trying to read some nullable data from the database, but your type is not nullable you can get this error.
        // reffering to: https://stackoverflow.com/questions/55883704/entity-framework-core-sqlnullvalueexception-data-is-null-how-to-troubleshoo

    }
}
