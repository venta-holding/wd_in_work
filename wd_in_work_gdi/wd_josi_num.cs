﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using kibicom.josi;
using kibicom.tlib;
using System.Windows.Forms;
using kibicom.wd;
using kibicom;
using Atechnology.DBConnections2;
using kibicom.tlib.data_store_cli;

namespace wd_in_work_gdi
{
	public class t_wd_josi_num:t
	{

		public t_store josi_store;

		dbconn db;

		public t_wd_josi_num()
		{
			josi_store = new t_store(new t()
			{
				{"josi_end_point","https://192.168.1.139/webproj/git/kibicom_venta/index.php"},
				//{"josi_end_point","http://kibicom.com/order_store_33/order_store/index.php"},
				{"login_name","dnclive"}, 
				{"pass","135"},
				{"login_on_cre", true},		//не логиниться при создании
			});
		}

		public t_wd_josi_num(t args)
		{
			//MessageBox.Show(args["josi_store"].f_json()["json_str"].f_str());
			if (!args["josi_store"].f_is_empty())
			{
				josi_store = args["josi_store"].f_val<t_store>();
				return;
			}

			string login_name = args["login_name"].f_def("dnclive").f_str();
			string pass = args["pass"].f_def("135").f_str();
			string josi_end_point = args["josi_end_point"].
				f_def("https://192.168.1.139/webproj/git/kibicom_venta/index.php").f_str();

			josi_store = new t_store(new t()
			{
				{"josi_end_point", josi_end_point},
				{"req_timeout", args["req_timeout"].f_def(5000).f_int()},
				{"login_name",login_name}, 
				{"pass",pass},
				{"login_on_cre", true},		//логинимся
				{"auth_try_count", args["auth_try_count"].f_def(3).f_int()},	//количество попыток авторизации
				{"f_done", args["f_done"].f_f()},
				{"f_fail", args["f_fail"].f_f()}
			});

			this["login_name"] = new t(login_name);
			this["pass"]=new t(pass);
			this["josi_end_point"] = new t(josi_end_point);
		}

		#region получение данных из базы WD

		public t f_load_wd_order_ds(t args)
		{

			t_wd wd = new t_wd();
			string idorder = args["idorder"].f_str();

			//инициализация соединения с базой
			wd.f_init(new t());

			db = new dbconn();

			//получение строки заказа
			DataTable tab_order = wd.f_tab_order(new t()
			{
				{"idorder",idorder}

			})["tab_order"].f_val<DataTable>();

			//инициализация расчета заказа
			//при этом будет сформирован dataset заказа
			//это наша цель
			torder order = new torder(db, tab_order.Rows[0]);//, pb);

			//забираем сформированный dataset
			this["ds"].f_set(order.args.ds);

			return new t();
		}

		public t f_load_wd_model_ds(t args)
		{

			t_wd wd = new t_wd();
			string idorder = args["idorder"].f_str();
			string idorderitem = args["idorderitem"].f_str();

			//инициализация соединения с базой
			wd.f_init(new t());

			DataSet ds = new DataSet();

			db = new dbconn();

			db.command.CommandText =
				"select * from model where deleted is null and idorderitem=" + idorderitem;

			DataTable tab_wd_o = new DataTable("model");

			db.adapter.Fill(tab_wd_o);

			//tab_wd_o.TableName = "model";

			ds.Tables.Add(tab_wd_o);

			//забираем сформированный dataset
			this["ds"].f_set(ds);

			return new t(){{"ds", ds}};
		}

		public t f_load_wd_order_to_get_payment(t args)
		{

			t_wd wd = new t_wd();
			int portion = args["portion"].f_def(100).f_int();

			//инициализация соединения с базой
			wd.f_init(new t());

			db = new dbconn();

			DataTable tab= db.GetDataTable
			(
				@"	select top 100 * 
					from view_order_payment_sm 
					where inwork_dt is not null and o_sm_int>=p_sm_int
					and inwork_dt = '20130819'
				"
			);


			t o_guid_arr = new t();
			string o_guid_arr_str = "";
			foreach (DataRow dr in tab.Rows)
			{
				o_guid_arr.Add(dr["guid"].ToString());
				o_guid_arr_str = t_uti.fjoin(o_guid_arr_str, ',', dr["guid"].ToString());
			}

			t o_name_arr = new t();
			string o_name_arr_str = "";
			foreach (DataRow dr in tab.Rows)
			{
				o_name_arr.Add(dr["name"].ToString());
				o_name_arr_str = t_uti.fjoin(o_name_arr_str, ',', dr["name"].ToString());
			}

			t res= new t()
			{
				{"self", this},
				{"tab",tab},
				{"o_guid_arr", o_guid_arr},
				{"o_guid_arr_str", o_guid_arr_str},
				{"o_name_arr", o_name_arr},
				{"o_name_arr_str", o_name_arr_str}
			};
			res["f_done"].f_set(new t_f<t, t>(delegate(t f)
			{
				t.f_f(f.f_f(), res);

				return res;
			}));
			res["f_fail"].f_set(new t_f<t, t>(delegate(t f)
			{
				//t.f_f(f.f_f(), res);

				return res;
			}));

			return res;

		}

		#endregion получение данных из базы WD

		#region заказы

		public t f_get_num(t args)
		{

			string idseller = args["idseller"].f_def(0).f_str();

			//запрос номера
			string res_dot_key_query_str = "kvl.0.f=service_wd_f_get_order_num_year&"+
											"kvl.1.wd_idseller="+idseller;

			//выполняем запрос
			josi_store.f_query(new t 
			{
				{"res_dot_key_query_str",res_dot_key_query_str},
				{"f_done",args["f_done"].f_f()},	//когда возвращен ответ зовем callback
				{"f_fail",args["f_fail"].f_f()},	//когда возвращен ответ зовем callback
				{"encode_json",true},
				{"cancel_prev",false},
				{"is_need_auth",true},
				{"needs", new t(){"is_auth_done"}}		//когда выполниться процесс авторизации
			});

			return new t();
		}

