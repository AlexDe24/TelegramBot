using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APITranslate
{
    public class Translator : INterfaceTranslator
    {
        /// <summary>
        /// Переводчик
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string Translate(string text, string langTo)
        {
            string langFrom = "";

            if (langTo == "ru")
                langFrom = "en";
            else
                langFrom = "ru";

            try
            {
                var client = new RestClient("https://translate.yandex.net/api/v1.5/tr.json/translate?"
                  + "key=trnsl.1.1.20170818T125137Z.bc602a78a7863e84.26863776415c893e9c6e51cc8a03462540980d54"
                  + "&text=" + text
                  + "&lang=" + langFrom + "-" + langTo);

                var request = new RestRequest("/", Method.POST);

                IRestResponse<TranslatorResponse> response = client.Execute<TranslatorResponse>(request);
                var translatorResponse = response.Data.Text;

                translatorResponse = translatorResponse.Remove(0, 2);
                translatorResponse = translatorResponse.Remove(translatorResponse.Length - 2, 2);

                return translatorResponse;
            }
            catch (Exception)
            {
                return text;
            }
        }
    }
}
