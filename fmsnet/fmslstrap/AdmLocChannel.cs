using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fmslstrap.Channel;
using fmslstrap.Configuration;
using System.IO;
using fmslstrap.Tasks;
using fmslstrap.Administrator;
using fmslstrap.Interface;

namespace fmslstrap
{
    /// <summary>
    /// Локальный административный канал
    /// </summary>
    public static class AdmLocChannel
    {
        #region Частные данные
        private static ChanConfig _admloc;
        private static AdmChannel _admchan;
        #endregion

        #region Инициализация
        internal static void Init(AdmChannel AdmChan)
        {
            _admchan = AdmChan;
            _admloc = Manager.SubscribeToChannel("ADMLOC", AdmLocReceived, ChannelType.Local);

            ConfigurationManager.OnConfigReload += OnConfigReload;
        }
        #endregion

        #region Обработка событий
        private static void OnConfigReload()
        {
            _admloc.SendMessage(new[] { (byte)'A' });
        }

        private static void AdmLocReceived(ChanConfig sender, DataPacket Packet)
        {
            var ms = new MemoryStream(Packet.Data);
            var bmsgrdr = new BinaryReader(ms);

            var cmd = (char)bmsgrdr.ReadByte();

            switch (cmd)
            {
                #region Отправка конфигурационной секции по умолчанию
                case 'B':
                    var componentid = new Guid(bmsgrdr.ReadBytes(16));
                    var instanceid = new Guid(bmsgrdr.ReadBytes(16));
                    var section = bmsgrdr.ReadString();
                    var token = bmsgrdr.ReadBytes(16);

                    var defaultconfigsection = new Dictionary<string, List<string>>();

                    if (section != "<default>")
                    {
                        var ps = string.Format(@"{0}.{1}", section, Config.WorkstationName);
                        var s = ConfigurationManager.GetRawSection(ps) ?? ConfigurationManager.GetRawSection(section);

                        if (s != null)
                            defaultconfigsection = s;
                    }
                    else
                    {
                        var sect = ConfigurationManager.GetSection("component.glue");

                        try
                        {
                            var cs = sect[string.Format(@"{{{0}}}", componentid)].Value;
                            var iss = ConfigurationManager.GetSection(cs);
                            var ise = iss[string.Format(@"instance.{{{0}}}", instanceid)].Value;

                            var ps = string.Format(@"{0}.{1}", ise, Config.WorkstationName);

                            var s = ConfigurationManager.GetRawSection(ps) ?? ConfigurationManager.GetRawSection(ise);

                            if (s != null)
                                defaultconfigsection = s;
                        }
                        catch (NullReferenceException) { }
                        catch (FormatException) { }
                        catch (KeyNotFoundException) { }
                        catch (IndexOutOfRangeException) { }
                    }

                    var oms = new MemoryStream();
                    oms.WriteByte((byte)'C');
                    var zwr = new BinaryWriter(oms);

                    zwr.Write(section);
                    zwr.Write(token);
                    zwr.Write(defaultconfigsection.Count);
                    foreach (var k in defaultconfigsection)
                    {
                        zwr.Write(k.Key);
                        zwr.Write(k.Value.Count);
                        foreach (var kv in k.Value)
                            zwr.Write(kv);
                    }
                                        
                    sender.SendMessage(oms.ToArray());
                    break;
                #endregion

                #region Глобальный запуск группы процессов
                case 'D':
                    var start = bmsgrdr.ReadString();

                    oms = new MemoryStream();
                    var bwr = new BinaryWriter(oms);
                    bwr.Write((byte)'E');
                    bwr.Write(start);

                    _admchan.SendMessage(oms.ToArray());

                    break;
                #endregion

                #region Глобальный останов группы
                case 'E':
                    var stop = bmsgrdr.ReadString();

                    oms = new MemoryStream();
                    bwr = new BinaryWriter(oms);
                    bwr.Write((byte)'F');
                    bwr.Write(stop);

                    _admchan.SendMessage(oms.ToArray());
                    break;
                #endregion

                #region Локальный полный останов
                case 'X':
                    TasksManager.ShutdownAllTasks();
                    break;
                #endregion

                #region Глобальный полный останов (всего домена)
                case 'Z':
                    _admchan.SendMessage(new[] { (byte)'Z' });
                    break;
                #endregion

                #region Локальный старт задачи
                case 'F':
                    var lstart = bmsgrdr.ReadString();
                    TasksManager.StartTask(lstart);
                    break;
                #endregion

                #region Удаленный старт задачи
                case 'G':
                    var rhost = bmsgrdr.ReadString();
                    var rstart = bmsgrdr.ReadString();

                    oms = new MemoryStream();
                    bwr = new BinaryWriter(oms);
                    bwr.Write((byte)'G');
                    bwr.Write(rhost);
                    bwr.Write(rstart);

                    _admchan.SendMessage(oms.ToArray(), rhost == "*" ? null : rhost);
                    break;
                #endregion

                #region KILLALL
                case 'Y':
                    oms = new MemoryStream();
                    bwr = new BinaryWriter(oms);
                    bwr.Write((byte)'Y');

                    _admchan.SendMessage(oms.ToArray());
                    break;
                #endregion

                #region Задачи
                case 'H':
                    oms = new MemoryStream();
                    bwr = new BinaryWriter(oms);
                    bwr.Write('I');

                    var tasks = TasksManager.GetExecutingTasks();

                    var cnt = (UInt16)tasks.Count;

                    bwr.Write(cnt);

                    for (var i = 0; i < cnt; i++)
                        bwr.Write(tasks[i]);

                    _admloc.SendMessage(oms.ToArray());
                    break;
                #endregion

                #region Всплывающие оповещения
                case 'J':
                    var ttrd = new BinaryReader(ms, Encoding.UTF8);

                    var duration = ttrd.ReadInt32();
                    var caption = ttrd.ReadString();
                    var text = ttrd.ReadString();
                    var icon = (System.Windows.Forms.ToolTipIcon)ttrd.ReadByte();
                    var force = ttrd.ReadBoolean();

                    InterfaceManager.ShowBalloonTip(duration, caption, text, icon, force);

                    break;
                #endregion

                #region Журналирование
                case 'K':
                    var ttk = new BinaryReader(ms, Encoding.GetEncoding(bmsgrdr.ReadInt32()));
                    var msg = ttk.ReadString();
                    var msgsender = ttk.ReadString();
                    Logger.WriteLine(msgsender, msg);

                    break;
                #endregion

                #region Завершение работы fmsldr
                case 'L':
                    InterfaceManager.PressExit();
                    break;
                #endregion

                #region Хосты
                case 'N':
                    oms = new MemoryStream();
                    bwr = new BinaryWriter(oms);
                    bwr.Write('O');

                    var hosts =
                        EndPointsList.GetHosts()
                                     .Union(new[] { Config.WorkstationName })
                                     .Distinct()
                                     .ToArray();

                    bwr.Write((UInt16)hosts.Length);

                    foreach (var h in hosts)
                    {
                        bwr.Write(h);

                        var chans = h == Config.WorkstationName
                                        ? Manager.GetAllChannels().Select(x => x.Name).ToList()
                                        : EndPointsList.GetByHost(h).Select(x => x.Channel).ToList();

                        bwr.Write((Int16)chans.Count);

                        foreach (var c in chans)
                            bwr.Write(c);
                    }

                    _admloc.SendMessage(oms.ToArray());
                    break;

                    #endregion
            }
        }
        #endregion

        public static void Send(byte[] Data)
        {
            if (_admloc == null)
                return;

            AdmLocReceived(_admloc, new DataPacket { Data = Data });
        }
    }
}
