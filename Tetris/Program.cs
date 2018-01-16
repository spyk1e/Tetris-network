using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Tetris
{
    class Program
    {
        public static int Main(String[] args)
        {
            Process.Start("C:\\Users\\Pierre\\Documents\\Visual Studio 2015\\Projects\\DesignPatern\\Tetris\\Server\\bin\\Debug\\Server.exe");
            Process.Start("C:\\Users\\Pierre\\Documents\\Visual Studio 2015\\Projects\\DesignPatern\\Tetris\\Client\\bin\\Debug\\Client.exe");
            return 0;
        }
    }
}
