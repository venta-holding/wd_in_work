﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using kibicom.tlib;

using wd_in_work_gdi;

namespace kibicom.my_wd_helper
{
	public partial class frm_conf : Form
	{

		string using_store = "mssql";

		t_kwj kwj_conf = new t_kwj();

		kibicom_mwh_frm_main frm_helper = new kibicom_mwh_frm_main(new t()
		{
			{
				"josi_store", new t()
				{
					{"login_name", "то"},
					{"pass", "1357"}
				}
			}
		});

		t_wd_josi_num wd_josi_num;

		test_form frm_test = new test_form();

		public frm_conf()
		{
			InitializeComponent();

			SaveFileDialog fsd = new SaveFileDialog();
			if (using_store == "sqlite")
			{

				fsd.FileName = "kibicom_wd_josi.db";

				fsd.ShowDialog();
			}

			kwj_conf = new t_kwj(new t()
			{
				{
					"local_store", new t()
					{
						{"store_type", using_store},
						{
							"sqlite_cli", new t()
 							{
								{"file_name", fsd.FileName}
							}
						},
						{
							"mssql_cli", new t()
 							{
								{"server",					"192.168.1.201"},
								{"server_name",				""},
								{"login",					"sa"},
								{"pass",					"82757662=z"},
								{"db_name",					"kwj_test"}
							}
						}
					}
				}
			});


			wd_josi_num = new t_wd_josi_num(new t()
			{
				{"josi_store", frm_helper.args["kwj"]["josi_store"]},
				//{"josi_end_point_","https://192.168.1.139/webproj/git/kibicom_venta/index.php"},
				//"josi_end_point","https://192.168.1.193/webproj/git/kibicom_venta/index.php"},
				{"josi_end_point","http://kibicom.com/order_store_339/index.php"},
				{"login_name","dnclive"},
				{"req_timeout", 5000},
				{"auth_try_count", 3},
				{"pass","4947"},
				//{"pass","135"},
				{
					"f_done",new t_f<t,t>(delegate(t args1)
					{

						//MessageBox.Show("Залогинились...");

						return new t();
					})
				},
				{
					"f_fail",new t_f<t,t>(delegate(t args1)
					{

						MessageBox.Show("Войти не удалось");

						return new t();
					})
				},
			});

		}

		private void btn_cre_kwj_Click(object sender, EventArgs e)
		{
			if (using_store == "mssql")
			{
				kwj_conf.f_kwj_mssql_cre(new t());
			}
			else if (using_store == "sqlite")
			{
				kwj_conf.f_kwj_sqlite_cre(new t());
			}
		}

		private void btn_fill_from_kibicom_Click(object sender, EventArgs e)
		{

			if (using_store == "mssql")
			{
				kwj_conf.f_fill_tab_customer_mssql(new t());
			}
			else if (using_store == "sqlite")
			{
				kwj_conf.f_fill_tab_customer_sqlite(new t());
			}
		}

		private void btn_fill_tab_address_Click(object sender, EventArgs e)
		{

			if (using_store == "mssql")
			{
				kwj_conf.f_fill_tab_address_mssql(new t());
			}
			else if (using_store == "sqlite")
			{
				kwj_conf.f_fill_tab_address_sqlite(new t());
			}
		}

		private void btn_helper_start_Click(object sender, EventArgs e)
		{
			
			//frm_helper.Owner = this;
			//frm_helper.Owner = null;
			frm_helper.Show(this);
			//frm_test.Show(this);
			//this.Activate();
			//frm_test.Show(this);
		}

		/*
		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}
		
		
		protected override CreateParams CreateParams
		{
			get
			{
				const int WM_NCHITTEST = 0x0084;
				const int WS_EX_TOPMOST = 0x00000008;
				const int WS_EX_TOOLWINDOW = 0x00000080;
				const int WS_EX_NOACTIVATE = 0x08000000;
				//const int WS_EX_TOPMOST = 0x00000008;

				CreateParams baseParams = base.CreateParams;

				baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);

				return baseParams;
			}
		}
		
		*/

		private void frm_conf_Activated(object sender, EventArgs e)
		{
			frm_helper.args["required_state"].f_set("shown");
			//frm_helper.args["main_wd_frm"].f_set("shown");

			//frm_helper.Show(this);
			//frm_test.Show(this);

			//this.Activate();
		}

		private void frm_conf_Deactivate(object sender, EventArgs e)
		{
			frm_helper.args["required_state"].f_set("hidden");
			//frm_helper.args["main_wd_frm"].f_set("hidden");
			//frm_helper.Hide();
			//frm_test.Hide();
			//this.Activate();
		}

		private void btn_start_test_form_Click(object sender, EventArgs e)
		{
			frm_test.Show();
		}

		private void btn_wd_in_work_gdi_Click(object sender, EventArgs e)
		{
			

			wd_josi_num.f_load_wd_order_ds(new t());

			wd_josi_num.f_put_order(new t()
			{
				{"ds", wd_josi_num["ds"].f_val<DataSet>()},
				{
					"f_fail",new t_f<t,t>(delegate(t args1)
					{

						MessageBox.Show("Войти не удалось");

						return new t();
					})
				}
			});
		}

		
	}
}
