﻿using Aspose.Font.Glyphs;
using Aspose.Font.Rendering;
using Aspose.Font.RenderingPath;
using Aspose.Font.Sources;
using Aspose.Font.Ttf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspose.Font.Examples.WorkingWithTrueTypeAndOpenTypeFonts
{
    class RenderingText
    {
        public static void Run()
        {
            //ExStart: 2
            string dataDir = RunExamples.GetDataDir_Data();

            string fileName1 = dataDir + "Montserrat-Bold.ttf"; //Font file name with full path
            FontDefinition fd1 = new FontDefinition(FontType.TTF, new FontFileDefinition("ttf", new FileSystemStreamSource(fileName1)));
            TtfFont ttfFont1 = Aspose.Font.Font.Open(fd1) as TtfFont;
            
            string fileName2 = dataDir + "Lora-Bold.ttf"; //Font file name with full path
            FontDefinition fd2 = new FontDefinition(FontType.TTF, new FontFileDefinition("ttf", new FileSystemStreamSource(fileName2)));
            TtfFont ttfFont2 = Aspose.Font.Font.Open(fd2) as TtfFont;

            DrawText("Hello world", ttfFont1, 14, Brushes.White, Brushes.Black, dataDir + "hello1_montserrat_out.jpg");
            DrawText("Hello world", ttfFont2, 14, Brushes.Yellow, Brushes.Red, dataDir + "hello2_lora_out.jpg");
            //ExEnd: 2
        }

        //ExStart: 1
        class GlyphOutlinePainter : IGlyphOutlinePainter
        {
            private System.Drawing.Drawing2D.GraphicsPath _path;
            private System.Drawing.PointF _currentPoint;

            public GlyphOutlinePainter(System.Drawing.Drawing2D.GraphicsPath path)
            {
                _path = path;
            }

            public void MoveTo(MoveTo moveTo)
            {
                _path.CloseFigure();
                _currentPoint.X = (float)moveTo.X;
                _currentPoint.Y = (float)moveTo.Y;
            }

            public void LineTo(LineTo lineTo)
            {
                float x = (float)lineTo.X;
                float y = (float)lineTo.Y;
                _path.AddLine(_currentPoint.X, _currentPoint.Y, x, y);
                _currentPoint.X = x;
                _currentPoint.Y = y;
            }

            public void CurveTo(CurveTo curveTo)
            {
                float x3 = (float)curveTo.X3;
                float y3 = (float)curveTo.Y3;

                _path.AddBezier(
                          _currentPoint.X,
                          _currentPoint.Y,
                          (float)curveTo.X1,
                          (float)curveTo.Y1,
                          (float)curveTo.X2,
                          (float)curveTo.Y2,
                          x3,
                          y3);

                _currentPoint.X = x3;
                _currentPoint.Y = y3;
            }

            public void ClosePath()
            {
                _path.CloseFigure();
            }
        }
        

        static void DrawText(string text, IFont font, double fontSize,
                    Brush backgroundBrush, Brush textBrush, string outFile)
        {
            //Get glyph identifiers for every symbol in text line
            GlyphId[] gids = new GlyphId[text.Length];
            for (int i = 0; i < text.Length; i++)
                gids[i] = font.Encoding.DecodeToGid(text[i]);
            // set common drawing settings
            double dpi = 300;

            double resolutionCorrection = dpi / 72; // 72 is font's internal dpi
            // prepare output bitmap
            Bitmap outBitmap = new Bitmap(960, 720);
            outBitmap.SetResolution((float)dpi, (float)dpi);
            Graphics outGraphics = Graphics.FromImage(outBitmap);
            outGraphics.FillRectangle(backgroundBrush, 0, 0, outBitmap.Width, outBitmap.Height);
            outGraphics.SmoothingMode = SmoothingMode.HighQuality;
            //declare coordinate variables and previous gid
            GlyphId previousGid = null;
            double glyphXCoordinate = 0;
            double glyphYCoordinate = fontSize * resolutionCorrection;
            //loop which paints every glyph in gids
            foreach (GlyphId gid in gids)
            {
                // if the font contains the gid
                if (gid != null)
                {
                    Glyph glyph = font.GlyphAccessor.GetGlyphById(gid);
                    if (glyph == null)
                        continue;

                    // path that accepts drawing instructions
                    GraphicsPath path = new GraphicsPath();

                    // Create IGlyphOutlinePainter implementation
                    GlyphOutlinePainter outlinePainter = new GlyphOutlinePainter(path);

                    // Create the renderer
                    Aspose.Font.Renderers.IGlyphRenderer renderer = new
                        Aspose.Font.Renderers.GlyphOutlineRenderer(outlinePainter);

                    // get common glyph properties
                    double kerning = 0;

                    // get kerning value
                    if (previousGid != null)
                    {
                        kerning = (font.Metrics.GetKerningValue(previousGid, gid) /
                                   glyph.SourceResolution) * fontSize * resolutionCorrection;
                        kerning += FontWidthToImageWith(font.Metrics.GetGlyphWidth(previousGid),
                                glyph.SourceResolution, fontSize);
                    }

                    // glyph positioning - increase glyph X coordinate according to kerning distance
                    glyphXCoordinate += kerning;

                    // Glyph placement matrix
                    TransformationMatrix glyphMatrix =
                        new TransformationMatrix(
                            new double[]
                                    {
                                            fontSize*resolutionCorrection,
                                            0,
                                            0,
                                        // negative because of bitmap coordinate system begins from the top
                                            - fontSize*resolutionCorrection,
                                            glyphXCoordinate,
                                            glyphYCoordinate
                                    });

                    // render current glyph
                    renderer.RenderGlyph(font, gid, glyphMatrix);
                    // fill the path
                    path.FillMode = FillMode.Winding;
                    outGraphics.FillPath(textBrush, path);
                }
                //set current gid as previous to get correct kerning for next glyph
                previousGid = gid;
            }
            //Save results
            outBitmap.Save(outFile);
        }

        static double FontWidthToImageWith(double width, int fontSourceResulution, double fontSize, double dpi = 300)
        {
            double resolutionCorrection = dpi / 72; // 72 is font's internal dpi
            return (width / fontSourceResulution) * fontSize * resolutionCorrection;
        }
        //ExEnd: 1
    }
}
