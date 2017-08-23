using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APITranslate
{
    public interface ICanTranslate
    {
        string Translate(string text, string langFrom, string langTo);
    }
}
