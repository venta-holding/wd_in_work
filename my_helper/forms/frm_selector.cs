using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Collections;
//using customer_info;
using josi;
//using josi.store.res;
using kibicom.tlib;
using kibicom.josi;


namespace kibicom.my_wd_helper
{

	
	public partial class frm_selector : Form
	{

		//DropShadow ds = new DropShadow();

		public bool is_shown = false;

		//������ �������� � josi
		//t_josi_store josi_store = new t_josi_store(new t_josi_auth_args("dnclive", "135"));
		public t_store josi_store =null;// new t_store(new t(){{"login_name","dnclive"}, {"pass","135"}});

		public t_kwj kwj;

		//����� ���������� ������� �� �������
		public TimeSpan last_key_down;

		//��������� �������� ������� txt_query
		public string last_filter_val = "";

		//���� - ����� �� ���������� ������������ ���������� �������
		//public bool show_result=true;

		//���������, ��� ��������� �������
		//public t selected_item;

		//public t tab=new t();

		//���������� ��������� ���� �� listbox
		Point last_lbx_mouse_point;

		//���������
		public t args=new t();

		//����� ����� ������/�������������� ��������
		public Form frm_cre_edit_item;

		public frm_selector()
		{
			InitializeComponent();
		}

		public frm_selector(t args)
			: this()
		{
			Owner = args["owner"].f_val<Form>();

			this.args["caption"] = args["caption"];
			lbl_invite.Text = this.args["caption"].f_def(lbl_invite.Text).f_str();

			string login_name = args["josi_store"]["login_name"].f_def("dnclive").f_str();
			string pass = args["josi_store"]["pass"].f_def("135").f_str();
			string josi_end_point = args["josi_store"]["josi_end_point"].
				f_def("https://192.168.1.139/webproj/git/kibicom_venta/index.php").f_str();
				//f_def("https://192.168.1.37/webproj/git/kibicom_venta/index.php").f_str();

			josi_store = new t_store(new t()
			{
				{"josi_end_point", josi_end_point},		//����� ����������� josi
				{"req_timeout", args["josi_store"]["req_timeout"].f_def(5000).f_int()},	//������� �������
				{"login_name",login_name},				//��� ��� �����
				{"pass",pass},							//������ ��� �����
				{"login_on_cre", true},					//���������
				{"auth_try_count", args["josi_store"]["auth_try_count"].f_def(3).f_int()},	//���������� ������� �����������
				{"f_done", args["f_done"].f_f()},		//�������� ����� ������������ �������
				{"f_fail", args["f_fail"].f_f()}		//�������� ���� ����������� �� �������
			});

			this.args["using_local_store"] = args["local_store"]["store_type"].f_def("mssql");

			//�������������� ��������� ���������
			f_cre_kwj(args);

			f_set_grafix(args);
		}


		public t f_set_grafix(t args)
		{

			//this.Scale((float)0.8);
			//lbx_items.Scale((float)0.8);
			lbx_items.ItemHeight = args["lbx"]["item_hight"].f_def(63).f_int();
			lbx_items.Font=new Font(lbx_items.Font.FontFamily, args["lbx"]["font_size"].f_def(16).f_int());

			lbx_items.ItemHeight = args["lbx"]["item_hight"].f_def(55).f_int();
			lbx_items.Font = new Font(lbx_items.Font.FontFamily, args["lbx"]["font_size"].f_def(14).f_int());

			//lbx_items.ItemHeight = args["lbx"]["item_hight"].f_def(43).f_int();
			//lbx_items.Font = new Font(lbx_items.Font.FontFamily, args["lbx"]["font_size"].f_def(11).f_int());

			return new t();
		}

		public t f_cre_kwj(t args)
		{
			//������� �������, ������������
			kwj = new t_kwj(new t()
			{
				{"josi_store", args["josi_store"]},
				{
					"local_store", args["local_store"]
				}
			});

			return new t();
		}

