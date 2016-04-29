using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientMQTT
{
    class MessageConfig
    {
        public string CommandType { get; set; }
        public string CommandId { get; set; }
        public string CommandAction { get; set; }
        public Data Data { get; set; }
        public int Status { get; set; }
    }

    class Data
    {
        public int Temperature { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }
}
