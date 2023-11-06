# DiscordStreamMonitor

Build docker image

```
docker build -t discordstreammonitor -f .\DiscordStreamMonitor\Dockerfile .
```

Run docker image

```
docker run -d -e DiscordToken=<token> -e ChannelName=<channelname> --restart unless-stopped --name DiscordStreamMonitor -e TZ=Asia/Singapore -e ConnectionString=<db> discordstreammonitor
```