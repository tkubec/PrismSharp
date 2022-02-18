using Orionsoft.PrismSharp.Highlighters.Rtf;
using Orionsoft.PrismSharp.Themes;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinForms
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(int hWnd, int nBar);

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, System.Windows.Forms.Orientation nBar, int nPos, bool bRedraw);

        private const uint WM_VSCROLL = 0x0115;
        private const uint WM_HSCROLL = 0x0114;
        private const int SB_THUMBPOSITION = 4;
        private const int WM_USER = 0x0400;
        private const int EM_SETEVENTMASK = (WM_USER + 69);
        private const int WM_SETREDRAW = 0x0b;
        private int OldEventMask;

        private readonly RtfHighlighter highlighter;

        public void BeginUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, 0, 0);
            OldEventMask = SendMessage(this.Handle, EM_SETEVENTMASK, 0, 0);
        }

        public void EndUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, 1, 0);
            SendMessage(this.Handle, EM_SETEVENTMASK, 0, OldEventMask);
            editor.Invalidate();
        }

        public Form1()
        {
            InitializeComponent();

            highlighter = new RtfHighlighter(new Orionsoft.PrismSharp.Tokenizing.Tokenizer(), Theme.Load(ThemeNames.Vs))
            {
                Font = "Consolas"
            };
        }

        private void editor_TextChanged(object sender, EventArgs e)
        {
            var vertPos = GetScrollPos((int)editor.Handle, (int)Orientation.Vertical);
            var horizPos = GetScrollPos((int)editor.Handle, (int)Orientation.Horizontal);
            BeginUpdate();

            int i = editor.SelectionStart;
            var hl = highlighter.Highlight(editor.Text + "\n", "csharp");

            var stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(hl));
            this.editor.LoadFile(stream, RichTextBoxStreamType.RichText);

            SendMessage(editor.Handle, WM_VSCROLL, vertPos << 16 | SB_THUMBPOSITION, 0);
            SendMessage(editor.Handle, WM_HSCROLL, horizPos << 16 | SB_THUMBPOSITION, 0);
            SetScrollPos(editor.Handle, Orientation.Vertical, vertPos, true);
            SetScrollPos(editor.Handle, Orientation.Horizontal, horizPos, true);
            editor.SelectionStart = i;

            EndUpdate();
        }
    }
}


