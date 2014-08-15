using AForge.Vision.GlyphRecognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Math;

namespace SteeringCarFromAbove
{
    public class MarkerFinder
    {
        virtual public IDictionary<string, PositionAndOrientation> FindMarkers(List<ExtractedGlyphData> glyphs)
        {
            return glyphs.Where(x => x.RecognizedGlyph != null).ToDictionary(x => x.RecognizedGlyph.Name, x => GetPositionAndOrientationFromGlyph(x));
        }

        private PositionAndOrientation GetPositionAndOrientationFromGlyph(ExtractedGlyphData glyph)
        {
            double x = glyph.RecognizedQuadrilateral.Average(g => g.X);
            double y = glyph.RecognizedQuadrilateral.Average(g => g.Y);

            Matrix4x4 transformationMatrix = glyph.TransformationMatrix;
            float roll;
            float pitch;
            float yaw;
            transformationMatrix.ExtractYawPitchRoll(out yaw, out pitch, out roll);

            double xP = Math.Cos((double)yaw) * Math.Cos((double)pitch);
            double yP = Math.Sin((double)yaw) * Math.Cos((double)pitch);
            double zP = Math.Sin((double)pitch);

            //double angle = yaw * 180.0d / Math.PI + 180.0d;
            double angle = 180.0d - Math.Atan2(yP, zP) / Math.PI * 180.0d;
            return new PositionAndOrientation(x, y, angle);
        }
    }
}
