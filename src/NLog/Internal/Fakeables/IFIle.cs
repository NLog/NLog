namespace NLog.Internal.Fakeables
{
    /// <summary>
    /// Abstract calls to File
    /// </summary>
    internal interface IFile
    {
        /// <summary>Determines whether the specified file exists.</summary>
        /// <param name="path">The file to check. </param>
        /// <returns></returns>
        bool Exists(string path);
    }
}
