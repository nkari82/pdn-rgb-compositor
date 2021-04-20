using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;


namespace RGBCompositorPlugin
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=113646");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "RGB Compositor")]
    public class RGBCompositor : PropertyBasedEffect
    {
        private Surface rSurface;
        private Surface gSurface;
        private Surface bSurface;
        private Surface aSurface;
        private string rPath;
        private string gPath;
        private string bPath;
        private string aPath;
        private int input1;
        private int input2;
        private int input3;
        private int input4;

        private static readonly Bitmap StaticIcon = new Bitmap(typeof(RGBCompositor), "RGBCompositor.png");

        public RGBCompositor()
          : base("RGB Compositor", StaticIcon, "Color", new EffectOptions() { Flags = EffectFlags.Configurable })
        {
        }

        private enum PropertyNames
        {
            RedFile,
            GreenFile,
            BlueFile,
            AlphaFile,
            Input1,
            Input2,
            Input3,
            Input4,
        }

        private enum InputColor
        {
            Blue,
            Green,
            Red,
            Alpha,
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                StaticListChoiceProperty.CreateForEnum(PropertyNames.Input1, InputColor.Red, false),
                new StringProperty(PropertyNames.RedFile, string.Empty),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.Input2, InputColor.Red, false),
                new StringProperty(PropertyNames.GreenFile, string.Empty),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.Input3, InputColor.Red, false),
                new StringProperty(PropertyNames.BlueFile, string.Empty),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.Input4, InputColor.Red, false),
                new StringProperty(PropertyNames.AlphaFile, string.Empty),
            };

            return new PropertyCollection(props);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Input1, ControlInfoPropertyNames.DisplayName, "Channel");
            configUI.SetPropertyControlValue(PropertyNames.Input2, ControlInfoPropertyNames.DisplayName, "Channel");
            configUI.SetPropertyControlValue(PropertyNames.Input3, ControlInfoPropertyNames.DisplayName, "Channel");
            configUI.SetPropertyControlValue(PropertyNames.Input4, ControlInfoPropertyNames.DisplayName, "Channel");

            configUI.SetPropertyControlValue(PropertyNames.RedFile, ControlInfoPropertyNames.DisplayName, "Red File");
            configUI.SetPropertyControlValue(PropertyNames.RedFile, ControlInfoPropertyNames.FileTypes, new string[] { "bmp", "gif", "jpg", "jpeg", "png" });
            configUI.SetPropertyControlType(PropertyNames.RedFile, PropertyControlType.FileChooser);

            configUI.SetPropertyControlValue(PropertyNames.GreenFile, ControlInfoPropertyNames.DisplayName, "Green File");
            configUI.SetPropertyControlValue(PropertyNames.GreenFile, ControlInfoPropertyNames.FileTypes, new string[] { "bmp", "gif", "jpg", "jpeg", "png" });
            configUI.SetPropertyControlType(PropertyNames.GreenFile, PropertyControlType.FileChooser);

            configUI.SetPropertyControlValue(PropertyNames.BlueFile, ControlInfoPropertyNames.DisplayName, "Blue File");
            configUI.SetPropertyControlValue(PropertyNames.BlueFile, ControlInfoPropertyNames.FileTypes, new string[] { "bmp", "gif", "jpg", "jpeg", "png" });
            configUI.SetPropertyControlType(PropertyNames.BlueFile, PropertyControlType.FileChooser);

            configUI.SetPropertyControlValue(PropertyNames.AlphaFile, ControlInfoPropertyNames.DisplayName, "Alpha File");
            configUI.SetPropertyControlValue(PropertyNames.AlphaFile, ControlInfoPropertyNames.FileTypes, new string[] { "bmp", "gif", "jpg", "jpeg", "png" });
            configUI.SetPropertyControlType(PropertyNames.AlphaFile, PropertyControlType.FileChooser);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            rPath = newToken.GetProperty<StringProperty>(PropertyNames.RedFile).Value;
            gPath = newToken.GetProperty<StringProperty>(PropertyNames.GreenFile).Value;
            bPath = newToken.GetProperty<StringProperty>(PropertyNames.BlueFile).Value;
            aPath = newToken.GetProperty<StringProperty>(PropertyNames.AlphaFile).Value;
            input1 = (int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Input1).Value;
            input2 = (int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Input2).Value;
            input3 = (int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Input3).Value;
            input4 = (int)newToken.GetProperty<StaticListChoiceProperty>(PropertyNames.Input4).Value;

            Bitmap rImage = null;
            if (File.Exists(rPath))
            {
                try
                {
                    rImage = new Bitmap(rPath);
                }
                catch
                {
                }
            }

            if (rSurface != null)
            {
                rSurface.Dispose();
                rSurface = null;
            }

            if (rImage != null && rImage?.Size == srcArgs.Size)
            {
                rSurface = Surface.CopyFromBitmap(rImage);
            }

            rImage?.Dispose();

            Bitmap gImage = null;
            if (File.Exists(gPath))
            {
                try
                {
                    gImage = new Bitmap(gPath);
                }
                catch
                {
                }
            }

            if (gSurface != null)
            {
                gSurface.Dispose();
                gSurface = null;
            }

            if (gImage != null && gImage.Size == srcArgs.Size)
            {
                gSurface = Surface.CopyFromBitmap(gImage);
            }

            gImage?.Dispose();

            Bitmap bImage = null;
            if (File.Exists(bPath))
            {
                try
                {
                    bImage = new Bitmap(bPath);
                }
                catch
                {
                }
            }

            if (bSurface != null)
            {
                bSurface.Dispose();
                bSurface = null;
            }

            if (bImage != null && bImage.Size == srcArgs.Size)
            {
                bSurface = Surface.CopyFromBitmap(bImage);
            }

            bImage?.Dispose();

            Bitmap aImage = null;
            if (File.Exists(aPath))
            {
                try
                {
                    aImage = new Bitmap(aPath);
                }
                catch
                {
                }
            }

            if (aSurface != null)
            {
                aSurface.Dispose();
                aSurface = null;
            }

            if (aImage != null && aImage?.Size == srcArgs.Size)
            {
                aSurface = Surface.CopyFromBitmap(aImage);
            }

            aImage?.Dispose();

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) 
                return;

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, renderRects[i]);
            }
        }

        private void Render(Surface dst, Rectangle rect)
        {
            bool rLoaded = rSurface != null;
            bool gLoaded = gSurface != null;
            bool bLoaded = bSurface != null;
            bool aLoaded = aSurface != null;

            if (IsCancelRequested)
                return;

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    var color = dst[x, y];
                    dst[x, y] = ColorBgra.FromBgra(
                        bLoaded ? bSurface[x, y][input1] : color.B,
                        gLoaded ? gSurface[x, y][input2] : color.G,
                        rLoaded ? rSurface[x, y][input3] : color.R,
                        aLoaded ? aSurface[x, y][input4] : color.A);
                }
            }
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                rSurface?.Dispose();
                gSurface?.Dispose();
                bSurface?.Dispose();
                aSurface?.Dispose();
            }

            base.OnDispose(disposing);
        }
    }
}