		public t f_cre_josi_store(t args)
		{
			string login_name = args["login_name"].f_def("dnclive").f_str();
			string pass = args["pass"].f_def("135").f_str();
			string josi_end_point = args["josi_end_point"].
				//f_def("https://192.168.1.139/webproj/git/kibicom_venta/index.php").f_str();
				f_def("https://192.168.1.37/webproj/git/kibicom_venta/index.php").f_str();

			josi_store = new t_store(new t()
			{
				{"josi_end_point", josi_end_point},		//����� ����������� josi
				{"req_timeout", args["req_timeout"].f_def(5000).f_int()},	//������� �������
				{"login_name",login_name},				//��� ��� �����
				{"pass",pass},							//������ ��� �����
				{"login_on_cre", true},					//���������
				{"auth_try_count", args["auth_try_count"].f_def(3).f_int()},	//���������� ������� �����������
				{"f_done", args["f_done"].f_f()},		//�������� ����� ������������ �������
				{"f_fail", args["f_fail"].f_f()}		//�������� ���� ����������� �� �������
			});

			//this["josi_store"] = josi_store;

			return new t();
		}

		#region �������

		//������� �� ������ � ������ ������
		private void textBox1_KeyDown(object sender, KeyEventArgs e)
		{

			if (e.KeyData == Keys.Down || e.KeyData == Keys.Up)
			{
				e.Handled = true;
				if (lbx_items.Items.Count == 0)
				{
					return;
				}
				if (e.KeyData == Keys.Down)
				{
					if (lbx_items.SelectedIndex == lbx_items.Items.Count - 1)
					{
						lbx_items.SelectedIndex = 0;
						e.Handled = false;
					}
					else
					{
						lbx_items.SelectedIndex++;
					}
					e.Handled = true;
				}
				if (e.KeyData == Keys.Up)
				{
					if (lbx_items.SelectedIndex == 0)
					{
						lbx_items.SelectedIndex = lbx_items.Items.Count - 1;
					}
					else
					{
						lbx_items.SelectedIndex--;
					}
					e.Handled = true;
				}
			}

			if (e.KeyData == Keys.Enter)
			{
				
				//f_select_item();
				f_touch_lbx_item();

			}
			
		}
		
		//���������� ������ � ������ ������
		private void txt_query_KeyUp(object sender, KeyEventArgs e)
		{

			if (e.KeyData == Keys.Up || e.KeyData == Keys.Down || e.KeyData==Keys.Enter)
			{
				return;
			}

			if (txt_query.Text.Length < 2&&1==0)
			{

				lbx_items.Items.Clear();
				fp_actions.Visible = false;
				//show_result = false;
				return;
			}

			if (last_filter_val == txt_query.Text)
			{
				//show_result = false;
				return;
			}

			/*
			this.args["eimit"].f_set(false);
			if (!f_check_speed_dial(new t())["emit"].f_bool())
			{
				return;
			}
			*/

			last_filter_val = txt_query.Text;

			f_find(new t());
			
			

		}

		//����� ����������
		private void josi_customer_Shown(object sender, EventArgs e)
		{
			txt_query.Focus();
			//ds.f_show(this);
		}

		//����� ��������� � listbox
		private void lbx_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			//MessageBox.Show("lbx");
			ListBox lbx = (ListBox)sender;
			//���� � ������ ��� ��������� ������ ������������� �������
			if (lbx.Items.Count == 0 || e.Index < 0)
			{
				return;
			}

			//�������� ������ �������� ������� � t_customer
			t item = (t)lbx.Items[e.Index];

			// Draw the background of the ListBox control for each item.
			e.DrawBackground();
			// Define the default color of the brush as black.
			//Brush text_brush = new System.Drawing.SolidBrush(ColorTranslator.FromHtml("#333"));

			//Brush b1 = new System.Drawing.SolidBrush(ColorTranslator.FromHtml("#0099cc"));

			//Font text_font = e.Font;

			//������������ ������� ���������� �������
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				//������ ������������ ����������� �������� (���)
				e.Graphics.FillRectangle(new System.Drawing.SolidBrush(ColorTranslator.FromHtml("#0099cc")), e.Bounds);
				//text_brush = SystemBrushes.HighlightText;
				//text_brush = new System.Drawing.SolidBrush(ColorTranslator.FromHtml("#9cc"));
				//text_font = new Font(e.Font, FontStyle.Bold);

