using PinkPact.Controls;
using PinkPact.Controls.Specific;
using PinkPact.Externals;
using PinkPact.Helpers;
using PinkPact.Playback;
using PinkPact.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinkPact
{
    /// <summary>
    /// Interaction logic for DayOne.xaml
    /// </summary>
    public partial class DayOne : UserControl
    {
        Image youImg;
        Grid[] characters;
        int selectedChar = -1,
            selectedOutfit = 0,
            selectedBubble = 0;

        string charName;

        bool outfitCycling = false;

        readonly MainWindow parent;

        public DayOne(MainWindow parent)
        {
            InitializeComponent();
            characters = new Grid[] { layla_option, raq_option, billie_option };
        }

        public async Task Day1(bool followUp = true)
        {
            viewport.Children.Clear();
            await viewport.Children.Add(introGrid);

            introGrid.Opacity = 1;
            introGrid.Visibility = Visibility.Visible;

            await 500;
            await introTitle.SetAsync("(s3)[DAY 1]");
            await introTitle.FadeOut(2000);
            await 1500;
            viewport.Children.Clear();

            if (followUp) await ShowLetter();
        }

        public async Task ShowLetter(bool followUp = true)
        {
            var bg = new Rectangle() { Fill = Brushes.Black, Stretch = Stretch.Fill };
            var bg_img = ResourceHelper.GetImageResource("Resources/Backgrounds/kunt.png");
            bg_img.Opacity = 0;

            viewport.Effect = new Vignette2Effect() { Color = Brushes.Black.Color, Intensity = 3 };

            var transforms = new TransformGroup() { Children = new TransformCollection() { new ScaleTransform(2.1, 2.1), new TranslateTransform(), new RotateTransform() { CenterX = 0.5, CenterY = 0.5 } } };
            var letter = new Letter() { RenderTransform = transforms, Opacity = 0 };

            var msg = ResourceHelper.GetImageResource("Resources/Others/message.png");
            msg.RenderTransform = new TranslateTransform();
            msg.Cursor = Cursors.Hand;
            msg.Height = 150;
            msg.Opacity = 0;

            await viewport.Children.Add(bg);
            await viewport.Children.Add(bg_img);
            await viewport.Children.Add(letter);
            await viewport.Children.Add(msg);

            Audio.PlayResource("Resources/Sounds/Extra/letter_appear.mp3");

            _ = OpacityProperty.Animate<DoubleAnimation, double>(msg, 0, 1, 300, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            await TranslateTransform.YProperty.Animate<DoubleAnimation, double>(msg.RenderTransform, 0, -50, 300, 60, true, true, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

            msg.RenderTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, 15, TimeSpan.FromMilliseconds(500)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true, EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut } });
            await msg.AwaitClick();

            await TranslateTransform.YProperty.Animate<DoubleAnimation, double>(0, -1000, 300, easing: new SineEase() { EasingMode = EasingMode.EaseIn });
            viewport.Children.Remove(msg);

            Audio.PlayResource("Resources/Sounds/Extra/letter_open.ogg", channel: "sfx");

            _ = OpacityProperty.Animate<DoubleAnimation, double>(letter, 0, 1, 2000, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            _ = OpacityProperty.Animate<DoubleAnimation, double>(bg_img, 0, 0.3, 1750, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            _ = TranslateTransform.YProperty.Animate<DoubleAnimation, double>(transforms.Children[1], 100, 0, 1750, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            await RotateTransform.AngleProperty.Animate<DoubleAnimation, double>(transforms.Children[2], 10, -3, 1750, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

            letter.Cursor = Cursors.Hand;
            await letter.AwaitClick();

            _ = OpacityProperty.Animate<DoubleAnimation, double>(letter, letter.Opacity, 0, 1500, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            _ = TranslateTransform.YProperty.Animate<DoubleAnimation, double>(transforms.Children[1], 0, -100, 1250, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            await RotateTransform.AngleProperty.Animate<DoubleAnimation, double>(transforms.Children[2], -3, -7, 1250, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

            await 500;
            viewport.Children.Remove(letter);
            _ = OpacityProperty.Animate<DoubleAnimation, double>(bg_img, 0.3, 1, 2000, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            await Vignette2Effect.IntensityProperty.Animate<DoubleAnimation, double>(viewport.Effect, 3, 0.5, 2000, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

            if (followUp) await CharacterSelection();
        }

        public async Task CharacterSelection(bool followUp = true)
        {
            viewport.Children.Clear();
            var bg_init = ResourceHelper.GetImageResource("Resources/Backgrounds/kunt.png");

            await viewport.Children.Add(bg_init);
            await viewport.Children.Add(chooseGrid);

            var arrow_anim = new DoubleAnimation(0, -20, TimeSpan.FromMilliseconds(500)) { EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut }, AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            char_larrow.RenderTransform.BeginAnimation(TranslateTransform.XProperty, arrow_anim);

            arrow_anim.To = 20;
            char_rarrow.RenderTransform.BeginAnimation(TranslateTransform.XProperty, arrow_anim);

            chooseGrid.Visibility = Visibility.Visible;
            await OpacityProperty.Animate<DoubleAnimation, double>(chooseGrid, 0, 1, 750, 12, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            bg_init.Opacity = 0.7;

            await chooseCharTitle.SetAsync("CHOOSE YOUR (c#ff99dc-#ff3bbc)[CHARACTER]");

            var nextChar = () => SelectCharacter((selectedChar + 1 > characters.Length - 1 ? 0 : selectedChar + 1));
            var prevChar = () => SelectCharacter((selectedChar - 1 < 0 ? characters.Length - 1 : selectedChar - 1));

            HotkeyManager.Add("choose", nextChar, Key.Right);
            HotkeyManager.Add("choose", nextChar, Key.D);

            HotkeyManager.Add("choose", prevChar, Key.Left);
            HotkeyManager.Add("choose", prevChar, Key.A);

            while (selectedChar < 0) await 5;
            chooseCharTitle.SequentialFading = false;
            await chooseCharTitle.AppendAsync(" (c#bda0b8,f0.5,fd1)[\\(Press Enter\\)]");
            await HotkeyManager.Await(Key.Enter);

            var choice = characters[selectedChar].Children[0] as Image;

            characters[selectedChar].Children.RemoveAt(0);
            await viewport.Children.Add(choice);

            charName = selectedChar == 0 ? "Layla" : selectedChar == 1 ? "Raquel" : "Billie";

            HotkeyManager.Remove("choose");
            if (followUp) await OutfitSelection();
        }

        public async Task OutfitSelection(bool followUp = true)
        {
            viewport.Children.Insert(viewport.Children.IndexOf(chooseGrid), outfitGrid);
            charBase.Source = ResourceHelper.GetImageResource($"Resources/Outfits/{(selectedChar == 0 ? "Layla" : selectedChar == 1 ? "Raquel" : "Billie")}/{(selectedChar == 0 ? "L" : selectedChar == 1 ? "R" : "B")}_base.png").Source;
            
            _ = SetOutfit(selectedChar, 0, 0, true, true);
            _ = SetOutfit(selectedChar, 1, 0, true, true);
            _ = SetOutfit(selectedChar, 2, 0, true, true);
            _ = SetOutfit(selectedChar, 3, 0, true, true);

            SelectOutfitBubble(0);

            var arrow_anim = new DoubleAnimation(0, -20, TimeSpan.FromMilliseconds(500)) { RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true, EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut } };
            outfitLeftArrow.RenderTransform.BeginAnimation(TranslateTransform.XProperty, arrow_anim);

            arrow_anim.To = 20;
            outfitRightArrow.RenderTransform.BeginAnimation(TranslateTransform.XProperty, arrow_anim);

            arrow_anim.From = 5;
            arrow_anim.To = -5;
            arrow_anim.Duration = TimeSpan.FromSeconds(2);
            Timeline.SetDesiredFrameRate(arrow_anim, 25);
            charModelGrid.RenderTransform.BeginAnimation(TranslateTransform.YProperty, arrow_anim);

            var choice = viewport.Children[viewport.Children.Count - 1] as Image;
            _ = OpacityProperty.Animate<DoubleAnimation, double>(chooseGrid, chooseGrid.Opacity, 0, 1000, 45, easing: new CubicEase() { EasingMode = EasingMode.EaseOut });
            await MarginProperty.Animate<ThicknessAnimation, Thickness>(choice, choice.Margin, new Thickness(0, 0, 0, 0), 1000, 45, easing: new CubicEase() { EasingMode = EasingMode.EaseOut });

            viewport.Children.Remove(chooseGrid);
            outfitGrid.Opacity = 0;
            outfitGrid.Visibility = Visibility.Visible;
            _ = OpacityProperty.Animate<DoubleAnimation, double>(outfitGrid, 0, 1, 500, 45, easing: new CubicEase() { EasingMode = EasingMode.EaseOut });
            await OpacityProperty.Animate<DoubleAnimation, double>(choice, choice.Opacity, 0, 500, 45, easing: new CubicEase() { EasingMode = EasingMode.EaseOut });
            viewport.Children.Remove(choice);

            Action nextOutfit = () => { if (!outfitCycling) _ = SetOutfit(selectedChar, selectedBubble, selectedOutfit = NextOutfitIndex(selectedOutfit), true); };
            Action prevOutfit = () => { if (!outfitCycling) _ = SetOutfit(selectedChar, selectedBubble, selectedOutfit = NextOutfitIndex(selectedOutfit, true), false); };

            HotkeyManager.Add("choose", nextOutfit, Key.Right);
            HotkeyManager.Add("choose", nextOutfit, Key.D);

            HotkeyManager.Add("choose", prevOutfit, Key.Left);
            HotkeyManager.Add("choose", prevOutfit, Key.A);

            Action nextBubble = () => SelectOutfitBubble(selectedBubble = (selectedBubble + 1 > 3 ? 0 : selectedBubble + 1));
            Action prevBubble = () => SelectOutfitBubble(selectedBubble = (selectedBubble - 1 < 0 ? 3 : selectedBubble - 1));

            HotkeyManager.Add("choose", prevBubble, Key.Up);
            HotkeyManager.Add("choose", prevBubble, Key.W);

            HotkeyManager.Add("choose", nextBubble, Key.Down);
            HotkeyManager.Add("choose", nextBubble, Key.S);

            await HotkeyManager.Await(Key.Enter);
            HotkeyManager.Remove("choose");

            viewport.Children.Clear();
            outfitGrid.Children.Remove(charModelGrid);
            
            await viewport.Children.Add(charModelGrid);
            await viewport.Children.Add(ashleyConfirmationText);
            await viewport.Children.Add(ashleySeeYouText);

            await 750;
            await MarginProperty.Animate<ThicknessAnimation, Thickness>(charModelGrid, charModelGrid.Margin, new Thickness(0, 0, 1000, 0), 1500, 25, easing: new CubicEase() { EasingMode = EasingMode.EaseOut });

            ashleyConfirmationText.Opacity = ashleySeeYouText.Opacity = 1;
            ashleyConfirmationText.Visibility = ashleySeeYouText.Visibility = Visibility.Visible;

            await ashleyConfirmationText.SetAsync($"(i,w4)[Amazing] outfit...\n(c#9c0000,s0.75,fd150)[{charName}].");
            await 1000;
            await ashleySeeYouText.SetAsync("See you in a minute! <3");
            await 1200;

            var effect = viewport.Effect;
            viewport.Effect = new ChunkGlitchEffect() { Intensity = 0.3, Seed = MathHelper.Random(1, 1000) };
            await 750;

            viewport.Effect = effect;
            viewport.Children.Clear();

            if (followUp) await BusScene();
        }

        public async Task BusScene()
        {
            viewport.Children.Clear();

            var bg = ResourceHelper.GetImageResource("Resources/Backgrounds/bus.jpg");
            bg.Opacity = 0;

            await viewport.Children.Add(bg);
            await OpacityProperty.Animate<DoubleAnimation, double>(bg, 0, 1, 1000, 25, easing: new SineEase() { EasingMode = EasingMode.EaseOut });

            var text = new FormattedTextBlock();
            text.FontSize = 130;
            text.VerticalAlignment = VerticalAlignment.Top;
            text.HorizontalAlignment = HorizontalAlignment.Left;
            text.FontFamily = FontHelper.RetrieveFontFamily("Mini Pixel-7", "Resources/Fonts");
            text.Margin = new Thickness(40, 20, 0, 0);
            text.Foreground = Brushes.White;
            text.FontSize = 110;

            text.Opacity = 1;
            text.Visibility = Visibility.Visible;
            await viewport.Children.Add(text);

            await 1000;
            await text.SetAsync("Next stop... (w4,c#ff7dfd)[Sorority Mansion].");

            await viewport.Children.Add(dialogueSkipArrow);
            dialogueSkipArrow.Opacity = 1;

            var anim = new DoubleAnimation(0, 10, TimeSpan.FromSeconds(1)) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }, RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true };
            Timeline.SetDesiredFrameRate(anim, 15);
            (dialogueSkipArrow.RenderTransform as TransformGroup).Children[0].BeginAnimation(TranslateTransform.XProperty, anim);

            await HotkeyManager.Await(Key.Enter);
            await text.SetAsync("");
            viewport.Children.Remove(dialogueSkipArrow);

            await Dialogue((charName, null, "..."));

            var bgMansion = ResourceHelper.GetImageResource("Resources/Backgrounds/manor_day.jpg");
            bgMansion.Stretch = Stretch.UniformToFill;
            bgMansion.Opacity = 0;

            await viewport.Children.Add(bgMansion);
            await OpacityProperty.Animate<DoubleAnimation, double>(bgMansion, 0, 1, 1000, 25, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            viewport.Children.Remove(bg);

            await 1000;
            await Dialogue((charName, null, $"Holy (w4)[shi{(charName == "Layla" ? "-" : "t!")}]"),
                           (charName, null, charName == "Billie" ? "It's so...(fd750)[‎ ]extra.." :
                                            charName == "Raquel" ? "Damn, that's exactly what I had pictured!" :
                                            "--crap.(fd350)[‎ ]Sorry.. It just looks so..(fd750)[‎ ]beautiful!"));

            var bgDoor = ResourceHelper.GetImageResource("Resources/Backgrounds/outside_door.jpg");
            bgDoor.Stretch = Stretch.UniformToFill;
            bgDoor.Opacity = 0;

            await viewport.Children.Add(bgDoor);
            await OpacityProperty.Animate<DoubleAnimation, double>(bgDoor, 0, 1, 1000, 25, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            viewport.Children.Remove(bgMansion);

            await 750;
            await Audio.PlayResource("Resources/Sounds/Doors/doorbell_big.ogg", channel: "sfx").AwaitSinglePlayback();
            await 1000;
            await Audio.PlayResource("Resources/Sounds/Footsteps/steps_muffled.ogg", channel: "sfx").AwaitSinglePlayback();
            await Audio.PlayResource("Resources/Sounds/Doors/creak.ogg", channel: "sfx").AwaitSinglePlayback();

            await Dialogue(("Ashley", ResourceHelper.GetImageResource("Resources/Dialogue/Ashley/a_surprised.png"), "Oh my god!"),
                           ("Ashley", ResourceHelper.GetImageResource("Resources/Dialogue/Ashley/a_happy.png"), "Hey diva!!(fd350)[‎ ]I'm so glad you're here!"));

            viewport.Children.Clear();

            await 2000;
            Environment.Exit(0);
        }

        public async Task Dialogue(params (string author, Image person, string text)[] lines)
        {
            var duration = dialogueText.FadingDuration;
            dialogueText.FadingDuration = 0;
            dialogueText.Text = "";
            dialogueText.FadingDuration = duration;
            dialoguePortrait.Opacity = 0;
            dialoguePortrait.Source = null;

            dialogueBox.Opacity = 0;
            viewport.Children.Remove(dialogueBox);
            await viewport.Children.Add(dialogueBox);

            var anim = new DoubleAnimation(0, 10, TimeSpan.FromSeconds(1)) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }, RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true };
            Timeline.SetDesiredFrameRate(anim, 15);

            (dialogueBoxSkipArrow.RenderTransform as TransformGroup).Children[0].BeginAnimation(TranslateTransform.XProperty, anim);

            dialogueAuthor.Text = lines[0].author;
            await OpacityProperty.Animate<DoubleAnimation, double>(dialogueBox, 0, 1, 350, 25, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            for (int i = 0; i < lines.Length; i++)
            {
                dialogueAuthor.Text = lines[i].author;
                dialoguePortrait.Source = lines[i].person?.Source;
                if (lines[i].person != null && (i - 1 < 0 || lines[i - 1].author != lines[i].author)) _ = OpacityProperty.Animate<DoubleAnimation, double>(dialoguePortrait, 0, 1, 350, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
                await dialogueText.SetAsync(lines[i].text);
                dialogueBoxSkipArrow.Opacity = 1;

                await HotkeyManager.Await(Key.Enter);
                dialogueBoxSkipArrow.Opacity = 0;

                Task t = null;
                if (lines[i].person != null && (i + 1 >= lines.Length || lines[i + 1].author != lines[i].author)) t = OpacityProperty.Animate<DoubleAnimation, double>(dialoguePortrait, 1, 0, 350, 45, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
                if (i == lines.Length - 1 && t != null) await t;
            }

            await OpacityProperty.Animate<DoubleAnimation, double>(dialogueBox, 1, 0, 250, 25, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            viewport.Children.Remove(dialogueBox);
        }

        public void SelectCharacter(int index)
        {
            selectedChar = index;
            for (int i = 0; i < characters.Length; i++)
            {
                //characters[i].Opacity = i == index ? 1 : 0;
                _ = OpacityProperty.Animate<DoubleAnimation, double>(characters[i], characters[i].Opacity, i == index ? 1 : 0, 200, 24, easing: new SineEase() { EasingMode = EasingMode.EaseOut });
            }
        }

        public async Task SetOutfit(int charIndex, int partIndex, int outfitIndex, bool toRight, bool instant = false)
        {
            outfitCycling = true;
            var replica = ResourceHelper.GetImageResource($"Resources/Outfits/{(charIndex == 0 ? "Layla" : charIndex == 1 ? "Raquel" : "Billie")}/{(charIndex == 0 ? "L" : charIndex == 1 ? "R" : "B")}_{(partIndex == 0 ? "hair" : partIndex == 1 ? "shirt" : partIndex == 2 ? "pants" : "shoes")}{outfitIndex + 1}.png");
            replica.Width = 450;
            replica.Opacity = 0;
            replica.RenderTransform = new TranslateTransform(toRight ? -300 : 300, 0);

            var original = partIndex == 0 ? charHair : partIndex == 1 ? charTop : partIndex == 2 ? charPants : charShoes;
            charModelGrid.Children.Insert(charModelGrid.Children.IndexOf(original), replica);

            original.RenderTransform = new TranslateTransform();

            if (!instant)
            {
                _ = OpacityProperty.Animate<DoubleAnimation, double>(replica, 0, 1, 250, 25, easing: new ExponentialEase() { EasingMode = EasingMode.EaseOut });
                _ = OpacityProperty.Animate<DoubleAnimation, double>(original, 1, 0, 250, 25, easing: new ExponentialEase() { EasingMode = EasingMode.EaseOut });
                _ = TranslateTransform.XProperty.Animate<DoubleAnimation, double>(replica.RenderTransform, toRight ? -300 : 300, 0, 250, 25, easing: new ExponentialEase() { EasingMode = EasingMode.EaseOut });
                await TranslateTransform.XProperty.Animate<DoubleAnimation, double>(original.RenderTransform, 0, toRight ? 300 : -300, 250, 25, easing: new ExponentialEase() { EasingMode = EasingMode.EaseOut });
            }

            charModelGrid.Children.Remove(replica);
            original.RenderTransform.BeginAnimation(TranslateTransform.XProperty, null);
            original.BeginAnimation(OpacityProperty, null);

            (original.RenderTransform as TranslateTransform).X = 0;
            original.Opacity = 1;

            if (partIndex == 0) charHair.Source = replica.Source;
            else if (partIndex == 1) charTop.Source = replica.Source;
            else if (partIndex == 2) charPants.Source = replica.Source;
            else charShoes.Source = replica.Source;
            outfitCycling = false;
        }

        public void SelectOutfitBubble(int index)
        {
            for (int i = 0; i < bubbles.Children.Count; i++)
            {
                (bubbles.Children[i] as StackPanel).Children[0].Opacity = i == index ? 1 : 0.5;
                (bubbles.Children[i] as StackPanel).Children[1].Opacity = i == index ? 1 : 0;
            }
        }

        public int NextOutfitIndex(int index, bool prev = false)
        {
            return prev ? (selectedOutfit - 1 < 0 ? ((selectedChar == 0 && selectedBubble == 2) || (selectedChar == 1 && selectedBubble == 3) ? 1 : 2) : selectedOutfit - 1)
                        : (selectedOutfit + 1 > ((selectedChar == 0 && selectedBubble == 2) || (selectedChar == 1 && selectedBubble == 3) ? 1 : 2) ? 0 : selectedOutfit + 1);
        }
    }
}