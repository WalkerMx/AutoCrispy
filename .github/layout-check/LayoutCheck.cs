using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

// Layout regression check for the real AutoCrispy main form.
// Loads AutoCrispy.exe, opens Form1 with the Spandrel backend selected, then resizes the
// window and simulates DPI scaling. Reports overlapping sibling controls, controls that
// leave their container, and clipped text. Overlaps/clipping fail the build; text that is
// shortened with an ellipsis is reported as a warning.
internal static class LayoutCheck
{
    private const int OverlapTolerancePixels = 4;
    private static readonly List<string> Errors = new List<string>();
    private static readonly List<string> Warnings = new List<string>();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WmClose = 0x0010;

    // Application code may open modal message boxes (for example settings-load errors).
    // The checker runs unattended, so it records and closes them instead of hanging.
    private static void StartDialogDismisser()
    {
        Thread thread = new Thread(() =>
        {
            while (true)
            {
                IntPtr dialog = FindWindow("#32770", null);
                while (dialog != IntPtr.Zero)
                {
                    StringBuilder title = new StringBuilder(256);
                    GetWindowText(dialog, title, title.Capacity);
                    lock (Warnings)
                    {
                        Warnings.Add("unattended run closed a dialog: \"" + title + "\" (the app showed it during startup)");
                    }
                    PostMessage(dialog, WmClose, IntPtr.Zero, IntPtr.Zero);
                    Thread.Sleep(300);
                    dialog = FindWindow("#32770", null);
                }
                Thread.Sleep(200);
            }
        });
        thread.IsBackground = true;
        thread.Start();
    }

    // Fails fast with a readable annotation instead of hanging until the job times out.
    private static void StartWatchdog()
    {
        Thread thread = new Thread(() =>
        {
            Thread.Sleep(240000);
            Console.WriteLine("::error title=Layout::layout checker timed out after 4 minutes; the main form did not finish loading or resizing.");
            Environment.Exit(1);
        });
        thread.IsBackground = true;
        thread.Start();
    }

    private static readonly Size[] WindowSizes =
    {
        new Size(560, 400), new Size(700, 480), new Size(820, 560), new Size(900, 600), new Size(980, 640),
        new Size(1100, 700), new Size(1280, 800), new Size(1600, 900)
    };

