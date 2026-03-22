using System.Collections.Generic;

// AI に作らせたらなんも分からんかったので public だけ切り出した闇

namespace Elin.Plugin.Main.Models
{
    public class TextureUtility
    {
        #region function

        public static void Fill(Texture2D texture, Color color)
        {

            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }

        private static List<Vector2> CreateRegularPolygonPoints2D(int sides, float outerRadius, float innerRadius, float rotationDeg, int texSize)
        {
            var pts = new List<Vector2>();
            int stepCount = (innerRadius > 0f) ? sides * 2 : sides;
            float rot = rotationDeg * Mathf.Deg2Rad;
            for (int i = 0; i < stepCount; i++)
            {
                float angle = (Mathf.PI * 2f * i / stepCount) + rot;
                float r = (innerRadius > 0f && (i % 2 == 1)) ? innerRadius : outerRadius;
                float vx = Mathf.Cos(angle) * r;
                float vy = Mathf.Sin(angle) * r;
                // map from [-1,1] ish to pixel coords (centered)
                float px = (vx * 0.5f + 0.5f) * (texSize - 1);
                float py = (vy * 0.5f + 0.5f) * (texSize - 1);
                pts.Add(new Vector2(px, py));
            }
            return pts;
        }

        private static List<Vector2> CreateCrossPolygonPoints2D(float size, float thickness, int texSize, float rotationDeg)
        {
            // Create a single rectangular arm centered, then rotate
            float half = size * 0.5f;
            float t = thickness * 0.5f;
            var arm = new Vector2[]
            {
                new Vector2(-half, -t),
                new Vector2(half, -t),
                new Vector2(half, t),
                new Vector2(-half, t),
            };
            var pts = new List<Vector2>();
            var q = Quaternion.Euler(0, 0, rotationDeg);
            for (int i = 0; i < 4; i++)
            {
                var v = q * new Vector3(arm[i].x, arm[i].y, 0f);
                float px = (v.x * 0.5f + 0.5f) * (texSize - 1);
                float py = (v.y * 0.5f + 0.5f) * (texSize - 1);
                pts.Add(new Vector2(px, py));
            }
            return pts;
        }

        private static void DrawFilledPolygon(Texture2D texture, List<Vector2> poly, Color fill)
        {
            if (poly.Count < 3)
            {
                return;
            }

            int minX = texture.width - 1, minY = texture.height - 1, maxX = 0, maxY = 0;
            foreach (var p in poly)
            {
                int ix = Mathf.Clamp(Mathf.FloorToInt(p.x), 0, texture.width - 1);
                int iy = Mathf.Clamp(Mathf.FloorToInt(p.y), 0, texture.height - 1);
                if (ix < minX)
                {
                    minX = ix;
                }
                if (iy < minY)
                {
                    minY = iy;
                }
                if (ix > maxX)
                {
                    maxX = ix;
                }
                if (iy > maxY)
                {
                    maxY = iy;
                }
            }

            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), poly))
                    {
                        texture.SetPixel(x, y, fill);
                    }
                }
        }

        // Ray casting algorithm
        private static bool PointInPolygon(Vector2 p, List<Vector2> poly)
        {
            bool inside = false;
            int count = poly.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = poly[i];
                var pj = poly[j];
                bool intersect = ((pi.y > p.y) != (pj.y > p.y)) &&
                                 (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y + float.Epsilon) + pi.x);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        public static Texture2D CreateShapeTexture(MarkerShape shape, int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Fill(texture, new Color(0f, 0f, 0f, 0f));

            var fill = Color.white;
            switch (shape)
            {
                case MarkerShape.TriangleUp:
                    DrawFilledPolygon(texture, CreateRegularPolygonPoints2D(3, 0.46f, 0f, -30f, size), fill);
                    break;
                case MarkerShape.TriangleDown:
                    DrawFilledPolygon(texture, CreateRegularPolygonPoints2D(3, 0.46f, 0f, +30f, size), fill);
                    break;
                case MarkerShape.Star:
                    DrawFilledPolygon(texture, CreateRegularPolygonPoints2D(5, 0.46f, 0.22f, 90f, size), fill);
                    break;
                case MarkerShape.Pentagon:
                    DrawFilledPolygon(texture, CreateRegularPolygonPoints2D(5, 0.42f, 0f, 90f, size), fill);
                    break;
                case MarkerShape.Cross:
                    var crossA = CreateCrossPolygonPoints2D(0.9f, 0.28f, size, 45f);
                    var crossB = CreateCrossPolygonPoints2D(0.9f, 0.28f, size, -45f);
                    DrawFilledPolygon(texture, crossA, fill);
                    DrawFilledPolygon(texture, crossB, fill);
                    break;
                case MarkerShape.Diamond:
                    DrawFilledPolygon(texture, CreateRegularPolygonPoints2D(4, 0.46f, 0f, 0, size), fill);
                    break;

                default:
                    throw new System.NotImplementedException();
            }

            // Apply once after drawing to avoid expensive per-draw Apply calls
            texture.Apply();

            return texture;
        }

        public static Texture2D RotateTexture(Texture2D source, float angleDeg)
        {
            int size = source.width;
            var dst = new Texture2D(size, size, source.format, false);
            float angle = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(-angle); // inverse rotation
            float sin = Mathf.Sin(-angle);
            float cx = (size - 1) * 0.5f;
            float cy = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float sx = cos * dx - sin * dy + cx;
                    float sy = sin * dx + cos * dy + cy;
                    // Normalize to [0,1] for GetPixelBilinear
                    float u = Mathf.Clamp01(sx / (size - 1));
                    float v = Mathf.Clamp01(sy / (size - 1));
                    var col = source.GetPixelBilinear(u, v);
                    dst.SetPixel(x, y, col);
                }
            }
            dst.Apply();
            return dst;
        }




        #endregion
    }
}
