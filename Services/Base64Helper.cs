using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncryptTool.Services
{
    public static class Base64Helper
    {
        public static string Encode(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str ?? string.Empty));

        public static string Decode(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("请输入需要解码的Base64内容");

            return Encoding.UTF8.GetString(Convert.FromBase64String(str));
        }
    }
}
