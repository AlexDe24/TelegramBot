using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logic.APITranslate
{
    public class Translator : ICanTranslate
    {
        private string _token;
        private string _request;

        public Translator(string token, string reqest)
        {
            _token = token;
            _request = reqest;
        }

        /// <summary>
        /// Перевод текста
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string Translate(string text, string langFrom, string langTo)
        {
            try
            {
                var client = new RestClient($"{_request}"
                  + $"key={_token}"
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
