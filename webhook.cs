using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace TestingEfficiency;


public class WebhookManager : ModSystem
{

    public static HttpClient textclient;
    public override void Load()
    {
        textclient = new HttpClient();
    }

    public override void Unload()
    {
        textclient = null;
    }
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
            return JsonSerializer.Serialize(this);

            return """{"name": """ + (name == null ? "null" : '"' + name + '"') + """, "value": """ + (value == null ? "null" : '"' + value + '"') + """, "inline": """ + (inline ? "true" : "false") + """},""";
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
            return """{"title": """ + (title == null ? "null" : '"' + title + '"') + """, "description": """ + (description == null ? "null" : '"' + description + '"') + (fieldstr == null || fieldstr == "" ? "" : ""","fields":[""" + fieldstr + """]""") + """},""";
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
        [JsonIgnore]
        public Texture2D image;

        public void Publish()
        {
            if (DiscordConfig.Instance.webhookurl == "")
            {
                Main.NewText("Add a Webhook URL to publish");
                return;
            }
            if (image is not null)
            {
                byte[] imageBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    image.SaveAsPng(ms, image.Width, image.Height);
                    imageBytes = ms.ToArray();
                }

                using (var client = new HttpClient())
                using (var form = new MultipartFormDataContent())
                {
                    var imageContent = new ByteArrayContent(imageBytes);
                    form.Add(imageContent, "file1", "testResult.png");
                    HttpResponseMessage response = client.PostAsync(DiscordConfig.Instance.webhookurl, form).GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to upload: {response.StatusCode}");
                    }
                }
            }
        }



    }


    #region DrawPanel
    private static int _cornerSize = 12;
    private static int _barSize = 4;
    public static Asset<Texture2D> borderTexture => _bdTX ??= Main.Assets.Request<Texture2D>("Images/UI/PanelBorder");
    public static Asset<Texture2D> backgroundTexture => _bgTX ??= Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
    public static Color BorderColor = Color.Black;
    public static Color BackgroundColor = new Color(63, 82, 151) * 0.7f;

    private static Asset<Texture2D> _bdTX;
    private static Asset<Texture2D> _bgTX;

    public static void DrawPanel(SpriteBatch spriteBatch, Rectangle frame, Color? bgColor = null, Color? bdColor = null)
    {
        Point point = new Point(frame.X, frame.Y);
        Point point2 = new Point(point.X + (int)frame.Width - _cornerSize, point.Y + (int)frame.Height - _cornerSize);
        int width = point2.X - point.X - _cornerSize;
        int height = point2.Y - point.Y - _cornerSize;

        var color = bgColor ?? BackgroundColor;
        var texture = backgroundTexture.Value;
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, _cornerSize, _cornerSize), new Rectangle(0, 0, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, 0, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(0, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y, width, _cornerSize), new Rectangle(_cornerSize, 0, _barSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point2.Y, width, _cornerSize), new Rectangle(_cornerSize, _cornerSize + _barSize, _barSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(0, _cornerSize, _cornerSize, _barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(_cornerSize + _barSize, _cornerSize, _cornerSize, _barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y + _cornerSize, width, height), new Rectangle(_cornerSize, _cornerSize, _barSize, _barSize), color);

        color = bdColor ?? BorderColor;
        texture = borderTexture.Value;
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, _cornerSize, _cornerSize), new Rectangle(0, 0, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, 0, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(0, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y, width, _cornerSize), new Rectangle(_cornerSize, 0, _barSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point2.Y, width, _cornerSize), new Rectangle(_cornerSize, _cornerSize + _barSize, _barSize, _cornerSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(0, _cornerSize, _cornerSize, _barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(_cornerSize + _barSize, _cornerSize, _cornerSize, _barSize), color);
        spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y + _cornerSize, width, height), new Rectangle(_cornerSize, _cornerSize, _barSize, _barSize), color);

    }
    #endregion

    #region DrawTextbox

    public static void DrawTexbox(SpriteBatch spriteBatch, Vector2 pos, string text, Rectangle? maxSize = null, Vector2? scale = null)
    {

        Color _color = Color.White;
        Color _shadowColor = Color.Black;
        DynamicSpriteFont font = FontAssets.MouseText.Value;
        var displayText = text;
        Vector2 vector = ChatManager.GetStringSize(font, displayText, new Vector2(1));
        Color baseColor = _shadowColor * ((float)(int)_color.A / 255f);
        Vector2 origin = new Vector2(0f, 0f) * vector;
        Vector2 baseScale = scale ?? Vector2.One;
        TextSnippet[] snippets = ChatManager.ParseMessage(displayText, _color).ToArray();
        ChatManager.ConvertNormalSnippets(snippets);
        DrawPanel(spriteBatch, new((int)pos.X, (int)pos.Y, (int)vector.X + 24, (int)vector.Y + 24));
        pos += new Vector2(12);
        ChatManager.DrawColorCodedStringShadow(spriteBatch, font, snippets, pos, baseColor, 0f, origin, baseScale, -1f, 1.5f);
        ChatManager.DrawColorCodedString(spriteBatch, font, snippets, pos, Color.White, 0f, origin, baseScale, out var _, -1f);
    }
    #endregion
}
