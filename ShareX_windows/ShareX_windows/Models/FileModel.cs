using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareX_windows.Models
{
    public class FileModel
    {
        public string Name { get;}
        public string Path { get;}
        public long size { get;}
        public double progress { get; set; } = 0;
        public bool completed { 
            get => progress >= 100;
        }


        public FileModel(string name, string path, long size)
        {
            Name = name;
            Path = path;
            this.size = size;
        }

    }
}
