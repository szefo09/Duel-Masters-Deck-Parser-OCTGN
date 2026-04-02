namespace Octgn.DuelMastersDeckParser
{
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.Core.Plugin;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Plugin;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;

    public class DuelMastersDeckParser : IDeckBuilderPlugin
    {
        public IEnumerable<IPluginMenuItem> MenuItems
        {
            get
            {
                // Add your menu items here.
                return new List<IPluginMenuItem>{new PluginMenuItem()};
            }
        }

        public void OnLoad(GameManager games)
        {
        }

        public Guid Id
        {
            get
            {
                // All plugins are required to have a unique GUID
                // http://www.guidgenerator.com/online-guid-generator.aspx
                return Guid.Parse("1d6665e4-0a7a-4fa9-b466-3493c116e190");
            }
        }

        public string Name
        {
            get
            {
                // Display name of the plugin.
                return "Duel Masters Deck Parser";
            }
        }

        public Version Version
        {
            get
            {
                // Version of the plugin.
                // This code will pull the version from the assembly.
                return Assembly.GetCallingAssembly().GetName().Version;
            }
        }

        public Version RequiredByOctgnVersion
        {
            get
            {
                // Don't allow this plugin to be used in any version less than 3.0.12.58
                return Version.Parse("3.1.0.0");
            }
        }
    }
    public static class SimpleInputBox
    {
        public static string Show(string title, string prompt, string textBoxText="")
        {
            var window = new Window
            {
                Title = title,
                Width = 350,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var panel = new StackPanel { Margin = new Thickness(10) };

            var text = new TextBlock { Text = prompt };
            var input = new TextBox
            {   Text = textBoxText,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 400
            };
            var ok = new Button { Content = "OK", Width = 60, Margin = new Thickness(5) };
            var cancel = new Button { Content = "Cancel", Width = 60, Margin = new Thickness(5) };

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);

            panel.Children.Add(text);
            panel.Children.Add(input);
            panel.Children.Add(buttons);

            window.Content = panel;

            string result = null;

            ok.Click += (s, e) =>
            {
                result = input.Text;
                window.DialogResult = true;
            };

            cancel.Click += (s, e) =>
            {
                window.DialogResult = false;
            };

            window.ShowDialog();

            return result;
        }
    }

    public class PluginMenuItem : IPluginMenuItem
    {
        private static readonly Dictionary<string, string> CardAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "bat", "bronze-arm tribe" },
            { "tdad", "terradragon arque delacerna" },
            {"trash crawler", "thrash crawler" }
        };
        private class ParsedCard
        {
            public int Count { get; set; }
            public string Name { get; set; }
        }
        private class NormalizedCard {             
            public Card Card { get; set; }
            public string NormalizedName { get; set; }
        }
        public string Name
        {
            get
            {
                return "Duel Masters Deck Parser";
            }
        }

        /// <summary>
        /// This happens when the menu item is clicked.
        /// </summary>
        /// <param name="con"></param>
        private ParsedCard ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            line = line.Trim();

            var match = Regex.Match(line, @"^(\d+)\s*x?\s*(.+)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return new ParsedCard
                {
                    Count = int.Parse(match.Groups[1].Value),
                    Name = match.Groups[2].Value.Trim().ToLower()
                };
            }

            return new ParsedCard
            {
                Count = 1,
                Name = line.ToLower()
            };
        }
        private Card FindBestMatch(string input, List<NormalizedCard> cards)
        {
            input = input.ToLower();


            var candidates = cards
                .Where(c => c.NormalizedName.Contains(input))
                .ToList();

            var preferred = FilterPreferred(candidates);
            if (preferred.Count > 0)
                return preferred[0].Card;

            if (candidates.Count > 0)
                return candidates[0].Card;

            candidates = cards
                .Where(c => input.Contains(c.NormalizedName))
                .ToList();

            preferred = FilterPreferred(candidates);
            if (preferred.Count > 0)
                return preferred[0].Card;

            if (candidates.Count > 0)
                return candidates[0].Card;

            return null;
        }
        private List<NormalizedCard> FilterPreferred(List<NormalizedCard> candidates)
        {
            var result = new List<NormalizedCard>();
           
            foreach (var c in candidates)
            {
                var set = c.Card.GetProperty("Set");
                var format = c.Card.GetProperty("Format");

                if (set == null || format == null)
                    continue;

                string setStr = set.ToString();
                string formatStr = format.ToString();

                if (Regex.IsMatch(setStr, @"DM-\d\d")  && formatStr.Contains("TCG"))
                {
                    result.Add(c);
                }
                if (setStr == "Promo and DMC Packs" || setStr == "English Promotional Cards")
                {
                    result.Add(c);
                }
            }
            return result;
        }
        private string ConvertDeckToText(IDeck deck)
        {
            if (deck == null)
                return "";
            string text = "";
            foreach (var sec in deck.Sections)
            {
                foreach (var card in sec.Cards)
                {
                    text += (card.Quantity + "x " + card.Name);
                    text += Environment.NewLine;
                }
            }
            return text;
        }
        public void OnClick(IDeckBuilderPluginController con)
        {
            var curDeck = con.GetLoadedDeck();
            var game = con.Games.Games.FirstOrDefault(x => x.Name=="Duel Masters");
            if (game == null)
            {
                MessageBox.Show("Duel Masters not installed!");
                return;
            }
            con.SetLoadedGame(game);
            var loadedDeck = con.GetLoadedDeck();
            string input = SimpleInputBox.Show("Paste Deck", "Enter your decklist:", ConvertDeckToText(loadedDeck));
            if(input == null)
                return;
            var d = game.CreateDeck();
            var allCards = game.AllCards().Select(c => new NormalizedCard{Card = c,NormalizedName = c.Name.ToLower()}).ToList();
            var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var secArray = d.Sections.ToArray();
            foreach (var line in lines)
            {
                var i = 0;
                if (line == "Hyperspatial Deck" || line == "Super Gacharange Zone")
                    if (i < secArray.Length)
                    {
                        i++;
                    }
                if (line == "Main" || line == "Duel Masters")
                    continue;
                var parsed = ParseLine(line);
                if (parsed == null)
                    continue;
                if (CardAliases.ContainsKey(parsed.Name))
                {
                    parsed.Name = CardAliases[parsed.Name];
                }
                var match = FindBestMatch(parsed.Name, allCards);
                if (match != null)
                {
                    var multiCard = match.ToMultiCard(parsed.Count);
                    secArray[i].Cards.AddCard(multiCard);
                }
                else
                {
                    MessageBox.Show($"No match for:\n{line}");
                }
            }
            con.LoadDeck(d);
        }
    }
}
