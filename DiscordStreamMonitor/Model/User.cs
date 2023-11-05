using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStreamMonitor.Model
{
    public class User
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public DateTime StartedStream { get; set; }
        public ulong MessageId { get; set; }
    }
}
