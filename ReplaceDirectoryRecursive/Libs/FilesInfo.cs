using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceDirectoryRecursive.Libs
{
    public static class FilesInfo
    {
        /// <summary>
        /// Return decimal size and format
        /// </summary>
        /// <param name="bytes">request size in bytes of file</param>
        /// <returns>return class type FilesInfoSizeModel</returns>
        public static FilesInfoSizeModel InfoSize(long bytes)
        {
            FilesInfoSizeModel info = new FilesInfoSizeModel();
            info.Format = "bytes";
            decimal div = bytes;
            if (bytes >= 1024)
            {
                div = ((decimal)bytes / 1024);
                info.Format = "KB";
            }

            if (bytes >= 1048576)
            {
                div = ((decimal)div / 1024);
                info.Format = "MB";
            }

            if (bytes >= 1073741824)
            {
                div = ((decimal)div / 1024);
                info.Format = "GB";
            }

            if (bytes >= 1099511627776)
            {
                div = ((decimal)div / 1024);
                info.Format = "TB";
            }

            info.Size = (decimal)Math.Round(div, 2);
            return info;
        }
    }
}