		private t _f_store_order(t args)
		{

			DataSet ds = args["ds"].f_val<DataSet>();
			db = new dbconn();
			string kibicom_order_id = args["kibicom_order_id"].f_def("").f_str();

			DataRow o_dr = ds.Tables["orders"].Select("deleted is null")[0];

			db.command.CommandText = 
				"select * from view_kibicom_wd_order where idorder=" + o_dr["idorder"].ToString();

			DataTable tab_wd_o=null;

			db.adapter.Fill(tab_wd_o);

			//string idseller = o_dr["idseller"].ToString();
			string order_name = o_dr["name"].ToString();
			string order_dtcre = t_uti.f_mssql_dt(o_dr["dtcre"].ToString());
			string order_comment = o_dr["comment"].ToString();
			string order_guid = o_dr["guid"].ToString();

			DataRow c_dr = ds.Tables["customer"].
							Select("deleted is null and idcustomer="+o_dr["idcustomer"].ToString())[0];
			string idcustomer=c_dr["idcustomer"].ToString();
			string customer_name = c_dr["name"].ToString();
			string address_name = c_dr["name"].ToString();
			string customer_guid = c_dr["guid"].ToString();

			/*** менеджер ***/
			DataRow[] od_manager_dr = ds.Tables["orderdiraction"].
				Select("deleted is null and diraction_name like 'Менеджер' and idorder=" + o_dr["idorder"].ToString());

			DataRow[] odp_manager_dr = null;
			if (od_manager_dr.Length > 0)
			{
				odp_manager_dr = ds.Tables["orderdiractionpeople"].
							Select("deleted is null and idorder=" + o_dr["idorder"].ToString() +
									" and idorderdiraction=" + od_manager_dr[0]["idorderdiraction"].ToString());

			}

			string manager_idpeople = "";
			string manager_people_guid = "";
			if (odp_manager_dr != null && odp_manager_dr.Length > 0)
			{
				manager_idpeople = odp_manager_dr[0]["idpeople"].ToString();
				manager_people_guid = odp_manager_dr[0]["guid"].ToString();
			}

			/*** технолог ***/
			DataRow[] od_tech_dr = ds.Tables["orderdiraction"].
				Select("deleted is null and diraction_name like 'Технолог' and idorder=" + o_dr["idorder"].ToString());

			DataRow[] odp_tech_dr = null;
			if (od_tech_dr.Length > 0)
			{
				odp_tech_dr = ds.Tables["orderdiractionpeople"].
							Select("deleted is null and idorder=" + o_dr["idorder"].ToString() +
									" and idorderdiraction=" + od_tech_dr[0]["idorderdiraction"].ToString());
			}

			string tech_idpeople = "";
			string tech_people_guid = "";
			if (odp_tech_dr != null && odp_tech_dr.Length > 0)
			{
				tech_idpeople = odp_tech_dr[0]["idpeople"].ToString();
				tech_people_guid = odp_manager_dr[0]["guid"].ToString();
			}

			/*** продавец ***/

			DataRow[] s_dr = ds.Tables["seller"].
							Select("deleted is null and idseller=" + o_dr["idseller"].ToString());
			
			string idseller="";
			string seller_guid="";
			if (s_dr != null && s_dr.Length > 0)
			{
				idseller = s_dr[0]["idseller"].ToString();
				//string customer_name = c_dr["name"].ToString();
				//string address_name = c_dr["name"].ToString();
				seller_guid = s_dr[0]["guid"].ToString();
			}

			/*** профиль ***/


			
			
			/*** структура заказа ***/

			t order = new t()
			{
				
				{
					"_relat",new t()
					{
						{
							"one_to_many",new t()
							{
								"tab_org_unit",
								"tab_sale_office",
								"tab_customer",
								"tab_address",
								"tab_wd_prof_sys",
								"tab_order_sign",
								"tab_adv_type"
								//"tab_concerned_people",
							}
						}
					}
				},
				{"id",kibicom_order_id},
				{"name",order_name},
				{"dt_make",order_dtcre},
				{"is_credit",""},
				{"is_vip",""},
				{"discount_zp",""},
				{"terminal",""},
				{"comment",order_comment},
				{"wd_order_guid",order_guid},
				{
					"tab_org_unit",new t()
					{
						new t()
						{
							{"id",""},
							{"name","Пластик"},
							{"_no_update",true}
						}
				
					}
				},
				{
					"tab_sale_office", new t()
					{
						new t()
						{
							{"_no_update",true},
							//_update_if_empty:true,
							{"id",""},
							{"name",""},
							//{"wd_idseller", idseller},
							{"dw_seller_guid", seller_guid},
						}
					}
				},
				{
					"tab_customer", new t()
					{
						new t()
						{
							{"_update_if_empty",true},
							{"id",""},
							{"name",customer_name},
							{"fio",""},
							{"phone",""},
							{"email",""},
							//{"wd_idcustomer", idcustomer},
							{"wd_customer_guid", customer_guid}
						}
					}
				},
				{
					"tab_address",new t()
					{
						new t()
						{
							{"_update_if_empty",true},
							{"id",""},
							{"name",customer_name},
							//{"wd_idaddress", idcustomer},
							{"wd_customer_guid", customer_guid}
						}
					}
				},
						
				{
					"tab_concerned_people", new t()
					{
						new t()
						{
							//_update_if_empty:true,
							{"id",""},
							{
								"tab_people_prof", new t()
								{
									new t()
									{
										{"_update_if_empty",true},
										//_no_update:true,
										{"id",""},
										{"name","Менеджер"}
									}
								}
							},
							{
								"tab_people", new t()
								{
									//_update_if_empty:true,
									{"_no_update",true},
									{"id",""},
									{"name",""},
									//{"wd_idpeople", manager_idpeople},
									{"wd_people_guid", manager_people_guid},
									{
										"tab_people_prof", new t()
										{
											new t()
											{
												{"_update_if_empty",true},
												//_no_update:true,
												{"id",""},
												{"name","Менеджер"}
											}
										}
									}
								}
							}
						},
						new t()
						{
							//_update_if_empty:true,
							{"id",""},
							{
								"tab_people_prof", new t()
								{
									new t()
									{
										{"_update_if_empty",true},
										//_no_update:true,
										{"id",""},
										{"name","Технолог"}
									}
								}
							},
							{
								"tab_people", new t()
								{
									new t()
									{
										//_update_if_empty:true,
										{"_no_update",true},
										{"id",""},
										{"name",""},
										//{"wd_idpeople", tech_idpeople},
										{"wd_people_guid", tech_people_guid},
										{
											"tab_people_prof", new t()
											{
												new t()
												{
													{"_update_if_empty",true},
													//_no_update:true,
													{"id",""},
													{"name","Технолог"}
												}
											}
										}
									}
								}
							}
						}
					}
				}
				/*,
				{
					"tab_wd_prof_sys", new t()
					{
						new t()
						{
							//_update_if_empty:true,
							{"_no_update",true},
							{"id",""},
							{"name",""},
							//{"wd_idprofsys", ""},
						}
					}
				},
				{
					"tab_order_sign", new t()
					{
						new t()
						{
							//_no_update:true,
							{"_update_if_empty",true},
							{"id",""},
							{"name",""}
						}
					}
				},
				{
					"tab_adv_type", new t()
					{
						new t()
						{
							{"_no_update",true},
							//_update_if_empty:true,
							{"id",""},
							{"name",""}
						}
					}
				}*/
			};


			

			//return new t();

			//выполняем запрос
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				//когда возвращен ответ
				{"method", "POST"},
				{
					"put_tab_arr", new t()
					{
						{"tab_order", new t(){order}}
					}
				},
				{"f_done",args["f_done"].f_f()},
				{"f_fail",args["f_fail"].f_f()},
				{"encode_json",true},
				{"cancel_prev",false},
			});