    private static readonly float[] Scales = { 1.0f, 1.25f, 1.5f };

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            // Unhandled exceptions only reach the raw log; surface them as an annotation.
            Console.WriteLine("::error title=Layout::layout checker crashed: " + Escape(ex.ToString().Split('\n')[0]) +
                              " at " + Escape(ex.StackTrace == null ? "?" : ex.StackTrace.Split('\n')[0]));
            return 1;
        }
    }

    private static int Run(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("usage: LayoutCheck <AutoCrispy.exe>");
            return 2;
        }

        Application.EnableVisualStyles();
        StartDialogDismisser();
        StartWatchdog();
        Assembly app = Assembly.LoadFrom(Path.GetFullPath(args[0]));
        Type formType = app.GetType("AutoCrispy.Form1", true);

        foreach (float scale in Scales)
        {
            string scaleLabel = (scale * 100f).ToString("0") + "%";
            Form form = (Form)Activator.CreateInstance(formType);
            try
            {
                form.Show();
                Pump(3000);
                SelectSpandrel(form);
                Pump(500);
                if (scale != 1.0f)
                {
                    form.Scale(new SizeF(scale, scale));
                    // Mirror Form1_DpiChanged, which re-reads the responsive margins.
                    InvokePrivate(form, "ConfigureResponsiveLayout");
                    InvokePrivate(form, "ApplyResponsiveLayout");
                    Pump(300);
                }

                Note(form, scaleLabel);
                foreach (Size requested in WindowSizes)
                {
                    form.Size = requested;
                    Pump(250);
                    // The window may be clamped to its minimum; always check what is really shown.
                    Size size = form.Size;
                    Visit(form, scaleLabel + " window " + size.Width + "x" + size.Height);
                    SaveScreenshot(form, scaleLabel, size);
                }
            }
            catch (Exception ex)
            {
                Errors.Add("harness exception @" + scaleLabel + ": " + ex.GetBaseException().Message);
            }
            finally
            {
                form.Close();
                form.Dispose();
            }
        }

        foreach (string w in Warnings)
        {
            Console.WriteLine((w.StartsWith("note ") ? "::notice" : "::warning") + " title=Layout::" + Escape(w));
        }
        foreach (string e in Errors)
        {
            Console.WriteLine("::error title=Layout::" + Escape(e));
        }
        Console.WriteLine("LayoutCheck: " + Errors.Count + " error(s), " + Warnings.Count + " warning(s).");
        return Errors.Count == 0 ? 0 : 1;
    }

    private static string Escape(string s)
    {
        return s.Replace("%", "%25").Replace("\r", "%0D").Replace("\n", "%0A");
    }

    private static void Pump(int milliseconds)
    {
        Stopwatch watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < milliseconds)
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }
    }

    // Records what the run actually showed, so an empty result cannot pass silently.
    private static void Note(Form form, string scaleLabel)
    {
        ComboBox exe = FindControl(form, "ExeComboBox") as ComboBox;
        Control model = FindControl(form, "PyModel");
        Control catalog = FindControl(form, "BrowseOpenModelDbButton");
        Control group = FindControl(form, "PyGroup");
        int visibleInGroup = 0;
        if (group != null)
        {
            foreach (Control c in group.Controls) if (c.Visible) visibleInGroup++;
        }
        if (group != null)
        {
            StringBuilder dump = new StringBuilder();
            foreach (Control c in group.Controls)
            {
                if (!c.Visible) continue;
                dump.Append(c.Name + "@" + c.Top + "+" + c.Height + " ");
            }
            Warnings.Add("note layout dump @" + scaleLabel + " PyGroup " + group.ClientSize + ": " + dump);
        }
        Warnings.Add("note @" + scaleLabel + ": backend=" + (exe == null ? "?" : Convert.ToString(exe.SelectedItem)) +
                     " PyModel.Visible=" + (model != null && model.Visible) +
                     " Catalog.Visible=" + (catalog != null && catalog.Visible) +
                     " visible controls in PyGroup=" + visibleInGroup);
    }

    private static Control FindControl(Control root, string name)
    {
        if (root.Name == name) return root;
        foreach (Control c in root.Controls)
        {
            Control found = FindControl(c, name);
            if (found != null) return found;
        }
        return null;
    }

    // Writes what the form actually draws, so a reviewer can see the layout in the artifact.
    private static void SaveScreenshot(Form form, string scaleLabel, Size size)
    {
        try
        {
            Directory.CreateDirectory("layout-screens");
            using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
                string name = "layout-screens/" + scaleLabel.TrimEnd('%') + "pct-" + size.Width + "x" + size.Height + ".png";
                bitmap.Save(name, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        catch (Exception ex)
        {
            Warnings.Add("note screenshot failed: " + ex.Message);
        }
    }

    private static void SelectSpandrel(Form form)
    {
        PropertyInfo prop = form.GetType().GetProperty("ExeComboBox",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        ComboBox combo = prop == null ? null : prop.GetValue(form, null) as ComboBox;
        if (combo == null)
        {
            Errors.Add("ExeComboBox not found");
            return;
        }
        if (!combo.Items.Contains("Spandrel")) combo.Items.Add("Spandrel");
        combo.SelectedItem = "Spandrel";
    }

    private static void InvokePrivate(object target, string name)
    {
        MethodInfo method = target.GetType().GetMethod(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null && method.GetParameters().Length == 0) method.Invoke(target, null);
    }

    // Children of these controls are internal parts (spin buttons, edit boxes, dropdown
    // parts) or tab pages sharing one display area; they are not laid out by AutoCrispy.
    private static bool IsInternalContainer(Control c)
    {
        return c is NumericUpDown || c is ComboBox || c is TextBox || c is ListBox || c is TabControl;
    }

    private static void Visit(Control parent, string context)
    {
        if (IsInternalContainer(parent)) return;

        List<Control> visible = new List<Control>();
        foreach (Control child in parent.Controls)
        {
            if (!child.Visible || child.Width <= 0 || child.Height <= 0) continue;
            visible.Add(child);
        }

        Rectangle parentClient = parent is Form
            ? Rectangle.Empty
            : parent.RectangleToScreen(parent.ClientRectangle);

        for (int i = 0; i < visible.Count; i++)
        {
            Control a = visible[i];
            Rectangle ra = ScreenBounds(a);
            CheckText(a, context);

            if (!(parent is Form) && !Contains(parentClient, ra))
            {
                Errors.Add(Describe(a) + " leaves its container " + Describe(parent) + " (" + context + ")");
            }

            for (int j = i + 1; j < visible.Count; j++)
            {
                Rectangle rb = ScreenBounds(visible[j]);
                Rectangle inter = Rectangle.Intersect(ra, rb);
                if (inter.Width > OverlapTolerancePixels && inter.Height > OverlapTolerancePixels)
                {
                    Errors.Add(Describe(a) + " overlaps " + Describe(visible[j]) + " by " +
                               inter.Width + "x" + inter.Height + "px (" + context + ")");
                }
            }
        }

        foreach (Control child in visible)
        {
            Visit(child, context);
        }
    }

    private static Rectangle ScreenBounds(Control c)
    {
        Point origin = c.Parent.PointToScreen(c.Location);
        return new Rectangle(origin, c.Size);
    }

    private static bool Contains(Rectangle outer, Rectangle inner)
    {
        const int tol = 1;
        return inner.Left >= outer.Left - tol && inner.Top >= outer.Top - tol &&
               inner.Right <= outer.Right + tol && inner.Bottom <= outer.Bottom + tol;
    }

    private static void CheckText(Control c, string context)
    {
        if (c.Parent is Form || string.IsNullOrWhiteSpace(c.Text)) return;
        Label label = c as Label;
        Button button = c as Button;
        CheckBox check = c as CheckBox;
        if (label == null && button == null && check == null) return;
        if ((label != null && label.AutoSize) || (check != null && check.AutoSize) ||
            (button != null && button.AutoSize)) return;

        int glyph = check != null ? 22 : 0;
        int padding = (label != null ? label.Padding.Horizontal : 0) + 6;
        Size measured = TextRenderer.MeasureText(c.Text, c.Font,
            new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        int needed = measured.Width + glyph + padding;
        if (needed > c.ClientSize.Width + 2)
        {
            string text = Describe(c) + " text \"" + Trim(c.Text) + "\" needs " + needed +
                          "px, has " + c.ClientSize.Width + "px (" + context + ")";
            bool ellipsis = label != null && label.AutoEllipsis;
            if (ellipsis) Warnings.Add("ellipsis " + text);
            else Errors.Add("clipped " + text);
        }
    }

    private static string Trim(string s)
    {
        string one = s.Replace("\r", " ").Replace("\n", " ");
        return one.Length > 60 ? one.Substring(0, 60) + "..." : one;
    }

    private static string Describe(Control c)
    {
        string name = string.IsNullOrEmpty(c.Name) ? c.GetType().Name : c.Name;
        return name + "[" + c.Location.X + "," + c.Location.Y + " " + c.Width + "x" + c.Height + "]";
    }
}
