using Discord;
using Discord.WebSocket;
using DiscordStreamMonitor.Model;
using System.Diagnostics.Metrics;

namespace DiscordStreamMonitor
{
    internal class Program
    {
        private static DiscordSocketClient _client;
        private static List<User> _users = new();
        public static async Task Main()
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

            await Task.Delay(-1);
        }

        private static async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState state1, SocketVoiceState state2)
        {
            if (state1.VoiceChannel.Name != Environment.GetEnvironmentVariable("ChannelName"))
                return;
            var user = _users.Where(x => x.Id == socketUser.Id).FirstOrDefault();
            if (user == null)
            {
                var userVoice = _client.GetGuild(state1.VoiceChannel.Guild.Id).GetUser(socketUser.Id);
                if (!userVoice.IsStreaming)
                    return;
                user = new User()
                {
                    Id = userVoice.Id,
                    Name = userVoice.Username,
                    Nickname = userVoice.Nickname,
                    StartedStream = DateTime.Now
                };

                String text = $"{user.Name}({user.Nickname})\n" +
                    $"Started stream: {DateTime.Now}";
                var res = await state1.VoiceChannel.SendMessageAsync(text: text);
                user.MessageId = res.Id;
                _users.Add(user);
            }
            else
            {
                var duration = DateTime.Now - user.StartedStream;
                String text = $"{user.Name}({user.Nickname})\n" +
                    $"Started stream: {user.StartedStream}\n" +
                    $"Stopped stream: {DateTime.Now}\n" +
                    $"Duration: {duration.ToString(@"h\h\ m\m\ s\s")}";

                await state1.VoiceChannel.ModifyMessageAsync(user.MessageId, x => x.Content = text);
                _users.Remove(user);
            }
        }

    }
}