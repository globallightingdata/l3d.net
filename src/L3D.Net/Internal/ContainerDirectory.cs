using System;
using L3D.Net.Internal.Abstract;

namespace L3D.Net.Internal
{
    internal class ContainerDirectory : IContainerDirectory
    {
        public ContainerDirectory()
        {
            while (Path == null || System.IO.Directory.Exists(Path))
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Constants.TempWorkingSubDirectory, Guid.NewGuid().ToString());
            }

            System.IO.Directory.CreateDirectory(Path);
        }

        public void CleanUp()
        {
            try
            {
                System.IO.Directory.Delete(Path, true);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string Path { get; }
    }
}
