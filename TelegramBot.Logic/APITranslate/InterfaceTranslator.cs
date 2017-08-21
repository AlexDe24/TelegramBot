using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APITranslate
{
    public interface INterfaceTranslator
    {
        string Translate(string text, string langTo);
    }
}
