using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordsReplace {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e) {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var wordsReplace = WordsReplace.Instance();
            var replaceResult = wordsReplace.Repalce(this.textBox1.Text);
            //var result = wordsReplace.Repalce(new List<string> { this.textBox1.Text, this.textBox1.Text });
            this.textBox2.Text = replaceResult;
            sw.Stop();
            this.label1.Text = sw.ElapsedMilliseconds.ToString();
        }
    }


}
