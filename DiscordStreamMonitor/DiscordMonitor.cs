using Discord.WebSocket;
using Discord;
using DiscordStreamMonitor.Model;
using Stream = DiscordStreamMonitor.Model.Stream;

namespace DiscordStreamMonitor
{
    public class DiscordMonitor
    {
        private DiscordSocketClient _client;
        private MonitorContext _context;
        public DiscordMonitor(MonitorContext context) => _context = context;
        public async Task StartAsync()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(_config);
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
            await _client.StartAsync();

            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.Ready += async () =>
            {
                Console.WriteLine("Bot is connected!");
                await _client.SetGameAsync("Watching Streams");
            };
        }

        private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState state1, SocketVoiceState state2)
        {
            ulong guidId = state1.VoiceChannel?.Guild.Id ?? state2.VoiceChannel.Guild.Id;
            if (state1.VoiceChannel != null)
            {
                if (state1.VoiceChannel.Name != Environment.GetEnvironmentVariable("ChannelName"))
                    return;
            }
            else
                if (state2.VoiceChannel.Name != Environment.GetEnvironmentVariable("ChannelName"))
                    return;

            var user = _context.Users.Where(x => x.Id == socketUser.Id).FirstOrDefault();
            if (user == null)
            {
                var userVoice = _client.GetGuild(guidId).GetUser(socketUser.Id);
                user = new User()
                {
                    Id = socketUser.Id,
                    Name = socketUser.Username,
                    Nickname = userVoice.Nickname,
                    GlobalName = socketUser.GlobalName,
                };
                await _context.Users.AddAsync(user);
            }


            if (state1.IsStreaming == false && state2.IsStreaming)
            {
                state2.VoiceChannel.GetUser(socketUser.Id);
                Stream stream = new()
                {
                    Start = DateTime.Now,
                    UserId = user.Id,
                };
                string text = $"{user.Name}({user.Nickname})\n" +
                    $"Started stream: {DateTime.Now}";
                var res = await state2.VoiceChannel.SendMessageAsync(text: text);
                stream.MessageId = res.Id;
                await _context.Streams.AddAsync(stream);
            }
            else if (state1.IsStreaming && (!state2.IsStreaming || state2.VoiceChannel==null))
            {
                Stream lastStream = _context.Streams.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Start).FirstOrDefault();
                if (lastStream == null)
                    return;
                else
                    if (lastStream.End != null)
                    return;
                var duration = DateTime.Now - lastStream.Start;
                var userVoice = _client.GetGuild(guidId).GetUser(socketUser.Id);
                if (userVoice.IsStreaming)
                    return;
                String text = $"{user.Name}({user.Nickname})\n" +
                    $"Started stream: {lastStream.Start}\n" +
                    $"Stopped stream: {DateTime.Now}\n" +
                    $"Duration: {duration.ToString(@"h\h\ m\m\ s\s")}";
                await state1.VoiceChannel.ModifyMessageAsync(lastStream.MessageId, x => x.Content = text);
                lastStream.End = DateTime.Now;
                _context.Streams.Update(lastStream);
            }
            await _context.SaveChangesAsync();
        }
    }
}
