﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using RequestReduce.Utilities.Quantizer;
using Xunit;

namespace RequestReduce.Facts.Integration
{
    public class ReducerFacts : IDisposable
    {
        private readonly IRRConfiguration config;

        public ReducerFacts()
        {
            config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var rrFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\RequestReduce.SampleWeb\\FactContent";
            config.SpritePhysicalPath = rrFolder;
        }

        [OutputTraceOnFailFact]
        public void WillReturnSavedCSS()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();
            var urls = "http://localhost:8877/Styles/style1.css::http://localhost:8877/Styles/style2.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            var result = reducer.Process(urls);

            Assert.Equal(result,
                         Directory.GetFiles(config.SpritePhysicalPath, key + "*").Single(x => x.EndsWith(".css")).Replace(
                             config.SpritePhysicalPath, config.SpriteVirtualPath).Replace("\\", "/"));
            reducer.Dispose();
        }

        [OutputTraceOnFailFact]
        public void WillSaveSprite()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();
            var urls = "http://localhost:8877/Styles/style1.css::http://localhost:8877/Styles/style2.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            var result = reducer.Process(urls);

            Assert.NotNull(Directory.GetFiles(config.SpritePhysicalPath,  key + "*").Single(x => x.EndsWith(".png")));
            reducer.Dispose();
        }

        [OutputTraceOnFailFact]
        public void WillContainImageWithPropperDimensions()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();
            var urls = "http://localhost:8877/Styles/style3.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            reducer.Process(urls);

            reducer.Dispose();
            using(var image = new Bitmap(Directory.GetFiles(config.SpritePhysicalPath, key + "*").Single(x => x.EndsWith(".png"))))
            {
                Assert.Equal(1040, image.Width);
                Assert.Equal(33, image.Height);
                image.Dispose();
            }
        }

        [Fact]
        [Trait("type", "manual_adhoc")]
        public void VsTest()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();
            var urls = "http://i1.qa.social.s-msft.com/contentservice/5c9312af-010f-4ea8-9114-9f3300f2c557/Msdn.css::http://i1.qa.social.s-msft.com/contentservice/271b0fc8-5324-44e5-b638-9dad00d725c4/Site.css::http://i1.qa.social.s-msft.com/contentservice/5eefc914-a55f-4d62-aabc-9f330122bf50/vstudio.css::http://i1.ppe.visualstudiogallery.msdn.s-msft.com/pageresource.css?groupname=visualstudiostyles&amp;v=2011.8.1.3571";

            reducer.Process(urls);

            var css = Directory.GetFiles(config.SpritePhysicalPath, "*.png");
            var newFile = "c:\\requestreduce\\requestreduce.sampleweb\\vstest\\vstest.png";
            File.Copy(css[0], newFile, true);
            reducer.Dispose();
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(config.SpritePhysicalPath, true);
            }
            catch(IOException)
            {
                Thread.Sleep(100);
                Directory.Delete(config.SpritePhysicalPath, true);
            }
            RRContainer.Current = null;
        }
    }
}
