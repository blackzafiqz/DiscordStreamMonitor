using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStreamMonitor.Model
{
    public class Stream
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public ulong MessageId { get; set; }

        public ulong UserId { get; set; }
        public virtual User User { get; set; }
    }
}
