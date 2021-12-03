using System;
using System.Numerics;
using ImGuiNET;

namespace FFLogsLookup.Gui
{
    internal class Spinner
    {
        // https://github.com/ocornut/imgui/issues/1901#issuecomment-552185000

        private const int num_segments = 20;
        private const float start_angle = (float)(-Math.PI / 2.0f); // Start at the top
        private const int num_detents = 5; // how many rotations we want before a repeat
        private const int skip_detents = 3; // how many steps we skip each rotation
        private const float period = 5.0f; // in seconds

        public float Radius { get; set; } = 5;
        public float Thickness { get; set; } = 1;
        public Vector4 Color { get; set; } = new Vector4(250 / 255f, 250 / 255f, 250 / 255f, 1);

        public static float sawtooth(float t)
        {
            return (num_detents * t) % 1.0f;
        }

        private static float tween(float t)
        {
            // bezier
            return lerp(0.0f, 1.0f, t);
        }

        private static float interval(float T0, float T1, float t)
        {
            return t < T0 ? 0.0f : t > T1 ? 1.0f : tween((t - T0) / (T1 - T0));
        }

        public static float stroke_head_tween(float t)
        {
            t = sawtooth(t);
            return interval(0.0f, 0.5f, t);
        }

        private static float stroke_tail_tween(float t)
        {
            t = sawtooth(t);
            return interval(0.5f, 1.0f, t);
        }

        private static float lerp(float x0, float x1, float t)
        {
            return (1 - t) * x0 + t * x1;
        }
        private static float step_tween(float t)
        {
            return (float)Math.Floor(lerp(0.0f, (float)num_detents, t));
        }

        private static float rotation_tween(float t)
        {
            return sawtooth(t);
        }

        private readonly DateTime dateTime = DateTime.UtcNow;
        public void Draw()
        {
            var style = ImGui.GetStyle();

            var pos = ImGui.GetCursorScreenPos();
            var centre = new Vector2(pos.X + this.Radius, pos.Y + this.Radius + style.FramePadding.Y);

            var t = ((float)(DateTime.UtcNow - this.dateTime).TotalSeconds % period) / period; // map period into [0, 1]
            var head_value = stroke_head_tween(t);
            var tail_value = stroke_tail_tween(t);
            var step_value = step_tween(t);
            var rotation_value = rotation_tween(t); //         auto rotation_tween = sawtooth<num_detents>;

            var min_arc = 30.0f / 360.0f * 2.0f * Math.PI;
            var max_arc = 270.0f / 360.0f * 2.0f * Math.PI;
            var step_offset = skip_detents * 2.0f * Math.PI / num_detents;
            var rotation_compensation = (4.0 * Math.PI - step_offset - max_arc) % (2 * Math.PI);

            var a_min = start_angle + tail_value * max_arc + rotation_value * rotation_compensation - step_value * step_offset;
            var a_max = a_min + (head_value - tail_value) * max_arc + min_arc;

            var drawList = ImGui.GetWindowDrawList();
            drawList.PathClear();
            for (int i = 0; i < num_segments; i++)
            {
                var a = a_min + (float)i / num_segments * (a_max - a_min);

                var x = (float)(centre.X + Math.Cos(a) * this.Radius);
                var y = (float)(centre.Y + Math.Sin(a) * this.Radius);

                drawList.PathLineTo(new(x, y));
            }

            var c =
                ((uint)(this.Color.X * 255) <<  0) |
                ((uint)(this.Color.Y * 255) <<  8) |
                ((uint)(this.Color.Z * 255) << 16) |
                ((uint)(this.Color.W * 255) << 24);

            drawList.PathStroke(c, ImDrawFlags.None, this.Thickness);

            ImGui.Dummy(new(this.Radius * 2, this.Radius * 2));
        }
    }
}
