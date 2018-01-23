using System.Net;
using System.IO;

namespace fmslstrap.CommandSocket
{
    /// <summary>
    /// Базовый класс управляющих команд обмена
    /// </summary>
    public abstract class BaseCommand
    {
        public abstract void Invoke(BinaryReader Reader, IPEndPoint EndPoint, out string LogLine);
    }
}