				//������� fio ������� � ������ �������
				t_uti.f_draw_text
				(
					e.Graphics,
					item["str1"].f_str(),
					new Font(e.Font, FontStyle.Bold),	//����� ������ ������
					new SolidBrush(ColorTranslator.FromHtml("#fff")),	//���� ������
					new SolidBrush(ColorTranslator.FromHtml("#333")),	//���� ����
					//������������� � ������� ��������� �����
					new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height)
				);

				//������ �������� � email �� ������ ������
				t_uti.f_draw_text
				(
					e.Graphics,
					item["str2"].f_str(),
					e.Font,			//����� ��������� ����������
					new SolidBrush(ColorTranslator.FromHtml("#eee")),	//���� ������
					new SolidBrush(ColorTranslator.FromHtml("#555")),	//���� ����
					//������������� � ������� ��������� ����� - ������ �� 31 ������� �� ��������� (����� ������ ������)
					new Rectangle(e.Bounds.X, e.Bounds.Y + lbx_items.ItemHeight/2-2, e.Bounds.Width, e.Bounds.Height)
				);

				//������� ������ �������������� �� ���������� ��������
				RectangleF rect = e.Bounds;
				fp_actions.Top = Convert.ToInt32(lbx_items.Top + rect.Top + rect.Height / 2 - fp_actions.Height / 2);
				fp_actions.Left = Convert.ToInt32(lbx_items.Left + rect.Right - rect.Height / 2 + fp_actions.Height / 2 - fp_actions.Width);

			}
			else	//������������ �� ���������� ��������
			{
				//�������� ������ ����� ����� ���
				if (e.Index % 2 == 0)
				{
					e.Graphics.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#fff")), e.Bounds);
				}
				else//������ ������ �����
				{
					e.Graphics.FillRectangle(new SolidBrush(ColorTranslator.FromHtml("#eee")), e.Bounds);
				}

				//������� fio ������� � ������ �������
				t_uti.f_draw_text
				(
					e.Graphics,
					item["str1"].f_str(),
					e.Font,
					new SolidBrush(ColorTranslator.FromHtml("#333")),	//���� ������
					new SolidBrush(ColorTranslator.FromHtml("#fff")),	//���� ����
					e.Bounds
				);
				//������ �������� � email �� ������ ������
				t_uti.f_draw_text
				(
					e.Graphics,
					item["str2"].f_str(),
					e.Font,
					new SolidBrush(ColorTranslator.FromHtml("#555")),	//���� ������
					new SolidBrush(ColorTranslator.FromHtml("#fff")),	//���� ����
					new Rectangle(e.Bounds.X, e.Bounds.Y + lbx_items.ItemHeight / 2-2, e.Bounds.Width, e.Bounds.Height)
				);

			}

			// If the ListBox has focus, draw a focus rectangle around the selected item.
			e.DrawFocusRectangle();
			//e.
		}

		private void lbx_items_Enter(object sender, EventArgs e)
		{
			//txt_query.Focus();
			if (lbx_items.Items.Count > 0)
			{
				fp_actions.Visible = true;
			}
		}

		private void lbx_items_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			//MessageBox.Show("kd");
			txt_query.Focus();
			//txt_query.fi
			//RaiseKeyEvent(e.KeyCode, new KeyEventArgs(e.KeyData));
			//RaiseKeyEvent(lbx_items, new KeyEventArgs(e.KeyData));
			//textBox1_KeyDown(txt_query, new KeyEventArgs(e.KeyData));
			//txt_query_KeyUp(txt_query, new KeyEventArgs(e.KeyData));
			//te

			//var key = Keys.Insert;                    // Key to send
			//var target = Keyboard.FocusedElement;    // Target element
			//var routedEvent = Keyboard.KeyDownEvent; // Event to send




			//target.RaiseEvent(new KeyEventArgs
			//(
			//	Keyboard.PrimaryDevice,
			//	PresentationSource.FromVisual(target),
			//	0,
			//	key)//,
			//{RoutedEvent=routedEvent }
			//);

		}

		private void lbx_items_MouseMove(object sender, MouseEventArgs e)
		{

			ListBox lbx = (ListBox)sender;


			if (last_lbx_mouse_point != null)
			{
				if (last_lbx_mouse_point.X == e.X && last_lbx_mouse_point.Y == e.Y)
				{
					return;
				}
			}

			//MessageBox.Show("move");

			lbx.Focus();

			last_lbx_mouse_point = new Point(e.X, e.Y);

			int itemIndex = lbx.IndexFromPoint(last_lbx_mouse_point);

			if (itemIndex >= 0 && itemIndex != 65535)
			{
				lbx.SelectedIndex = itemIndex;
				f_mouse_change_selected_item(new t()
				{
					{"item", (t)(lbx.Items[itemIndex])}
				});
				fp_actions.Visible = true;
			}

		}

		private void josi_customer_Deactivate(object sender, EventArgs e)
		{
			is_shown = false;
			if (!this.args["is_blocked"].f_def(false).f_bool())
			{
				//ds.f_hide();
				f_leaved(this.args);
				Hide();
			}
			if (frm_cre_edit_item != null)
			{
				frm_cre_edit_item.Activate();
			}
			else if (Owner != null)
			{
				Owner.Activate();
			}

		}

		private void josi_customer_FormClosing(object sender, FormClosingEventArgs e)
		{
			is_shown = false;
			//ds.f_hide();
			Hide();
			//e.Cancel = true;
		}

		private void lbx_items_Click(object sender, EventArgs e)
		{
			f_touch_lbx_item();
		}

		#region ������

		private void btn_change_Click(object sender, EventArgs e)
		{
			//�������� ���������� �������
			//t selected_item = this.args["selected_item"].f_set((t)lbx_items.SelectedItem);

			//�������� ������� ��������������
			f_modify_item(new t() { { "item", (t)lbx_items.SelectedItem } });
		}

		private void btn_change_MouseEnter(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			f_btn_push(btn);
		}

		private void btn_change_MouseLeave(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			f_btn_up(btn);
		}

		//pick
		private void btn_pick_Click(object sender, EventArgs e)
		{
			//�������� ������� ��������������
			f_pick_item(new t() 
			{ 
				{ "item", (t)lbx_items.SelectedItem },
				{"btn", sender}
			});
		}

		//������ ������ �������
		public void f_btn_push(Button btn)
		{
			//btn.Image = ((System.Drawing.Image)(Properties.Resources.btn_print_1));
			btn.FlatAppearance.BorderSize = 3;
		}

		//������ ����������
		public void f_btn_up(Button btn)
		{
			//btn.Image = ((System.Drawing.Image)(Properties.Resources.btn_print_1));
			btn.FlatAppearance.BorderSize = 0;
		}

		//drop
		private void btn_drop_Click(object sender, EventArgs e)
		{
			//�������� ������� ��������������
			f_drop_item(new t() 
			{ 
				{"item", (t)lbx_items.SelectedItem},
				{"btn", sender}
			});
		}

		//��������������
		private void btn_revert_Click(object sender, EventArgs e)
		{
			//�������� ������� ��������������
			f_revert_item(new t() 
			{ 
				{"item", (t)lbx_items.SelectedItem},
				{"btn", sender}
			});
		}

		//�����
		private void btn_opt_Click(object sender, EventArgs e)
		{
			//�������� ������� ��������������
			f_opt(new t());
		}


		#endregion ������


		private void frm_finder_Load(object sender, EventArgs e)
		{

		}

		private void lbx_items_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		

		//border only
		protected override void WndProc(ref Message message)
		{
			const int WM_NCHITTEST = 0x0084;

			if (message.Msg == WM_NCHITTEST)
				return;

			base.WndProc(ref message);
		}

		#endregion �������

		public t f_check_speed_dial(t args)
		{

			TimeSpan last_key_down= this.args["last_key_down"].
					f_def_set(new TimeSpan(DateTime.Now.Ticks)).f_val<TimeSpan>();

			if (DateTime.Now.Ticks - last_key_down.Ticks > 10000000)
			{
				this.args["emit"].f_set(true);
			}

			this.args["last_key_down"].f_set(new TimeSpan(DateTime.Now.Ticks));

			return this.args;
		}

		//������ ��������� �� ������ ��������
		virtual public t f_mouse_change_selected_item(t args)
		{
			return new t();
		}

		//����� ��������������
		virtual public t f_leaved(t args)
		{

			t.f_f("f_leaved", this.args);

			return new t();
		}

		//��������� �����
		public void f_find(t args)
		{
			fp_actions.Visible = false;

			this.args["items"].Clear();

			//���� ���� ����� �������� � ���� ���������� �� �����
			if (this.args["new_items"].Count > 0)
			{
				foreach (t new_item in (IList<t>) this.args["new_items"])
				{
					this.args["items"].Add(new_item);
				}

				this.args["new_items"].Clear();

				f_fill_lbx(args);

				return;
			}

			pb_loading_2.Show();

			f_get_items(args);
		}

		//��������� ��������� �� ���������
		virtual public t f_get_items(t args)
		{

			return new t();
		}

		//���������� listbox ����������� ����������
		virtual public t f_fill_lbx(t args)
		{

			//t_f<t,t> f_done=

			t_f<t,t> f=new t_f<t, t>(delegate(t args1)
			{
				//string lbx_add = args1["lbx_add"].f_def("replace").f_str();

				

				bool lbx_add_replace = args1["lbx_add"].f_def("replace").f_str() == "replace";

				//MessageBox.Show(lbx_add_replace.ToString());

				pb_loading_2.Hide();

				//���� ���������� ������������ ����������� 0 � ������ �� ����
				//�� ��������� ����� �������
				if (this.args["items"].Count == 0&& txt_query.Text!="")
				{
					f_add_new();
				}

				//������� ���� �����
				if (lbx_add_replace) lbx_items.Items.Clear();

				//��������� � �������� ������
				foreach (t item in (IList<t>)this.args["items"])
				{
					lbx_items.Items.Add(item);
				}

				lbx_items.SelectedIndex = 0;

				t.f_f("f_done", args1);

				return new t();

			});

			if (lbx_items.InvokeRequired)
			{
				lbx_items.Invoke(f, new object[] { args });
			}
			else
			{
				f(args);
			}

			return new t();
		}

		//���� ������� ������� ����, ���������� ������� ����� ������� �������
		virtual public void f_add_new()
		{
			this.args["items"].Add(new t()
			{
				{"str1",this.args["new_item_caption"].f_def("�������� ����� �������")},
				{"str2",txt_query.Text},
				{"is_new", true}
			});
		}

		//����� �������� listbox
		virtual public void f_touch_lbx_item()
		{
			//�������� ���������� �������
			t selected_item=this.args["selected_item"].f_set((t)lbx_items.SelectedItem);

			//���� �� �����
			if (selected_item["is_new"].f_bool())
			{
				//�������� ��� ��������
				f_cre_item(new t());
			}
			else
			{
				//����� ������ ��������
				f_select_item();
			}
		}

		//����� ����������� ��������
		virtual public t f_select_item()
		{
			return new t();
		}

		//�������� ������ ��������
		virtual public t f_cre_item(t args)
		{

			t item = args["new item"];

			this.args["items"][0] = item;

			this.args["selected_item"] = item;

			lbx_items.Items.Clear();

			lbx_items.Items.Add(item);

			lbx_items.SelectedIndex = 0;

			t.f_f("f_done", this.args);

			//ds.f_hide();

			Hide();

			return new t();
		}

		//��������������
		virtual public t f_modify_item(t args)
		{
			return new t();
		}

		//������������
		virtual public t f_pick_item(t args)
		{
			return new t();
		}

		//��������
		virtual public t f_drop_item(t args)
		{
			return new t();
		}

		//��������������
		virtual public t f_revert_item(t args)
		{
			return new t();
		}

		//������������ ��������
		virtual public t f_opt(t args)
		{
			return new t();
		}

	}

	

}