using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TestingEfficiency;


public class webhookmanager : ModSystem
{
    public struct WebhookField
    {
        public WebhookField(string Name, string Value, bool Inline = false)
        {
            name = Name;
            value = Value;
            inline = Inline;
        }
        public string name;
        public string value;
        public bool inline;
        public string ToString()
        {
            return """{"name": """+ (name == null? "null" : '"'+name+'"') + """, "value": """+ (value == null? "null" : '"'+value+'"') + """, "inline": """+ (inline? "true" : "false") + """},""";
        }
    }
    public struct Embed
    {
        public Embed(string Title = null, string Description = null, string TitleURL = null, string Color = null, List<WebhookField> Fields = null, string Author = null, string Footer = null, string Timestamp = null, string Thumbnail = null)
        {
            title = Title;
            description = Description;
            titleurl = TitleURL;
            color = Color;
            fields = Fields;
            author = Author;
            footer = Footer;
            timestamp = Timestamp;
            thumbnail = Thumbnail;
        }
        public string title;
        public string description;
        public string titleurl;
        public string color;
        public List<WebhookField> fields;
        string author;
        string footer;
        string timestamp;
        string image;
        string thumbnail;

        public string ToString()
        {
            var fieldstr = "";
            /*if (fields == null)
            {
                Main.NewText("get a field");
                return null;
            }*/
            if (fields != null) foreach (var x in fields)
            {
                fieldstr += x.ToString();
            }
            if (fieldstr != null && fieldstr != "") fieldstr = fieldstr.Substring(0, fieldstr.Length - 1);
            return """{"title": """+ (title == null? "null": '"'+title+'"') + """, "description": """+ (description == null? "null" : '"'+description+'"') + (fieldstr == null || fieldstr == "" ? "" : ""","fields":["""+fieldstr+"""]""")+"""},""";
        }
    }
    public struct Webhook
    {
        public Webhook(string Content = null, List<Embed> Embeds = null, string Username = null, string PFP = null)
        {
            content = Content;
            embeds = Embeds;
            username = Username;
            pfp = PFP;
        }
        public string content;
        public List<Embed> embeds;
        public string username;
        public string pfp;

        public void Publish()
        {
            if (DiscordConfig.Instance.webhookurl == "")
            {
                Main.NewText("Add a Webhook URL to publish");
                return;
            }
            string embdstr = "";
            foreach (var x in embeds)
            {
                embdstr += x.ToString();
            }
            if (embdstr != null && embdstr != "") embdstr = embdstr.Substring(0, embdstr.Length - 1);
            string webhook = DiscordConfig.Instance.webhookurl;
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\":" + (content == null ? "null" : '"'+content+'"') + ", \"embeds\":["+ embdstr +"]}";
            Console.WriteLine(payload);
            try {
                client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
                    } catch (Exception e)
            {
                Main.NewText("Publish Failed! " + e);
            }
        }
        


    }
}
