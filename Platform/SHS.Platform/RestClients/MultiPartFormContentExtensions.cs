using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Platform.RestClients
{
    public static class MultiPartFormDataContentExtensions
    {
        public static void Add(this MultipartFormDataContent form, HttpContent content, object formValues)
        {
            Add(form, content, formValues);
        }

        public static void Add(this MultipartFormDataContent form, HttpContent content, string name, object formValues)
        {
            Add(form, content, formValues, name: name);
        }

        public static void Add(this MultipartFormDataContent form, HttpContent content, string name, string fileName, object formValues)
        {
            Add(form, content, formValues, name: name, fileName: fileName);
        }


        private static void Add(this MultipartFormDataContent form, HttpContent content, object formValues, string name = null, string fileName = null)
        {
            var header = new ContentDispositionHeaderValue("form-data");
            header.Name = name;
            header.FileName = fileName;
            header.FileNameStar = fileName;

            var headerParameters = new HttpRouteValueDictionary(formValues);
            foreach (var parameter in headerParameters)
            {
                header.Parameters.Add(new NameValueHeaderValue(parameter.Key, parameter.Value.ToString()));
            }

            content.Headers.ContentDisposition = header;
            form.Add(content);
        }

    }

}
