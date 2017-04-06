using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SMT.Models
{
    public class Util
    {

        public class Attachment
        {
            public string name { get; set; }
            public string data { get; set; }
        }

        public class CaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }

        /// <summary>
        /// Envía un email a un destinatario utilizando un template de html
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="template">Nombre del archivo html</param>
        /// <param name="parametros"></param>
        public static void SendEmailWithTemplate(string email, string subject, string template, object parametros, string user = null, Attachment attachment = null)
        {
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/PlantillasEmail/");

            if (!string.IsNullOrEmpty(template))
            {
                try
                {
                    string html = File.ReadAllText(path + template);

                    foreach (var m in parametros.GetType().GetProperties())
                    {
                        html = html.Replace("{" + m.Name + "}", parametros.GetType().GetProperty(m.Name).GetValue(parametros, null).ToString());
                    }

                    user = "soporte";

                    SendEmail(user, email, html, subject, attachment);
                }
                catch (Exception e)
                {

                }
            }


        }

        /// <summary>
        /// Enviar un email
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email">Destinatario</param>
        /// <param name="message"></param>
        /// <param name="subject"></param>
        public static void SendEmail(string user, string email, string message, string subject, Attachment attachment = null)
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;
            string userName = appConfig["EmailUser"], mask = appConfig["EmailMask"], password = appConfig["EmailPass"], host = appConfig["EmailHost"];
            int port = int.Parse(appConfig["EmailPort"]);

            MailMessage mail = new MailMessage();

            mail.To.Add(email);
            mail.From = new MailAddress(string.Format(mask, user), string.Format(mask, user), System.Text.Encoding.UTF8);
            mail.Subject = subject;
            mail.IsBodyHtml = true;

            if (attachment != null && attachment.data != null)
            {
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(message, Encoding.UTF8, MediaTypeNames.Text.Html);
                String encodingPrefix = "base64,";
                int contentStartIndex = attachment.data.IndexOf(encodingPrefix) + encodingPrefix.Length;
                byte[] imageData = Convert.FromBase64String(attachment.data.Substring(contentStartIndex));

                LinkedResource img = new LinkedResource(new MemoryStream(imageData));
                img.ContentId = "image";
                img.TransferEncoding = TransferEncoding.Base64;
                img.ContentType.Name = !string.IsNullOrEmpty(attachment.name) ? attachment.name : string.Format("Archivo_{0}.png", DateTime.Now.ToString("MMddyyyyHmm"));
                img.ContentLink = new Uri("cid:" + img.ContentId);
                htmlView.LinkedResources.Add(img);

                mail.AlternateViews.Add(htmlView);
            }
            else
            {
                mail.Body = message;
            }

            SmtpClient smtpClient = new SmtpClient();

            smtpClient.Credentials = new System.Net.NetworkCredential(userName, password);
            smtpClient.Port = port;
            smtpClient.Host = host;
            smtpClient.EnableSsl = false;

            try
            {
                smtpClient.Send(mail);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal static T Cast<T>(T tObj, dynamic obj)
        {
            T item = (T)Activator.CreateInstance(tObj.GetType());
            foreach (PropertyInfo prop in item.GetType().GetProperties())
                if (obj.GetType().GetProperty(prop.Name) != null && obj.GetType().GetProperty(prop.Name).GetValue(obj, null) != null)
                {
                    Type type = prop.PropertyType;

                    if (!type.Namespace.StartsWith("System") || type.IsArray)
                    {
                        if (!type.IsPrimitive)
                        {
                            prop.SetValue(item, Util.Cast(Activator.CreateInstance(type), obj.GetType().GetProperty(prop.Name).GetValue(obj, null)));
                        }
                        else if (type.IsArray)
                        {

                        }
                    }
                    else
                        prop.SetValue(item, TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromString(obj.GetType().GetProperty(prop.Name).GetValue(obj, null).ToString().Replace("\"", "")));
                }

            return item;
        }

        internal static void ValidateCaptcha(string response)
        {
            WebClient client = new WebClient();
            string reply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", ConfigurationManager.AppSettings["ReCaptchaSecretKey"], response));
            CaptchaResponse result = JsonConvert.DeserializeObject<CaptchaResponse>(reply);

            if (!result.Success)
            {
                if (result.ErrorCodes.Count <= 0)
                    throw new Exception("Ha ocurrido un error. Intente nuevamente (Captcha)");

                switch (result.ErrorCodes[0].ToLower())
                {
                    case ("missing-input-secret"):
                        throw new Exception("No se encontro el parametro secreto (Captcha)");
                    case ("invalid-input-secret"):
                        throw new Exception("El parametro secreto es invalido (Captcha)");
                    case ("missing-input-response"):
                        throw new Exception("No se encontro el parametro de respuesta (Captcha)");
                    case ("invalid-input-response"):
                        throw new Exception("El parametro de respuesta es invalido (Captcha)");
                    default:
                        throw new Exception("Ha ocurrido un error. Intente nuevamente (Captcha)");
                }
            }
        }
        public static int GetFileType(string fileName)
        {
            Regex reg = new Regex(@"\.xls");
            Regex reg2 = new Regex(@"\.xlsx");
            Regex reg3 = new Regex(@"\.csv");

            if (reg2.IsMatch(fileName))
                return 2;
            else if (reg.IsMatch(fileName))
                return 1;
            else if (reg3.IsMatch(fileName))
                return 3;
            else
                return 0;
        }



        public static Stream convertirJPG(Stream file, int maxWidth = 800, int maxHeight = 800)
        {
            // Load the image.

            System.Drawing.Image image = System.Drawing.Image.FromStream(file);

            image = ScaleImage(image, maxWidth, maxHeight);

            // Save the image in JPEG format.
            image.Save(file, System.Drawing.Imaging.ImageFormat.Jpeg);



            return file;
        }
        public static Stream convertirJPGNew(Stream file, int maxWidth = 800, int maxHeight = 800)
        {
            // Load the image.

            System.Drawing.Image image = System.Drawing.Image.FromStream(file);

            if (image.Size.Width > 800 || image.Size.Height > 800)
            {
                throw new System.ArgumentException("Error: las dimensiones máximas permitidas son de  800 de ancho y 800 de alto", "");
            }
            image = ScaleImage(image, maxWidth, maxHeight);

            // Save the image in JPEG format.
            image.Save(file, System.Drawing.Imaging.ImageFormat.Jpeg);



            return file;
        }
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }


        public static string RemoveDiacritics(string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                var normalizedString = text.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            }

            return "";
        }

        public static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string UpperTitle(string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        public static double calificacionSEP10(double cali)
        {
            if (cali < 6)
                return 5;
            if (cali > 10)
                return 10;
            return cali;
        }

        public static string calificacionSEP10String(double cali)
        {
            if (cali < 6)
                return string.Format("5({0:0.#})", cali);
            if (cali > 10)
                return string.Format("10({0:0.#})", cali);
            return string.Format("{0:0.#}", cali);
        }

        public static double calificacionSEP100(double cali)
        {
            if (cali < 60)
                return 50;
            if (cali > 100)
                return 100;
            return cali;
        }

        public static string calificacionSEP100String(double cali)
        {
            if (cali < 60)
                return string.Format("50({0:0.#})", cali);
            if (cali > 100)
                return string.Format("100({0:0.#})", cali);
            return string.Format("{0:0.#}", cali);
        }

        public static string substring(string texto, int caracteres)
        {
            return texto.Substring(0, (texto.Length < caracteres ? texto.Length : caracteres)) + (texto.Length > caracteres ? "..." : "");
        }


        public static DateTime toHoraMexico(DateTime fecha)
        {
            return fecha.AddHours(-5);
        }

        public static DateTime toHoraUTC(DateTime fecha)
        {
            return fecha.AddHours(5);
        }
    }


}