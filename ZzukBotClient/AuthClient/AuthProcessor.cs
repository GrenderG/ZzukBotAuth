﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZzukBotClient.Models;

namespace ZzukBotClient.AuthClient
{
    [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "Apply to member * when constructor or method or event: virtualization")]
    internal class AuthProcessor
    {
        private static readonly object _lock = new object();
        private static readonly Lazy<AuthProcessor> _instance = new Lazy<AuthProcessor>(() => new AuthProcessor());
        internal static AuthProcessor Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance.Value;
                }
            }
        }

        private AuthProcessor()
        {
        }

        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _accountName = 0;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _accountPassword = 1;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _md5String = 2;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _hardwareId = 3;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _loginResult = 4;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _eventSignal0Detour = 5;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _eventSignalDetour = 6;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _wardenLoadDetour = 7;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _WardenPageScanDetour = 8;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _wardenMemCpyDetour = 9;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _heartbeat = 10;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _ping = 11;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _pong = 12;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _botVersion = 13;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _hardwareIdResult = 14;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _botVersionReuslt = 15;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _md5StringResult = 16;
        [Obfuscation(ApplyToMembers = true, Exclude = false, Feature = "virtualization")]
        private uint _newSessionResult = 17;

        internal bool Auth(string user, string pass, out string reason)
        {
            try
            {
                if (!AuthClientHandler.Instance.CheckConnection())
                {
                    throw new Exception();
                }
                AuthClientHandler.Instance.Write(_ping);
                var packet = AuthClientHandler.Instance.GetNextPacket();
                if (packet.Opcode != _pong)
                {
                    reason = "Recieved wrong packets";
                    return false;
                }
                var accName = new PacketModel
                {
                    Opcode = _accountName,
                    Content = Encoding.Unicode.GetBytes(user.ToLower())
                };
                var accPass = new PacketModel
                {
                    Opcode = _accountPassword,
                    Content = Encoding.Unicode.GetBytes(pass)
                };
                AuthClientHandler.Instance.WriteRandomly(10, accName, accPass);
                packet = AuthClientHandler.Instance.GetNextPacket();
                if (packet.Opcode != _loginResult)
                {
                    reason = "Recieved wrong packets";
                    return false;
                }
                if (packet.Content[0] != 1)
                {
                    reason = "The account data seems to be invalid (false password, no credits etc.)";
                    Console.WriteLine(reason);
                    return false;
                }
                
                packet = AuthClientHandler.Instance.GetNextPacket();
                if (packet.Opcode != _newSessionResult)
                {
                    reason = "Recieved wrong packets";
                    return false;
                }
                if (packet.Content[0] != 1)
                {
                    reason = "Max sessions reached";
                    Console.WriteLine(reason);
                    return false;
                }
                //AuthClientHandler.Instance.Write(_hardwareId);
                //packet = AuthClientHandler.Instance.GetNextPacket();
                //if (packet.Opcode != _hardwareIdResult)
                //{
                //    reason = "Recieved wrong packets";
                //    return false;
                //}
                //if (packet.Content[0] != 1)
                //{
                //    reason = "All sessions for this account must be run on the same PC";
                //    Console.WriteLine(reason);
                //    return false;
                //}

                AuthClientHandler.Instance.Write(_md5String, "TEST".ToByte());
                packet = AuthClientHandler.Instance.GetNextPacket();
                if (packet.Opcode != _md5StringResult)
                {
                    reason = "Recieved wrong packets";
                    return false;
                }
                if (packet.Content[0] != 1)
                {
                    reason = "Please update the software";
                    Console.WriteLine(reason);
                    return false;
                }

                AuthClientHandler.Instance.Write(_botVersion, new byte[] { 1 });
                packet = AuthClientHandler.Instance.GetNextPacket();
                if (packet.Opcode != _botVersionReuslt)
                {
                    reason = "Recieved wrong packets";
                    return false;
                }
                if (packet.Content[0] != 1)
                {
                    reason = "Your product isnt supported by this authentication server";
                    Console.WriteLine(reason);
                    return false;
                }

                var packets = AuthClientHandler.Instance.GetRandomly(10, _wardenLoadDetour, _wardenMemCpyDetour);
                var loadDetour = packets[0].Content.BToString();
                var memcpyDetour = packets[1].Content.BToString();
                InitHeartbeat();
                reason = "";
                return true;
            }
            catch
            {
            }
            reason = "Something went wrong. Authentication server might be unreachable.";
            return false;
        }

        private async void InitHeartbeat()
        {
            try
            {
                while (true)
                {
                    AuthClientHandler.Instance.Write(_ping);
                    var pong = AuthClientHandler.Instance.GetNextPacket();
                    if (pong.Opcode != _pong) break;
                    await Task.Delay(10000);
                }
            }
            catch (Exception e)
            {
            }
            // quit;
        }
    }
}