			return new t();

		}

		private t _f_store_order_3(t args)
		{

			MessageBox.Show("try save order");

			DataSet ds = args["ds"].f_def(this["ds"].f_val<DataSet>()).f_val<DataSet>();
			db = new dbconn();
			string idorder = args["idorder"].f_str();
			//idorder = "234234";
			string kibicom_order_id = args["kibicom_order_id"].f_def("").f_str();


			MessageBox.Show(args["ds"].f_val<DataSet>().Tables["orders"].Select("deleted is null").Length.ToString());
			MessageBox.Show(idorder);

			DataRow[] o_dr_arr = ds.Tables["orders"].Select("deleted is null and idorder=" + idorder);

			MessageBox.Show(o_dr_arr.Length.ToString());

			if (o_dr_arr.Length == 0)
			{
				t.f_f(args["f_fail"].f_f(), args.f_dub_drop(new string[] { "f_done", "f_fail" }).f_add(true, new t()
				{
					{
						"err", new t()
						{
							{ "message", "не смог найти заказ idorder="+idorder}
						}
					}
				}));
				return new t();
			}

			MessageBox.Show("try save order 2");

			DataRow o_dr = o_dr_arr[0];

			//db.command.CommandText =
			//	"select * from view_kibicom_wd_order where idorder=" + o_dr["idorder"].ToString();

			DataTable tab_wd_o = db.GetDataTable
				("select * from view_kibicom_wd_order where idorder=" + o_dr["idorder"].ToString());

			//db.adapter.Fill(tab_wd_o);

			if (tab_wd_o.Rows.Count == 0)
			{

				t.f_f("f_fail", args.f_dub_drop(new string[] { "f_done", "f_fail" }).f_add(true, new t()
				{
					{
						"err", new t()
						{
							{ 
								"message", 
								"select * from view_kibicom_wd_order where idorder=" 
								+ o_dr["idorder"].ToString()
								+"\r\nЗапрос вернул пустой результат..."
							}
						}
					}
				}));

				return new t();
			}

			DataRow wd_o_dr = tab_wd_o.Rows[0];

			//string idseller = o_dr["idseller"].ToString();
			string order_name = wd_o_dr["order_name"].ToString();
			string order_dtcre = t_uti.f_mssql_dt(wd_o_dr["order_dtcre"].ToString());
			string order_comment = wd_o_dr["order_comment"].ToString();
			string order_smbase = Math.Round(Convert.ToDouble(wd_o_dr["order_smbase"].ToString()))
									.ToString().Replace(',', '.');
			string order_guid = wd_o_dr["order_guid"].ToString();

			string customer_name = wd_o_dr["customer_name"].ToString();
			string customer_guid = wd_o_dr["customer_guid"].ToString();

			string address_name = wd_o_dr["address_name"].ToString();
			string address_guid = wd_o_dr["address_guid"].ToString();

			string man_name = wd_o_dr["man_name"].ToString();
			string man_guid = wd_o_dr["man_guid"].ToString();

			string tech_name = wd_o_dr["tech_name"].ToString();
			string tech_guid = wd_o_dr["tech_guid"].ToString();

			/*** продавец ***/
			string seller_name = wd_o_dr["seller_name"].ToString();
			string seller_guid = wd_o_dr["seller_guid"].ToString();
			//MessageBox.Show(seller_guid);

			/*** профиль фурнитура***/
			string profsys_name = wd_o_dr["profsys_name"].ToString();
			string furnsys_name = wd_o_dr["furnsys_name"].ToString();

			/*** структура заказа ***/

			t order = new t()
			{
				
				{
					"_relat",new t()
					{
						{
							"one_to_many",new t()
							{
								"tab_org_unit",
								"tab_sale_office",
								"tab_customer",
								"tab_address",
								"tab_wd_prof_sys",
								"tab_order_sign",
								"tab_adv_type",
								"tab_concerned_people"
							}
						}
					}
				},
				{"id",kibicom_order_id},
				{"name",order_name},
				{"dt_make",order_dtcre},
				{"is_credit",""},
				{"is_vip",""},
				{"discount_zp",""},
				{"terminal",""},
				{"comment",order_comment},
				{"wd_order_guid",order_guid},
				{"sm", order_smbase},
				{"is_real_order",0},
				{
					"tab_org_unit",new t()
					{
						new t()
						{
							{"id",""},
							{"name","Пластик"},
							{"_no_update",true}
						}
				
					}
				},
				{
					"tab_sale_office", new t()
					{
						new t()
						{
							{"_no_update",true},
							//_update_if_empty:true,
							{"id",""},
							//{"name",""},
							//{"wd_idseller", idseller},
							{"wd_seller_guid", seller_guid},
						}
					}
				},
				{
					"tab_customer", new t()
					{
						new t()
						{
							{"_update_if_empty",true},
							{"id",""},
							{"name",customer_name},
							{"fio",""},
							{"phone",""},
							{"email",""},
							//{"wd_idcustomer", idcustomer},
							{"wd_customer_guid", customer_guid}
						}
					}
				},
				{
					"tab_address",new t()
					{
						new t()
						{
							{"_update_if_empty",true},
							{"id",""},
							{"name",address_name},
							//{"wd_idaddress", idcustomer},
							{"wd_address_guid", address_guid}
						}
					}
				},
						
				{
					"tab_concerned_people", new t()
					{
						new t()
						{
							//_update_if_empty:true,
							{"id",""},
							{
								"tab_people_prof", new t()
								{
									new t()
									{
										{"_update_if_empty",true},
										//_no_update:true,
										{"id",""},
										{"name","Менеджер"}
									}
								}
							},
							{
								"tab_people", new t()
								{
									new t()
									{
										{"_update_if_empty",true},
										//{"_no_update",true},
										{"id",""},
										{"name",man_name},
										//{"wd_idpeople", manager_idpeople},
										{"wd_people_guid", man_guid},
										{
											"tab_people_prof", new t()
											{
												new t()
												{
													{"_update_if_empty",true},
													//_no_update:true,
													{"id",""},
													{"name","Менеджер"}
												}
											}
										}
									}
								}
							}
						},
						new t()
						{
							//_update_if_empty:true,
							{"id",""},
							{
								"tab_people_prof", new t()
								{
									new t()
									{
										{"_update_if_empty",true},
										//_no_update:true,
										{"id",""},
										{"name","Технолог"}
									}
								}
							},
							{
								"tab_people", new t()
								{
									new t()
									{
										{"_update_if_empty",true},
										//{"_no_update",true},
										{"id",""},
										{"name",tech_name},
										//{"wd_idpeople", tech_idpeople},
										{"wd_people_guid", tech_guid},
										{
											"tab_people_prof", new t()
											{
												new t()
												{
													{"_update_if_empty",true},
													//_no_update:true,
													{"id",""},
													{"name","Технолог"}
												}
											}
										}
									}
								}
							}
						}
					}
				},
				{
					"tab_wd_prof_sys", new t()
					{
						new t()
						{
							{"_update_if_empty",true},
							//{"_no_update",true},
							{"id",""},
							{"name",profsys_name},
							//{"wd_idprofsys", ""},
						}
					}
				}
				,
				{
					"tab_order_sign", new t()
					{
						new t()
						{
							{"_no_update",true},
							//{"_update_if_empty",true},
							{"id",""},
							{"name","Заказчик"}
						}
					}
				}
				/*,
				{
					"tab_adv_type", new t()
					{
						new t()
						{
							{"_no_update",true},
							//_update_if_empty:true,
							{"id",""},
							{"name",""}
						}
					}
				}*/
			};


			//string order_json = order.f_json()["json_str"].f_str();

			//MessageBox.Show(order_json);

			//return new t();

			//выполняем запрос
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				//когда возвращен ответ
				//{"debug_group", "tstore_relat_one_to_many"},
				//{"debug_group", "tstore_sql"},
				{"method", "POST"},
				{
					"put_tab_arr", new t()
					{
						{"tab_order", new t(){order}}
					}
				},
				{"f_done_",args["f_done"].f_f()},
				{
					"f_done", new t_f<t,t>(delegate(t args5)
					{
						if (kibicom_order_id == "")
						{
							if (args5["resp_str"].f_str().Contains("id"))
							{
								kibicom_order_id = t_dot.f_get_val_from_json_obj
								(
									args5["resp_json"].f_val(),
									"tab_order.0.id"
								).ToString();
							}
						}

						t.f_f(args["f_done"].f_f(), args5.f_dub().
							f_add(true, new t(){{"kibicom_order_id",kibicom_order_id}}));

						return new t();
					})
				},
				{"f_fail",args["f_fail"].f_f()},
				{"encode_json",true},
				{"cancel_prev",false},
				{"needs", new t(){"is_auth_done","authenticated"}}		//когда выполниться процесс авторизации
			});

			return new t();

		}

		public t f_put_order(t args)
		{
			//MessageBox.Show(josi_store.f_json()["json_str"].f_str());
			DataSet ds = args["ds"].f_val<DataSet>();
			DataRow o_dr = ds.Tables["orders"].Select("deleted is null")[0];

			string order_guid = o_dr["guid"].ToString();

			t order_get = new t()
			{
				{"id",""},
				{"wd_order_guid",order_guid}
			};

			//MessageBox.Show(order_guid);

			//выполняем запрос kibicom id заказа по guid
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				//когда возвращен ответ
				{"method", "POST"},
				//{"debug_group", "tstore_sql"},
				{
					"get_tab_arr", new t()
					{
						{"tab_order", new t(){order_get}}
					}
				},
				{
					//когда получен id
					//сохраняем заказ с учетом полученного id
					"f_done", new t_f<t,t>(delegate(t args1)
					{
						string kibicom_order_id = "";
						if (args1["resp_str"].f_str().Contains("id"))
						{
							kibicom_order_id = t_dot.f_get_val_from_json_obj
							(
								args1["resp_json"].f_val(),
								"tab_order.0.id"
							).ToString();
						}

						//выполняем сохранение текущей информации по заказу
						_f_store_order_3(new t().f_add(true,args).f_add(true, new t() 
						{ 
							{ "kibicom_order_id", kibicom_order_id },
							{
								"f_done", new t_f<t,t>(delegate(t args2)
								{
									
									//MessageBox.Show("done");

									t.f_f(args["f_done"].f_f(), args2);

									return new t();
								})
							},
							{
								"f_fail", new t_f<t,t>(delegate(t args2)
								{

									//MessageBox.Show("fail");

									t.f_f("f_fail", args);

									return new t();
								})
							}
						}));

						return new t();
					})
				},
				{"f_fail",args["f_fail"]},
				{"encode_json",true},
				{"cancel_prev",false},
				{"needs", new t(){"is_auth_done","authenticated"}}		//когда выполниться процесс авторизации
			});

			return new t();
		}

		#endregion заказы

		#region этапы

		//отправляем этапы в Кибиком
		public t f_put_order_diraction(t args)
		{

			DataSet ds = args["ds"].f_def(this["ds"].f_val<DataSet>()).f_val<DataSet>();
			string idorder = args["idorder"].f_str();
			DataRow[] o_dr_arr = ds.Tables["orders"].Select("deleted is null and idorder="+idorder);

			MessageBox.Show(ds.Tables["orders"].Select("deleted is null").Length.ToString());
			MessageBox.Show(idorder);

			if (o_dr_arr.Length==0)
			{
				t.f_f(args["f_fail"].f_f(), args.f_dub_drop(new string[]{"f_done","f_fail"}).f_add(true, new t()
				{
					{
						"err", new t()
						{
							{ "message", "не смог найти заказ idorder="+idorder}
						}
					}
				}));
				return new t();
			}

			DataRow o_dr = o_dr_arr[0];

			//MessageBox.Show(josi_store.f_json()["json_str"].f_str());

			string order_guid = o_dr["guid"].ToString();

			//MessageBox.Show(order_guid);

			t order_get = new t()
			{
				{"id",""},
				{"wd_order_guid",order_guid}
			};

			//MessageBox.Show(order_guid);

			//выполняем запрос kibicom id заказа по guid
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				
				{"method", "POST"},
				//{"debug_group", "tstore_sql"},
				{
					"get_tab_arr", new t()
					{
						{"tab_order", new t(){order_get}}
					}
				},
				{
					//когда возвращен ответ
					//когда получен id
					//сохраняем заказ с учетом полученного id
					"f_done", new t_f<t,t>(delegate(t args1)
					{
						string kibicom_order_id = "";
						if (args1["resp_str"].f_str().Contains("id"))
						{
							kibicom_order_id = t_dot.f_get_val_from_json_obj
							(
								args1["resp_json"].f_val(),
								"tab_order.0.id"
							).ToString();
						}

						t_f<t, t> f_store_dir = new t_f<t, t>(delegate(t args3)
						{



							return new t();
						});

						MessageBox.Show(kibicom_order_id);

						//если в базе кибиком еще нет этого заказа
						if (kibicom_order_id=="")
						{

							MessageBox.Show(ds.Tables["orders"].Select("deleted is null").Length.ToString());
							MessageBox.Show(idorder);

							//выполняем сохранение текущей информации по заказу
							_f_store_order_3(new t().f_add(true, args).f_add(true, new t() 
							{ 
								{ "kibicom_order_id", kibicom_order_id },
								{"ds", ds},
								{"idorder", idorder},
								{
									//если заказ сохранен успешно
									"f_done", new t_f<t,t>(delegate(t args2)
									{
										//сохраняем этапы
										MessageBox.Show("done");

										if (kibicom_order_id=="")
										{
											kibicom_order_id = args2["kibicom_order_id"].f_str();
										}

										_f_store_order_diraction_3(new t()
										{
											{ "kibicom_order_id", kibicom_order_id },
											{"ds", ds},
											{
												//если заказ сохранен успешно
												"f_done", new t_f<t,t>(delegate(t args3)
												{
													//сохраняем этапы


													//MessageBox.Show("done");

													t.f_f
													(
														args["f_done"].f_f(), 
														args3.f_dub_drop(new string[] { "f_done", "f_fail" })
													);

													return new t();
												})
											},
											{
												"f_fail", new t_f<t,t>(delegate(t args3)
												{

													//MessageBox.Show("fail");

													t.f_f(args["f_fail"].f_f(), args3);

													return new t();
												})
											}
										});

										//t.f_f("f_done", args);

										return new t();
									})
								},
								{
									"f_fail", new t_f<t,t>(delegate(t args2)
									{

										//MessageBox.Show("fail");

										t.f_f(args["f_fail"].f_f(), args2);

										return new t();
									})
								}
							}));
						}
						else
						{
							_f_store_order_diraction_3(new t()
							{
								{ "kibicom_order_id", kibicom_order_id },
								{"ds", ds},
								{
									//если заказ сохранен успешно
									"f_done", new t_f<t,t>(delegate(t args2)
									{
										//сохраняем этапы


										//MessageBox.Show("done");

										//t new_args=args2.f_dub_drop(new string[]{"f_done","f_fail"});

										t.f_f(args["f_done"].f_f(), args2.f_dub_drop
										(
											new string[] { "f_done", "f_fail" }
										));

										return new t();
									})
								},
								{
									"f_fail", new t_f<t,t>(delegate(t args2)
									{

										//MessageBox.Show("fail");

										t.f_f(args["f_fail"].f_f(), args2);

										return new t();
									})
								}
							});
						}

						

						return new t();
					})
				},
				{"f_fail",args["f_fail"]},
				{"encode_json",true},
				{"cancel_prev",false},
				{"needs", new t(){"is_auth_done","authenticated"}}		//когда выполниться процесс авторизации
			});

			return new t();
		}

		//сохранение этапов в Кибиком
		private t _f_store_order_diraction_3(t args)
		{
			DataSet ds = args["ds"].f_def(this["ds"].f_val<DataSet>()).f_val<DataSet>();
			db = new dbconn();
			string kibicom_order_id = args["kibicom_order_id"].f_def("").f_str();

			DataRow o_dr = ds.Tables["orders"].Select("deleted is null")[0];

			//db.command.CommandText =
			//	"select * from view_kibicom_wd_order where idorder=" + o_dr["idorder"].ToString();

			DataTable tab_wd_o_d = db.GetDataTable
				("select * from view_kibicom_wd_order_diraction where idorder=" + o_dr["idorder"].ToString());

			//db.adapter.Fill(tab_wd_o);

			//MessageBox.Show(kibicom_order_id+" "+o_dr["idorder"].ToString()+" "+tab_wd_o_d.Rows.Count.ToString());

			if (tab_wd_o_d.Rows.Count == 0)
			{

				t.f_f("f_fail", args);

				return new t();
			}

			DataRow wd_o_dr = tab_wd_o_d.Rows[0];

			string order_guid = wd_o_dr["order_guid"].ToString();
			string is_real_order = wd_o_dr["is_real_order"].ToString();

			//MessageBox.Show(is_real_order);

			/*** профиль фурнитура***/
			string profsys_name = wd_o_dr["profsys_name"].ToString();
			string furnsys_name = wd_o_dr["furnsys_name"].ToString();

			
			/*** структура этапов заказа ***/

			t order_diraction = new t();

			foreach (DataRow dr in tab_wd_o_d.Rows)
			{

				//DateTime dt = new DateTime();
				//DateTime.TryParse(dr["plandate"].ToString(), out dt);

				//MessageBox.Show(dr["plandate"].ToString() + "\r\n" + dt.ToString());

				order_diraction.Add(new t()
				{
					//{"_id_key", "wd_order_guid"},
					{"wd_order_guid",dr["order_guid"].ToString()},
					{"id",""},
					{
						"tab_stage", new t()
						{
							new t()
							{
								{"_id_key", "wd_diraction_guid"},
								{"wd_diraction_guid",dr["diraction_guid"].ToString()},
								{"name",dr["diraction_name"].ToString()},
							}
						}
					},
					{"plandt",t_uti.f_mssql_dt(dr["diraction_plandate"].ToString())},
					{"factdt",t_uti.f_mssql_dt(dr["diraction_factdate"].ToString())},
					{"comment",dr["diraction_comment"].ToString()}
				});
			}

			/*** структура заказа ***/

			t order = new t()
			{
				
				{
					"_relat",new t()
					{
						{
							"one_to_many",new t()
							{
								"tab_doc_stage",
							}
						}
					}
				},
				{"id",kibicom_order_id},
				{"name",""},
				{"dt_make",""},
				{"is_credit",""},
				{"is_vip",""},
				{"discount_zp",""},
				{"terminal",""},
				{"comment",""},
				{"sm", ""},
				{"wd_order_guid",order_guid},
				{"is_real_order", is_real_order},
				{"tab_doc_stage",order_diraction}
			};


			//string order_json = order.f_json()["json_str"].f_str();

			//MessageBox.Show(order_json);

			//return new t();

			//выполняем запрос
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				//когда возвращен ответ
				//{"debug_group", "tstore_relat_one_to_many"},
				//{"debug_group", "tstore_drop_old"},
				{"method", "POST"},
				{
					"put_tab_arr", new t()
					{
						{"tab_order", new t(){order}}
					}
				},
				{"f_done",args["f_done"].f_f()},
				{"f_fail",args["f_fail"].f_f()},
				{"encode_json",true},
				{"cancel_prev",false},
				{"needs", new t(){"is_auth_done","authenticated"}}		//когда выполниться процесс авторизации
			});

			return new t();

		}

		//получение этапов из Кибиком
		public t f_get_order_diraction(t args)
		{

			string idseller = args["idseller"].f_def(0).f_str();
			string login_name = args["login_name"].f_def(this["login_name"].f_val()).f_def("dnclive").f_str();
			string pass = args["pass"].f_def(this["pass"].f_val()).f_def("135").f_str();

			//авторизуемся
			josi_store.f_login(new t()
			{
				{"login_name",login_name}, 
				{"pass",pass},
				{	
					//если и когда войти удалось
					"f_done", new t_f<t,t>(delegate(t args1)
					{
						//запрос номера
						string res_dot_key_query_str = "kvl.0.f=service_wd_f_get_order_num&"+
														"kvl.1.wd_idseller="+idseller;

						//выполняем запрос
						josi_store.f_query(new t 
						{
							{"res_dot_key_query_str",res_dot_key_query_str},
							//когда возвращен ответ
							{"f_done",args["f_done"].f_f()},
							{"encode_json",true},
							{"cancel_prev",false},
						});

						return new t();
					})
				}
			});

			return new t();
		}

		#endregion этапы

		#region платежи

		public t f_get_payment(t args)
		{
			DataTable tab = args["tab"].f_val<DataTable>();
			t o_guid_arr = args["o_guid_arr"];
			t o_name_arr = args["o_name_arr"];
			RichTextBox rtxt_log = args["rtxt_log"].f_val<RichTextBox>();

			t ret = t.f_when_cre(args, this);

			/*** структура заказа ***/

			//string json_str = o_guid_arr.f_json()["json_str"].f_str();

			//MessageBox.Show("123");

			//MessageBox.Show(json_str["json_str"].f_str());

			t order = new t()
			{
				{"id",""},
				//{"wd_order_guid", o_guid_arr},
				{"name", o_name_arr},
				{"sm", ""},
				{	"_important_sub",new t()
					{
						"tab_payment"
					}
				},
				{"_limit", new t(){4000}},
				{
					"tab_payment", new t()
					{
						new t()
						{
							{"id",""},
							{"name",""},
							{"dt_make",""},
							{"sm",""},
							{"comment",""},
							{"confirm",""},
							{"wd_paymentdoc_guid", ""},
							{
								"tab_payment_way", new t()
								{
									new t()
									{
										{"id",""},
										{"name",""}
									}
								}
							}
						}
					}
				}
			};


			string order_json = order.f_json()["json_str"].f_str();

			rtxt_log.Text = order_json;

			//MessageBox.Show(order_json);

			//return new t();

			t res = new t()
			{
				{"self", this}
			};
			res["f_done"].f_set(new t_f<t, t>(delegate(t f_arg)
			{
				//добавляем функцию в массив вызываемых когда _f_done true
				res["f_arr"]["f_done"].Add(f_arg);

				if (res["done_arr"]["f_done"].f_bool())
				{
					foreach (t f in (IList<t>)res["f_arr"]["f_done"])
					{
						t.f_f(f.f_f(), res);
					}

					res["f_arr"]["f_done"].Clear();
				}

				return res;
			}));
			res["f_fail"].f_set(new t_f<t, t>(delegate(t f_arg)
			{
				//добавляем функцию в массив вызываемых когда _f_done true
				res["f_arr"]["f_fail"].Add(f_arg);

				if (res["done_arr"]["f_fail"].f_bool())
				{
					foreach (t f in (IList<t>)res["f_arr"]["f_fail"])
					{
						t.f_f(f.f_f(), res);
					}

					res["f_arr"]["f_fail"].Clear();
				}

				return res;
			}));
			res["f_set"].f_set(new t_f<t, t>(delegate(t args1)
			{
				string f_name = args1["f_name"].f_str();
				t f_args = args1["f_args"];
				res["done_arr"][f_name].f_set(true);

				foreach (t f in (IList<t>)res["f_arr"][f_name])
				{
					t.f_f(f.f_f(), f_args);
				}

				res["f_arr"][f_name].Clear();

				return res;
			}));

			//если нет фильтра не по guid не по имени заказа
			//не нужно выполнять запрос
			if (o_guid_arr.f_is_empty() && o_name_arr.f_is_empty())
			{

				ret.f_when_done(new t()
				{
					{"f_name", "f_fail"},
					{"f_args",  ret.f_fail(args, "в базе WD нет заказов для которых необходимо получить платежи.")}
				});

				/*
				t.f_f(res["f_set"].f_f(), new t() 
				{ 
					{ "f_name", "f_fail" }, 
					{ "f_args", new t(){{"message", "в базе WD нет заказов для которых необходимо получить платежи."}}},
				});
				*/

				//return res;
				return ret;
			}


			//выполняем запрос
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				//когда возвращен ответ
				//{"debug_group", "tstore_relat_one_to_many"},
				//{"debug_group", "tstore_sql"},
				{"req_timeout", 30000},
				{"method", "POST"},
				{
					"get_tab_arr", new t()
					{
						{"tab_order", new t(){order}}
					}
				},
				{
					"f_done",new t_f<t, t>(delegate(t args1)
					{
						//t.f_f(res["f_set"].f_f(), new t() { { "f_name", "f_done"}, {"f_args", args1} });

						ret.f_when_done(new t()
						{
							{"f_name", "f_done"},
							{"f_args",  args1}
						});

						//t.f_f(args["f_done"].f_f(), args1);

						//return res;
						return ret;
					})
				},
				{
					"f_fail",new t_f<t, t>(delegate(t args1)
					{
						//t.f_f(res["f_set"].f_f(), new t() { { "f_name", "f_fail" } });

						ret.f_when_done(new t()
						{
							{"f_name", "f_fail"},
							{"f_args",  args1}
						});

						//t.f_f(args["f_done"].f_f(), args1);

						//return res;
						return ret;
					})
				},
				//{"f_fail",args["f_fail"].f_f()},
				{"encode_json",true},
				{"cancel_prev",false},
				{"needs", new t(){"is_auth_done","authenticated"}}		//когда выполниться процесс авторизации
			});

			

			return ret;
		}

		public t f_put_payment_wd(t args)
		{
			t payment = args["payment"];
			t order=args["order"];
			t payments_to_kibicom = args["payments_to_kibicom"];
			t tab_order = args["tab_order"];
			int order_i = args["order_i"].f_int();
			int payment_i = args["payment_i"].f_int();

			order = tab_order[order_i];
			payment = order["tab_payment"][payment_i];

			db = new dbconn();

			//возвращаемые данные
			//t res = new t()
			//{
			//	{"payments_to_kibicom",payments_to_kibicom},
			//	{"self", this}
			//};

			t res = t.f_when_cre(args, this);

			if (payment["dt_make"].f_is_empty() || payment["sm"].f_is_empty())
			{
				return res;
			}

			t_msslq_cli mssql_cli=new t_msslq_cli(new t()
			{
				{"conn", db.command.Connection}
			});

			this["mssql_cli"].f_set(mssql_cli);

			DataTable tab_payment = db.GetDataTable("select top 0 * from paymentdoc");
			tab_payment.TableName = "paymentdoc";
			DataTable tab_paymentgroup = db.GetDataTable("select * from paymentgroup");
			//DataRow dr_o=db.GetDataRow("select * from orders where guid ='"+order["wd_order_guid"].f_str()+"' ");
			DataRow dr_o = db.GetDataRow
			(
				"select * from orders where deleted is null and name = '" + order["name"].f_str() + "' "
			);

			//если у платежа нет guid из WD значит платеж новый
			if (payment["wd_paymentdoc_guid"].f_is_empty())
			{

				DataRow dr = tab_payment.NewRow();

				string idpaymentdoc = mssql_cli.f_exec_cmd(new t()
				{
					{"cmd" , "exec gen_id 'gen_paymentdoc'"},
					{"exec_scalar", true}
				})["res_cnt"].f_str();

				Guid guid = Guid.NewGuid();

				//MessageBox.Show(payment["tab_payment_way"][0]["name"].f_str());


				DataRow[] pg_dr_arr = 
					tab_paymentgroup.Select("name = '" + payment["tab_payment_way"][0]["name"].f_str()+"'");

				dr["idpaymentdoc"] = idpaymentdoc;
				dr["idorder"] = dr_o["idorder"].ToString();
				dr["name"] = payment["name"].f_str();
				dr["idpaymentdocgroup"] = f_get_idpaymentdocgroup(new t() { { "payment", payment } })["idpaymentdocgroup"].f_str();
				dr["idpaymentgroup"] = (pg_dr_arr.Length > 0 ? "0" : pg_dr_arr[0]["idpaymentgroup"].ToString());
				dr["iddocoper"] = 5;
				dr["smbase"] = payment["sm"].f_str();
				dr["comment"] = payment["comment"].f_str();
				dr["guid"] = guid;

				tab_payment.Rows.Add(dr);
				
				mssql_cli.f_put_store_de(new t()
				{
					{"tab", tab_payment},
					{"key_name", "idpaymentdoc"},
					{
						"f_done", new t_f<t,t>(delegate (t args2)
						{
							//запоминаем сформированный guid и помещаем на отправку в кибиком
							payment["wd_paymentdoc_guid"].f_set(guid.ToString().ToLower());
							payments_to_kibicom.Add(payment);

							res.f_when_done(new t()
							{
								{"f_name", "f_done"},
								{"f_args",  new t(){{"payments_to_kibicom",payments_to_kibicom}}}
							});
							return new t();
						})
					}
				});

				return res;
				
				/*
				mssql_cli.f_store_tab(new t()
				{
					{"tab", tab_payment},
					{
						"f_done", new t_f<t,t>(delegate (t args2)
						{
							//запоминаем сформированный guid и помещаем на отправку в кибиком
							payment["wd_paymentdoc_guid"].f_set(guid.ToString().ToLower());
							payments_to_kibicom.Add(payment);

							res.f_when_done(new t()
							{
								{"f_name", "f_done"},
								{"f_args",  new t(){{"payments_to_kibicom",payments_to_kibicom}}}
							});
							return new t();
						})
					}
				});
				*/

			}
			else //иначе обновляем существующий по guid
			{
				//***временно пока не работает обновление платежей
				//елси текущий уже есть в баже WD просто переходи к следующему
				res.f_when_done(new t()
				{
					{"f_name", "f_done"},
					{"f_args",  new t(){{"payments_to_kibicom",payments_to_kibicom}}}
				});
				/*
				DataRow dr = tab_payment.NewRow();

				dr["name"] = payment["name"].f_str();
				dr["idpaymentgroup"] = tab_paymentgroup.Select("name = '" + payment["tab_payment_way"]["name"].f_str());
				dr["smbase"] = payment["smbase"].f_str();
				dr["comment"] = payment["comment"].f_str();
				dr["guid"] = payment["wd_paymentdoc_guid"].f_str();

				dr.SetModified();

				mssql_cli.f_exec_cmd(new t()
				{
					{
						"cmd" , @"update paymentdoc "
								+"	set name="+payment["name"].f_str()+","
								+"	set idpaymentgroup"+tab_paymentgroup.Select
								(
									"name = '" + payment["tab_payment_way"]["name"].f_str()
								)+","
								+"	set smbase"++","
					},
					{"exec_scalar", true}
				})["res_cnt"].f_str();
			*/
			}

			/*
			string cmd = "insert "+tab_name+" (id, idgood, marking, idorder, name, qu) values ("+
					t_sql_builder.f_db_val(tabres_id)+","+
					t_sql_builder.f_db_val(mc_dr, "id_good")+","+
					t_sql_builder.f_db_val(mc_dr, "marking_id")+","+
					t_sql_builder.f_db_val(mc_dr, "idorder")+","+
					t_sql_builder.f_db_val("good_needs")+","+
					t_sql_builder.f_db_val(mc_dr, "qu")+")"
			*/

			return res;
		}

		public t f_get_idpaymentdocgroup(t args)
		{
			t payment=args["payment"];

			//MessageBox.Show(payment["dt_make"].f_str());

			DateTime dt = DateTime.Now;
			DateTime.TryParse(payment["dt_make"].f_str(), out dt);

			//получаем папку для года
			string pg_id_year=f_get_paymentdocgroup(new t()
			{
				{"pg_name", dt.Year.ToString()},
				{"where", " deleted is null and parentid is null and name = '"+dt.Year.ToString()+"'"},
				{"pg_parentid", "null"}
			})["pg_idpaymentdocgroup"].f_str();

			//получаем папку для месяца
			string pg_id_month = f_get_paymentdocgroup(new t()
			{
				{"pg_name", dt.Month.ToString()},
				{"where", " deleted is null and parentid ="+pg_id_year+" and name = '"+dt.Month.ToString()+"'"},
				{"pg_parentid", pg_id_year}
			})["pg_idpaymentdocgroup"].f_str();

			//получаем папку для дня
			string pg_id_day = f_get_paymentdocgroup(new t()
			{
				{"pg_name", dt.Day.ToString()},
				{"where", " deleted is null and parentid ="+pg_id_month+" and name = '"+dt.Day.ToString()+"'"},
				{"pg_parentid", pg_id_month}
			})["pg_idpaymentdocgroup"].f_str();


			return new t() { { "idpaymentdocgroup", pg_id_day } };
		}

		public t f_get_paymentdocgroup(t args)
		{
			db = new dbconn();
			string where = args["where"].f_str();
			string pg_name = args["pg_name"].f_str();
			string pg_parentid = args["pg_parentid"].f_str();
			string pg_idpaymentdocgroup="0";

			t_msslq_cli mssql_cli = this["mssql_cli"].f_val<t_msslq_cli>();

			//получаем папку платежей
			DataRow dr_pg = db.GetDataRow
			(
				"select * from paymentdocgroup where "+where
			);

			if (dr_pg == null)
			{

				pg_idpaymentdocgroup = mssql_cli.f_exec_cmd(new t()
				{
					{"cmd" , "exec gen_id 'gen_paymentdocgroup'"},
					{"exec_scalar", true}
				})["res_cnt"].f_str();

				int exe_cnt = db.Exec
				(
					@"insert paymentdocgroup (idpaymentdocgroup, name, parentid) 
						values ("+pg_idpaymentdocgroup+", '"+pg_name+"', "+pg_parentid+") "
				);
			}
			else
			{
				pg_idpaymentdocgroup=dr_pg["idpaymentdocgroup"].ToString();
			}

			return new t() { { "pg_idpaymentdocgroup", pg_idpaymentdocgroup } };
		}

		public t f_put_payment_kibicom(t args)
		{
			t tab_payment=args["tab_payment"];

			t ret = t.f_when_cre(args, this);

			//если нет платежей для синхронизации
			if (tab_payment.Count < 1)
			{
				ret.f_when_done(new t()
				{
					{"f_name", "f_done"}
				});
				return ret;
			}

			//выполняем запрос
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				//{"debug_group", "tstore_relat_one_to_many"},
				//{"debug_group", "tstore_sql"},
				{"req_timeout", 10000},
				{"method", "POST"},
				{
					"put_tab_arr", new t()
					{
						{"tab_payment", tab_payment}
					}
				},
				{"f_done_",args["f_done"].f_f()},
				{"f_fail_",args["f_fail"].f_f()},
				{"f_done",ret.f_when_done_f("f_done")},
				{"f_fail",ret.f_when_done_f("f_fail")},
				{"encode_json",true},
				{"cancel_prev",false},
				{"needs", new t(){"is_auth_done","authenticated"}}		//когда выполниться процесс авторизации
			});

			return ret;
		}

		#endregion платежи
	}
}
