using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class MyXMLWorkerHelper
    {
        public static ElementList ParseToElementList(string html, string css)
        {
            ICSSResolver iCSSResolver = new StyleAttrCSSResolver();
            if (css != null)
            {
                ICssFile cSS = XMLWorkerHelper.GetCSS(new MemoryStream(Encoding.Default.GetBytes(css)));
                iCSSResolver.AddCss(cSS);
            }

            MyFontsProvider fontProvider = new MyFontsProvider();
            CssAppliers cssAppliers = new CssAppliersImpl(fontProvider);
            //CssAppliers cssAppliers = new CssAppliersImpl(FontFactory.FontImp);
            HtmlPipelineContext htmlPipelineContext = new HtmlPipelineContext(cssAppliers);
            htmlPipelineContext.SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            htmlPipelineContext.AutoBookmark(false);
            ElementList elementList = new ElementList();
            ElementHandlerPipeline next = new ElementHandlerPipeline(elementList, null);
            HtmlPipeline next2 = new HtmlPipeline(htmlPipelineContext, next);
            CssResolverPipeline pipeline = new CssResolverPipeline(iCSSResolver, next2);
            XMLWorker listener = new XMLWorker(pipeline, true);
            XMLParser xMLParser = new XMLParser(listener);

            //因为XMLWork不支持html的单标签，所以要对单标签进行过滤。不然就会报错：Invalid nested tag div found, expected closing tag br
            html = html.Replace("<br>", "").Replace("<hr>", "").Replace("<img>", "").Replace("<param>", "")
                    .Replace("<link>", "");

            //xMLParser.Parse(new MemoryStream(Encoding.Default.GetBytes(html)));
            xMLParser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(html)));
            return elementList;
        }
    }

    public class MyFontsProvider : XMLWorkerFontProvider
    {
        public MyFontsProvider()
        {
          
        }

        public override Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color)
        {
            string arialFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialuni.ttf");
            BaseFont basefont = BaseFont.CreateFont(
              arialFontPath,
              BaseFont.IDENTITY_H,
              BaseFont.NOT_EMBEDDED);
            Font font = new Font(basefont);
            return font;
        }
    }
}
