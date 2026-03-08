using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Tournament_Scoring_Prototype
{
    public class Participant
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public List<Guid> Events { get; } = new();
        public int Points { get; set; }
        public override string ToString() => $"{Name}  |  {Points} نقطة";
    }

    public class Team
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public List<string> Members { get; } = new();
        public List<Guid> Events { get; } = new();
        public int Points { get; set; }
        public override string ToString() => $"{Name}  |  {Points} نقطة";
    }

    public class EventItem
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public bool IsGroup { get; set; }
        public List<Guid> Registrations { get; } = new();
        public Dictionary<Guid, int> Rankings { get; } = new();
        public override string ToString() => Name + (IsGroup ? " " : " ");
    }

    public partial class MainForm : Form
    {
        private List<Participant> participants = new();
        private List<Team> teams = new();
        private List<EventItem> events = new();
        private Dictionary<int, int> scoring = new() { [1] = 10, [2] = 7, [3] = 5, [4] = 3, [5] = 1 };

        private ListBox lbParticipants, lbTeams, lbEvents, lbRegs;
        private Label lblTitle, lblStats;
        private Panel headerPanel;

        public MainForm()
        {
            this.Text = " نظام إدارة المسابقات والمعارض";
            this.Width = 1120;
            this.Height = 680;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = Color.FromArgb(240, 244, 250);
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeGUI();
            ShowWelcomeMessage();
        }

        private void InitializeGUI()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(52, 152, 219)
            };

            lblTitle = new Label
            {
                Text = " نظام إدارة المسابقات والمعارض",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(30, 18)
            };
            headerPanel.Controls.Add(lblTitle);

            lblStats = new Label
            {
                Text = "الأفراد: 0    |    الفرق: 0    |    الفعاليات: 0",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(236, 240, 241),
                AutoSize = true,
                Location = new Point(750, 27)
            };
            headerPanel.Controls.Add(lblStats);

            Controls.Add(headerPanel);

            int panelY = 90;
            int panelW = 250;
            int panelH = 280;

            CreatePanel("المشاركين الأفراد ", 20, panelY, panelW, panelH, () => lbParticipants, out lbParticipants);
            CreatePanel("الفرق ", 290, panelY, panelW, panelH, () => lbTeams, out lbTeams);
            CreatePanel("الفعاليات ", 560, panelY, panelW, panelH, () => lbEvents, out lbEvents);
            CreatePanel("التسجيلات والمشاركين ", 830, panelY, panelW, panelH, () => lbRegs, out lbRegs);

            int btnY = 400;
            int btnW = 130;
            int btnH = 45;
            int gap = 15;

            var buttons = new (string text, int x, Action click, Color color)[]
            {
                (" إضافة فرد", 20, AddParticipant, Color.FromArgb(46, 204, 113)),
                (" إضافة فريق", 165, AddTeam, Color.FromArgb(155, 89, 182)),
                (" إضافة فعالية", 310, AddEvent, Color.FromArgb(52, 152, 219)),
                (" تسجيل مشاركة", 455, Register, Color.FromArgb(241, 196, 15)),
                (" إدخال ترتيب", 600, EnterRanking, Color.FromArgb(230, 126, 34)),
                (" تعديل النقاط", 745, EditScoring, Color.FromArgb(149, 165, 166)),
                (" الترتيب النهائي", 890, ShowFinalRanking, Color.FromArgb(231, 76, 60))
            };

            foreach (var btn in buttons)
            {
                CreateStyledButton(btn.text, btn.x, btnY, btnW, btnH, btn.click, btn.color);
            }

            var notePanel = new Panel
            {
                Location = new Point(20, 460),
                Size = new Size(1000, 40),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            var noteLabel = new Label
            {
                Text = "📌 ملاحظات: الحد الأقصى 20 فرد | 4 فرق (5 أعضاء لكل فريق) | 5 فعاليات | 5 مشاركات لكل مشارك",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            notePanel.Controls.Add(noteLabel);
            Controls.Add(notePanel);

            lbEvents.SelectedIndexChanged += (s, e) => RefreshRegs();
        }

        private void CreatePanel(string title, int x, int y, int w, int h, Func<ListBox> getListBox, out ListBox lb)
        {
            ListBox listBox = null;
            
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };

            var label = new Label
            {
                Text = title,
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };
            panel.Controls.Add(label);

            listBox = new ListBox
            {
                Location = new Point(5, 35),
                Size = new Size(w - 30, h - 70),
                BackColor = Color.FromArgb(249, 249, 249),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                RightToLeft = RightToLeft.Yes,
                IntegralHeight = false
            };
            panel.Controls.Add(listBox);

            listBox.DrawMode = DrawMode.OwnerDrawFixed;
            var listBoxRef = listBox;
            listBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                e.DrawBackground();
                using var brush = new SolidBrush(e.ForeColor);
                e.Graphics.DrawString(listBoxRef.Items[e.Index].ToString(), e.Font, brush, e.Bounds);
            };

            Controls.Add(panel);
            lb = listBox;
        }

        private void CreateStyledButton(string text, int x, int y, int w, int h, Action click, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(backColor, 0.2f);
            btn.Click += (s, e) => click();
            Controls.Add(btn);
        }

        private void ShowWelcomeMessage()
        {
            MessageBox.Show(
                " أهلاً بك في نظام إدارة المسابقات والمعارض!\n\n" +
                " البداية: أضف المشاركين الفرادي، ثم الفرق، ثم الفعاليات.\n" +
                " الخطوة التالية: سجّل المشاركين في الفعاليات.\n" +
                " في النهاية: أدخل ترتيب الفائزين في كل فعالية.\n\n" +
                " نصيحة: راجع نظام النقاط قبل البدء!",
                "مرحباً بك",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void AddParticipant()
        {
            if (participants.Count >= 20)
            {
                MessageBox.Show(" تم الوصول للحد الأقصى (20 شخص).", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var name = Prompt.InputBox(" إضافة مشارك جديد", "أدخل اسم المشارك:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                participants.Add(new Participant { Name = name.Trim() });
                RefreshAll();
                MessageBox.Show($" تم إضافة: {name.Trim()}", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddTeam()
        {
            if (teams.Count >= 4)
            {
                MessageBox.Show(" تم الوصول للحد الأقصى (4 فرق).", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var name = Prompt.InputBox(" إضافة فريق جديد", "أدخل اسم الفريق:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var members = new List<string>();
            for (int i = 1; i <= 5; i++)
            {
                var m = Prompt.InputBox($" اسم العضو {i}/5 في فريق {name}:", $"العضو {i}:");
                if (string.IsNullOrWhiteSpace(m))
                {
                    MessageBox.Show(" يجب إدخال أسماء الـ 5 أعضاء.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                members.Add(m.Trim());
            }

            var team = new Team { Name = name.Trim() };
            team.Members.AddRange(members);
            teams.Add(team);
            RefreshAll();

            MessageBox.Show($" تم إضافة فريق: {name.Trim()}\n الأعضاء: {string.Join(", ", members)}", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddEvent()
        {
            if (events.Count >= 5)
            {
                MessageBox.Show(" تم الوصول للحد الأقصى (5 فعاليات).", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var name = Prompt.InputBox(" إضافة فعالية جديدة", "أدخل اسم الفعالية:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var isGroup = MessageBox.Show(" هل هذه الفعالية جماعية (للفرق)؟\n\nنعم = جماعية\nلا = فردية", "نوع الفعالية", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

            events.Add(new EventItem { Name = name.Trim(), IsGroup = isGroup });
            RefreshAll();
            MessageBox.Show($" تم إضافة: {name.Trim()}\n النوع: {(isGroup ? "جماعية" : "فردية")}", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Register()
        {
            if (lbEvents.SelectedItem is not EventItem evt)
            {
                MessageBox.Show(" الرجاء اختيار فعالية أولاً!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (evt.IsGroup)
            {
                if (teams.Count == 0)
                {
                    MessageBox.Show(" لا توجد فرق مسجلة!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var pick = PickBox.Pick(" اختر فريقاً للتسجيل:", teams.Cast<object>().ToList());
                if (pick is Team t)
                {
                    if (t.Events.Count >= 5)
                    {
                        MessageBox.Show(" الفريق سجل في 5 فعاليات بالفعل!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!t.Events.Contains(evt.Id))
                    {
                        t.Events.Add(evt.Id);
                        evt.Registrations.Add(t.Id);
                        RefreshRegs();
                        MessageBox.Show($" تم تسجيل فريق {t.Name} في {evt.Name}", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(" الفريق مسجل مسبقاً!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            else
            {
                if (participants.Count == 0)
                {
                    MessageBox.Show(" لا توجد مشاركين مسجلين!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var pick = PickBox.Pick(" اختر مشاركاً للتسجيل:", participants.Cast<object>().ToList());
                if (pick is Participant p)
                {
                    if (p.Events.Count >= 5)
                    {
                        MessageBox.Show(" Participant registered in 5 events already!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!p.Events.Contains(evt.Id))
                    {
                        p.Events.Add(evt.Id);
                        evt.Registrations.Add(p.Id);
                        RefreshRegs();
                        MessageBox.Show($" تم تسجيل {p.Name} في {evt.Name}", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(" Participant already registered!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void EnterRanking()
        {
            if (lbEvents.SelectedItem is not EventItem evt) return;
            if (evt.Registrations.Count == 0)
            {
                MessageBox.Show(" لا يوجد مسجلون في هذه الفعالية!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (evt.Rankings.Count > 0)
            {
                var confirm = MessageBox.Show(" تم إدخال ترتيب سابق! هل تريد إعادة إدخاله؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
                evt.Rankings.Clear();
            }

            int max = Math.Min(5, evt.Registrations.Count);
            var s = Prompt.InputBox(" إدخال الترتيب", $"كم مركز تريد إدخاله (1-{max})؟", max.ToString());
            if (!int.TryParse(s, out int count)) return;
            count = Math.Min(count, max);

            var used = new HashSet<Guid>();
            var results = new List<string>();

            for (int r = 1; r <= count; r++)
            {
                var candidates = evt.Registrations.Where(id => !used.Contains(id))
                    .Select(id => (participants.FirstOrDefault(p => p.Id == id) as object) ?? teams.First(t => t.Id == id)).ToList();

                var win = PickBox.Pick($" المركز #{r} - اختر الفائز:", candidates);
                if (win == null) break;

                Guid wid = (win is Participant p) ? p.Id : ((Team)win).Id;
                used.Add(wid);
                evt.Rankings[wid] = r;

                int pts = scoring.ContainsKey(r) ? scoring[r] : 0;
                var part = participants.FirstOrDefault(x => x.Id == wid);
                if (part != null)
                {
                    part.Points += pts;
                    results.Add($"#{r}: {part.Name} → {pts} نقطة");
                }
                else
                {
                    teams.First(x => x.Id == wid).Points += pts;
                    results.Add($"#{r}: {teams.First(x => x.Id == wid).Name} → {pts} نقطة");
                }
            }

            RefreshAll();
            MessageBox.Show($" تم إدخال ترتيب {evt.Name}:\n\n{string.Join("\n", results)}", "تم", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditScoring()
        {
            var msg = "⚙️ تعديل نظام النقاط\n\nالنظام الحالي:\n";
            for (int i = 1; i <= 5; i++)
            {
                msg += $"المركز {i}: {scoring[i]} نقاط\n";
            }
            msg += "\nأدخل النقاط الجديدة:";
            MessageBox.Show(msg, "تعديل النقاط", MessageBoxButtons.OK, MessageBoxIcon.Information);

            for (int i = 1; i <= 5; i++)
            {
                var s = Prompt.InputBox($"🎯 نقاط المركز {i}:", scoring[i].ToString());
                if (int.TryParse(s, out int val)) scoring[i] = val;
            }
            MessageBox.Show(" تم حفظ نظام النقاط الجديد!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowFinalRanking()
        {
            var sortedP = participants.OrderByDescending(p => p.Points).ToList();
            var sortedT = teams.OrderByDescending(t => t.Points).ToList();

            var msg = " === الترتيب النهائي === \n\n";

            msg += " الأفراد:\n";
            if (sortedP.Count == 0) msg += "   لا يوجد مشاركين\n";
            else
            {
                int rank = 1;
                foreach (var p in sortedP)
                {
                    var medal = rank == 1 ? "" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : "  ";
                    msg += $"   {medal} {rank}. {p.Name} ← {p.Points} نقطة\n";
                    rank++;
                }
            }

            msg += "\n الفرق:\n";
            if (sortedT.Count == 0) msg += "   لا يوجد فرق\n";
            else
            {
                int rank = 1;
                foreach (var t in sortedT)
                {
                    var medal = rank == 1 ? "" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : "  ";
                    msg += $"   {medal} {rank}. {t.Name} ← {t.Points} نقطة\n";
                    rank++;
                }
            }

            MessageBox.Show(msg, " الترتيب النهائي", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshAll()
        {
            lbParticipants.DataSource = null;
            lbParticipants.DataSource = participants;

            lbTeams.DataSource = null;
            lbTeams.DataSource = teams;

            lbEvents.DataSource = null;
            lbEvents.DataSource = events;

            lblStats.Text = $"الأفراد: {participants.Count}/20    |    الفرق: {teams.Count}/4    |    الفعاليات: {events.Count}/5";

            RefreshRegs();
        }

        private void RefreshRegs()
        {
            lbRegs.Items.Clear();
            if (lbEvents.SelectedItem is not EventItem e) return;

            foreach (var id in e.Registrations)
            {
                var p = participants.FirstOrDefault(x => x.Id == id);
                if (p != null)
                {
                    var rank = e.Rankings.ContainsKey(id) ? $" (#{e.Rankings[id]})" : "";
                    lbRegs.Items.Add($" {p.Name}{rank}");
                }
                else
                {
                    var t = teams.First(x => x.Id == id);
                    var rank = e.Rankings.ContainsKey(id) ? $" (#{e.Rankings[id]})" : "";
                    lbRegs.Items.Add($" {t.Name}{rank}");
                }
            }
        }
    }

    public static class Prompt
    {
        public static string InputBox(string title, string label, string defaultValue = "")
        {
            Form f = new Form
            {
                Width = 400,
                Height = 160,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ControlBox = false,
                BackColor = Color.FromArgb(240, 244, 250)
            };

            var lbl = new Label
            {
                Text = label,
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11)
            };

            var tb = new TextBox
            {
                Location = new Point(20, 50),
                Width = 340,
                Font = new Font("Segoe UI", 11),
                Text = defaultValue
            };

            var btnOk = new Button
            {
                Text = " موافق",
                Location = new Point(200, 85),
                Width = 80,
                Height = 35,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var btnCancel = new Button
            {
                Text = " إلغاء",
                Location = new Point(290, 85),
                Width = 80,
                Height = 35,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOk.Click += (s, e) => { f.DialogResult = DialogResult.OK; f.Close(); };
            btnCancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };

            f.Controls.AddRange(new Control[] { lbl, tb, btnOk, btnCancel });
            f.AcceptButton = btnOk;

            return f.ShowDialog() == DialogResult.OK ? tb.Text : "";
        }
    }

    public static class PickBox
    {
        public static object Pick(string title, List<object> items)
        {
            if (items.Count == 0) return null;

            Form f = new Form
            {
                Width = 400,
                Height = 350,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                BackColor = Color.FromArgb(240, 244, 250)
            };

            var lb = new ListBox
            {
                Left = 20,
                Top = 20,
                Width = 340,
                Height = 220,
                DataSource = items,
                RightToLeft = RightToLeft.Yes,
                Font = new Font("Segoe UI", 11)
            };

            var btnOk = new Button
            {
                Text = " اختيار",
                Location = new Point(190, 260),
                Width = 80,
                Height = 35,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var btnCancel = new Button
            {
                Text = " إلغاء",
                Location = new Point(280, 260),
                Width = 80,
                Height = 35,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOk.Click += (s, e) => { f.DialogResult = DialogResult.OK; f.Close(); };
            btnCancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };

            f.Controls.AddRange(new Control[] { lb, btnOk, btnCancel });
            f.AcceptButton = btnOk;

            return f.ShowDialog() == DialogResult.OK ? lb.SelectedItem : null;
        }
    }
}
