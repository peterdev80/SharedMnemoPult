namespace fmslapi
{
    /// <summary>
    /// Расширения интерфейса IManager версии 2
    /// </summary>
    public interface IManager2 : IManager1
    {
        /// <summary>
        /// Возвращает путь к используемой библиотеке fmslapi
        /// </summary>
        string GetActualFMSLAPIPath();

        /// <summary>
        /// Возвращает содержимое используемой библиотеки fmslapi
        /// </summary>
        byte[] GetActualFMSLAPI();
    }
}
