using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using demo.Models;

namespace demo.Formatter
{
    public class CsvFormatter : BufferedMediaTypeFormatter 
    {
        public CsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == typeof(Product))
            {
                return true;
            }
            
            Type enumerableType = typeof(IEnumerable<Product>);
            return enumerableType.IsAssignableFrom(type);
        }

        
        public override void WriteToStream(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content)
        {
            using (var writer = new StreamWriter(writeStream))
            {

                var products = value as IEnumerable<Product>;
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        WriteItem(product, writer);
                    }
                }
                else
                {
                    var singleProduct = value as Product;
                    if (singleProduct == null)
                    {
                        throw new InvalidOperationException("Cannot serialize type");
                    }
                    WriteItem(singleProduct, writer);
                }
            }
            writeStream.Close();
        }

        // Helper methods for serializing Products to CSV format. 
        private void WriteItem(Product product, StreamWriter writer)
        {
            writer.WriteLine("{0},{1}", Escape(product.ProductId), Escape(product.ProductName));
        }

        static readonly char[] _specialChars = new char[] { ',', '\n', '\r', '"' };

        private string Escape(object o)
        {
            if (o == null)
            {
                return "";
            }

            string field = o.ToString();

            return field.IndexOfAny(_specialChars) != -1 ? String.Format("\"{0}\"", field.Replace("\"", "\"\"")) : field;
        }
    }
}