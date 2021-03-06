﻿using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using FileManager.Class;

namespace FileManager.Archives
{
    public class ArchiveFile : AbstractFile
    {
        private readonly ZipArchiveEntry _entry;

        public ArchiveFile(string abstractPath, ZipArchiveEntry entry)
        {
            // для работы с entry архив должен быть открыт в ArchiveDirectory в режиме Update
            _entry = entry;
            // path внутри архива
            AbstractPath = abstractPath;

            AbstractName = entry.Name;
            AbstractSize = entry.CompressedLength;
            DateOfLastAppeal = entry.LastWriteTime.DateTime.ToString(CultureInfo.InvariantCulture);
        }

        public override void AbstractCopy(AbstractFile file)
        {
            var buffer = new byte[1024 * 1024]; //мегабайтный буфер
            using (var aStream = _entry.Open())
            {
                aStream.Read(buffer, 0, buffer.Length);
                file.AbstractWrite(buffer);
            }
        }

        public override void AbstractWrite(byte[] bytesArr)
        {
            using (var aStream = _entry.Open())
                aStream.Write(bytesArr, 0, bytesArr.Length);
        }

        public override void AbstractReplace(AbstractFolder inDirectory)
        {
            // нет упрощенного перемещения для архивных файлов, оставляем только обобщенный код
            var abstractFile = inDirectory.AbstractCreateFile(AbstractName);
            AbstractCopy(abstractFile);
            AbstractRemove();
        }

        public override void AbstractRemove()
        {
            _entry.Delete();
        }

        public override void AbstractOpen()
        {
            var result = Path.GetTempPath();
            _entry.ExtractToFile(Path.Combine(result, _entry.FullName));
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            AbstractPath = Path.Combine(result, AbstractName);
            var psi = new ProcessStartInfo { FileName = AbstractPath };
            var process = Process.Start(psi);
            if (process != null) process.WaitForExit();
            if (!System.IO.File.Exists(AbstractPath)) return;
            System.IO.File.Delete(AbstractPath);
           
        }
    }
}
