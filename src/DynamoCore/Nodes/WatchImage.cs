﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using Image = System.Windows.Controls.Image;

namespace Dynamo.Nodes
{

    [NodeName("Watch Image")]
    [NodeDescription("Previews an image")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("image")]
    [IsDesignScriptCompatible]
    public class WatchImageCore : NodeModel, IWpfNode
    {
        //private ResultImageUI resultImageUI = new ResultImageUI();
        private Image image;

        public WatchImageCore()
        {
            InPortData.Add(new PortData("image", "image", typeof(System.Drawing.Bitmap)));
            OutPortData.Add(new PortData("image", "image", typeof(System.Drawing.Bitmap)));

            RegisterAllPorts();
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0])
            };

            return resultAst;
        }

        public void SetupCustomUIElements(dynNodeView nodeUi)
        {
            image = new Image
            {
                MaxWidth = 400,
                MaxHeight = 400,
                Margin = new Thickness(5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Name = "image1",
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            this.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName != "IsUpdated") return;
                var im = GetImageFromMirror();
                nodeUi.Dispatcher.Invoke(new Action<Bitmap>(SetImageSource), new object[] { im });
            };

            nodeUi.grid.Children.Add(image);
            image.SetValue(Grid.RowProperty, 2);
            image.SetValue(Grid.ColumnProperty, 0);
            image.SetValue(Grid.ColumnSpanProperty, 3);
        }

        private void SetImageSource(System.Drawing.Bitmap bmp)
        {
            // how to convert a bitmap to an imagesource http://blog.laranjee.com/how-to-convert-winforms-bitmap-to-wpf-imagesource/ 
            // TODO - watch out for memory leaks using system.drawing.bitmaps in managed code, see here http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/4e213af5-d546-4cc1-a8f0-462720e5fcde
            // need to call Dispose manually somewhere, or perhaps use a WPF native structure instead of bitmap?

            var hbitmap = bmp.GetHbitmap();
            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            image.Source =  imageSource;
        }

        private System.Drawing.Bitmap GetImageFromMirror()
        {
            if (this.InPorts[0].Connectors.Count == 0) return null;

            var mirror = dynSettings.Controller.EngineController.GetMirror(AstIdentifierForPreview.Name);

            if (null == mirror)
                return null;

            var data = mirror.GetData();

            if (data == null || data.IsNull) return null;
            if (data.Data is System.Drawing.Bitmap) return data.Data as System.Drawing.Bitmap;
            return null;
        }

        public override void UpdateRenderPackage()
        {
            //do nothing
            //a watch should not draw its outputs
        }

        public class ResultImageUI : INotifyPropertyChanged
        {
            private System.Windows.Media.ImageSource resultImage;
            public System.Windows.Media.ImageSource ResultImage
            {
                get { return resultImage; }

                set
                {
                    resultImage = value;
                    Notify("ResultImage");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, e);
            }

            protected void OnPropertyChanged(string propertyName)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }

            protected void Notify(string propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    try
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                    catch (Exception ex)
                    {
                        DynamoLogger.Instance.Log(ex.Message);
                        DynamoLogger.Instance.Log(ex.StackTrace);
                    }
                }
            }

        }

    }

}