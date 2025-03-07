﻿using System.Text.RegularExpressions;

namespace attochatter.Hubs;

public class ChatHub : Hub
{
    private static List<string> CurrentGroups = new List<string>() { "General" };
    private readonly IHubContext<ServerHub> _serverHub;
    private Regex mention = new Regex("@(?<name>[^\\s]+)");
    public ChatHub(IHubContext<ServerHub> server) { _serverHub = server; }

    public async Task Ping() =>
        await Clients.Caller.SendAsync("pong", "🏓");

    //public void ???() => new SoundEffects().Play(SoundEffect.bubble_pop);

    public async Task chat(string username, string message, string chatroom)
    {
        if (chatroom == null || chatroom.Length == 0)
        {
            // do nothing!
        } 
        else
        {
            await Clients.Group(chatroom).SendAsync("chat", username, message);
            await TrySendMentions(username, message, chatroom);
        }
    }

    private async Task TrySendMentions(string username, string message, string chatroom)
    {
        var results = mention.Matches(message)
                        .Cast<Match>()
                        .Select(m => m.Groups["name"].Value)
                        .ToArray();
        if (results.Length > 0)
        {
            try
            {
                foreach (var match in results)
                {
                    if (match != null)
                    {
                        await _serverHub.Clients.Group(username).SendAsync("mention", chatroom);
                    }
                }
            }
            catch { }
        }
    }

    public async Task joinChatroom(string chatroom)
    {
        if (chatroom != null && chatroom.Length > 0)
        {
            if (!CurrentGroups.Contains(chatroom))
                CurrentGroups.Add(chatroom);

            await Groups.AddToGroupAsync(Context.ConnectionId, chatroom);
        }
    }

    public async Task listChatrooms()
    {
        await Clients.Caller.SendAsync("listChatrooms", CurrentGroups);
    }

}