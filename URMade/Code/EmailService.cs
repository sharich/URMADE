using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.IO;
using RazorEngine;
using RazorEngine.Templating;

namespace URMade
{
    public partial class EmailService
    {
        public static void SendMessage(
            IEnumerable<string> recipients,
            string subject,
            string message,
            string cc,
            bool useHtml = true
            )
        {
            SendMessage(
                recipients,
                subject,
                message,
                new List<string>() { cc },
                useHtml);
        }

        public static void SendMessage(
          string recipient,
          string subject,
          string message,
          string cc = null,
          bool useHtml = true
          )
        {
            SendMessage(
                new List<string>() { recipient },
                subject,
                message,
                String.IsNullOrWhiteSpace(cc) ?
                    null
                    : new List<string>() { cc },
                useHtml);
        }

        public static void SendMessage(
          IEnumerable<string> recipient,
          string subject,
          string message,
          IEnumerable<string> cc = null,
          bool useHtml = true,
          string replyTo = null
          )
        {
            var msg = new MailMessage();
            foreach (var address in recipient)
            {
                msg.To.Add(new MailAddress(address));
            }
            if (cc != null)
            {
                foreach (var address in cc)
                {
                    msg.CC.Add(address);
                }
            }
            msg.From = new MailAddress(Settings.FromAddress);
            if (replyTo != null) msg.ReplyToList.Add(replyTo);
            msg.Subject = subject;
            msg.IsBodyHtml = useHtml;
            msg.Body = message;

            var client = new SmtpClient();
            client.Send(msg);
        }

		public static void SendTemplate(string templatePath, Type modelType, object model, string key, IEnumerable<string> recipients)
		{
			string contents;

			if (!Engine.Razor.IsTemplateCached(key, modelType))
				contents = Engine.Razor.RunCompile(File.ReadAllText(templatePath), key, modelType, model);
			else
				contents = Engine.Razor.Run(key, modelType, model);
			
			SendMessage(recipients, key, contents);
		}
    }
}