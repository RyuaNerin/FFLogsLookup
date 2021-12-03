using System;
using System.Collections.Generic;
using System.Reflection;
using Dalamud.Game.Command;
using FFLogsLookup.Game;
using static Dalamud.Game.Command.CommandInfo;

namespace FFLogsLookup
{
    internal class PluginCommand : IDisposable
    {
        private readonly Plugin plugin;

        private readonly List<string> commands = new();

        public PluginCommand(Plugin plugin)
        {
            this.plugin = plugin;

            foreach (var method in this.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod))
            {
                try
                {
                    var commands = method.GetCustomAttribute<CommandAttribute>().Commands;
                    var help = method.GetCustomAttribute<HelpMessageAttribute>().HelpMessage;
                    var showInHelp = method.GetCustomAttribute<ShowInHelpAttribute>()?.ShowInHelp ?? false;

                    var commandInfo = new CommandInfo(method.CreateDelegate<HandlerDelegate>(this))
                    {
                        HelpMessage = help,
                        ShowInHelp = showInHelp,
                    };

                    foreach (var cmd in commands)
                    {
                        DalamudInstance.CommandManager.AddHandler(cmd, commandInfo);
                        commandInfo.ShowInHelp = false;
                        this.commands.Add(cmd);
                    }
                }
                catch
                {
                }
            }
        }
        ~PluginCommand()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool disposed;
        protected void Dispose(bool disposing)
        {
            if (this.disposed) return;
            this.disposed = true;

            if (disposing)
            {
                foreach (var s in commands)
                {
                    DalamudInstance.CommandManager.RemoveHandler(s);
                }
            }
        }

        [Command("/fflogsconfig", "/ffconf")]
        [HelpMessage("FFLogsLookup의 설정창을 엽니다.")]
        [ShowInHelp(true)]
        private void OpenWindow(string command, string args)
        {
            this.plugin.WindowConfig.IsOpen = true;
        }

        [Command("/fflogs", "/ff", "/ㅍㅍ", "/프프로그", "/ㅍㅍㄺ", "/ㅍㅍㄹㄱ")]
        [HelpMessage("해당 사용자의 fflogs를 조회합니다.")]
        [ShowInHelp(true)]
        private void View(string command, string args)
        {
            if (args == "party" || args == "p" || args == "파티" || args == "파")
            {
                this.ViewParty(null, null);
                return;
            }

            try
            {
                var sp = args.Split(' ', '@');
                var name = sp[0].Trim();
                var serverStr = sp[1].Trim();

                var server = GameData.GetGameServer(serverStr);

                this.plugin.WindowDetail.Update(name, server);
            }
            catch
            {
                DalamudInstance.ChatGui.Print($"사용방법 : {command} 아이디 서버");
            }
        }

        [Command("/fflogsparty", "/fp", "/프프파티", "/프프로그파티", "/ㅍㅍㄺㅍㅌ", "/ㅍㅍㄹㄱㅍㅌ")]
        [HelpMessage("현재 파티원의 fflogs를 조회합니다.")]
        [ShowInHelp(true)]
        private void ViewParty(string command, string args)
        {

















        }

        public class CommandAttribute : Attribute
        {
            public CommandAttribute(params string[] commands)
            {
                this.Commands = commands;
            }
            public string[] Commands { get; }
        }
        public class HelpMessageAttribute : Attribute
        {
            public HelpMessageAttribute(string helpMessage)
            {
                this.HelpMessage = helpMessage;
            }
            public string HelpMessage { get; }
        }
        public class ShowInHelpAttribute : Attribute
        {
            public ShowInHelpAttribute(bool showInHelp)
            {
                this.ShowInHelp = showInHelp;
            }
            public bool ShowInHelp { get; }
        }
    }
}
