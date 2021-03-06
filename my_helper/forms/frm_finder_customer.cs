﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using kibicom.tlib;
using kibicom.josi;

namespace kibicom.my_wd_helper
{
	public partial class frm_finder_customer : frm_finder
	{

		public frm_finder_customer()
			: base()
		{
			InitializeComponent();
		}

		public frm_finder_customer(t args)
			: base(args)
		{
			this._args["wd_seller_guid"] = args["wd_seller_guid"];
		}

		//получение элементов из источника josi_store
		public t f_get_items_(t args)
		{

			t query = new t()
			{
				{
					"_query",new t()
					{
						{
							"entry", new t()
							{
								"name"
							}
						}
					}
				},
				{"id", ""},
				{"name", txt_query.Text},
				{"phone", ""},
				{"email", ""}
			};

			//выполняем запрос
			josi_store.f_store(new t 
			{
				//{"res_dot_key_query_str",res_dot_key_query_str},
				//когда возвращен ответ
				{"needs", new t(){"is_auth_done"}},		//когда выполниться процесс авторизации
				{"method", "POST"},
				{
					"get_tab_arr", new t()
					{
						{"tab_customer", new t(){query}}
					}
				},
				{
					//когда получен id
					//сохраняем заказ с учетом полученного id
					"f_done", new t_f<t,t>(delegate(t args1)
					{

						//если есть не пустой ответ
						if (args1["resp_json"].f_val() == null)
						{
							return null;
						}

						string tab = "tab_customer";
						ArrayList tab_rows = (ArrayList)args1["resp_json"].f_val<Dictionary<string, object>>()[tab];
						
						//если количество возвращенных результатов 0
						//то предлагаем создать нового контрагента
						if (tab_rows.Count == 0)
						{
							f_fill_lbx(new t());
							return new t();
						}

						//перебираем элементы результата и формируем элменты для listbox
						foreach (Dictionary<string, object> row in tab_rows)
						{

							string row_id = row.ContainsKey("id") && row["id"] != null ? row["id"].ToString() : "";
							string row_name = row.ContainsKey("name") && row["name"] != null ? row["name"].ToString() : "";
							string row_phone = row.ContainsKey("phone") && row["phone"] != null ? row["phone"].ToString() : "";
							string row_email = row.ContainsKey("email") && row["email"] != null ? row["email"].ToString() : "";
							string row_wd_customer_guid = row.ContainsKey("wd_customer_guid") && row["wd_customer_guid"] != null ?
															row["wd_customer_guid"].ToString() : "";


							//создаем очередной элемент
							this._args["items"].Add(new t()
							{
								{"str1", row_name},
								{"str2", row_phone},
								{
									"item", new t()
									{
										{"id",row_id},
										{"name",row_name},
										{"phone",row_phone},
										{"email",row_email},
										{"wd_customer_guid",row_wd_customer_guid}
									}
								}
							});

						}

						f_fill_lbx(new t());

						return new t();
					})
				},
				{"encode_json",true},
				{"cancel_prev",false},
			});

			return new t();
		}

		//получение элементов из источника local_store
		override public t f_get_items(t args)
		{
			string tab_address_uid = this._args["tab_address_uid"].f_str();
			string query = txt_query.Text.Replace(' ', '%');
			query = (new Regex("(\\d)")).Replace(query, "$1%");
			//query = (new Regex("(\\d)(?<=\\d)(\\d)")).Replace(query, "$1[-$2]");

			string where = "";
			if (this._args["using_local_store"].f_str() == "mssql")
			{
				if (tab_address_uid != "" && query=="")
				{
					where = " uid in (select tab_customer_uid from tab_relat_391" +
							" where tab_address_uid='" + tab_address_uid + "')" +
							" order by deleted, tab_pick_id desc, freq ";
				}
				else
				{
					where = " name like '%" + query + "%' " +
							" or phone like '%" + query + "%' " +
							" or email like '%" + query + "%' " +
							" order by deleted, tab_pick_id desc, freq ";
				}
			}
			else if (this._args["using_local_store"].f_str() == "sqlite")
			{
				where = " _nocase_search like '%" + query + "%' ";
			}

			kwj.f_select_tab_customer(new t()
			{
				{"limit" , 20},
				{"where", where},
				{
					"f_each", new t_f<t,t>(delegate (t args1)
					{
						DataRow dr = args1["each"]["item"].f_val<DataRow>();

						//создаем очередной элемент
						this._args["items"].Add(new t()
						{
							{"str1", dr["name"].ToString()},
							{"str2", dr["phone"].ToString()},
							{
								"item", new t()
								{
									{"id",dr["id"].ToString()},
									{"uid",dr["uid"].ToString()},
									{"name",dr["name"].ToString()},
									{"phone",dr["phone"].ToString()},
									{"email",dr["email"].ToString()},
									{"deleted",dr["deleted"].ToString()},
									{"wd_customer_guid",dr["wd_customer_guid"].ToString()},
									{"tab_pick_id",dr["tab_pick_id"].ToString()}
								}
							}
						});

						return new t();
					})
				},
				{
					"f_done", new t_f<t,t>(delegate (t args1)
					{
						
						f_fill_lbx(new t());

						return new t();
					})
				}
			});

			

			return new t();
		}

		//курсон переведен на другой элемента
		override public t f_mouse_change_selected_item(t args)
		{

			f_check_pick_btn(new t()
			{
				{"item", args["item"]},
				{"btn", btn_pick}
			});

			f_check_drop(new t()
			{
				{"item", args["item"]},
				{"btn", btn_pick}
			});

			return new t();
		}

		//сохранение созданного клиента в josi_store
		public void fstore_customer()
		{
			//текст введенный менеджером - номер карты
			//string fio = txt_fio.Text;
			//string phone = txt_phone.Text;
			//string email = txt_email.Text;
			//string address = txt_address.Text;

			josi_store.f_gen_id(new t()
			{
				{"res_name","tab_customer"}, 
				{"id_key","id"},
				{
					"f_done",
					new t_f<t,t>(delegate(t args)
					{
						MessageBox.Show(args["id_str"].f_str());
						string id = args["id_str"].f_str();
						string res_dot_key_query_str =	"&kvl.1.tab_arr.tab_customer.0.id=" + id +
														"&kvl.1.tab_arr.tab_customer.0.name=" + args["selected_item"]["name"].f_str() +
														"&kvl.1.tab_arr.tab_customer.0.phone=" + args["selected_item"]["phone"].f_str() +
														"&kvl.1.tab_arr.tab_customer.0.email=" + args["selected_item"]["email"].f_str();


						josi_store.f_store(new t()
						{
							{"res_dot_key_query_str",res_dot_key_query_str},
							{
								"f_done",
								new t_f<t,t>(delegate(t args1)
								{
									MessageBox.Show(args1["resp_str"].f_str());
									return null;
								})
							},
							{"encode_json",true},
							{"cancel_prev",true},
						});

						/*
						josi_store.f_store(new t_josi_store_req_args
						(
							res_dot_key_query_str,
							delegate(t_josi_store_req_args args1)
							{
								MessageBox.Show(args1.resp_str);
							}, 
							true, 
							true
						));
						 * */
						return null;
					})
				}
			});

			return;

		}

		//выбор отмеченного контрагента
		//вызывается если нажали Enter или клинкули на элементе listbox
		override public t f_select_item()
		{

			Hide();

			t.f_f("f_done", args);

			return new t();
		}

		override public t f_cre_item(t args)
		{

			//создаем форму ввода данных нового контрагента
			frm_cre_edit_item = new customer_info_form(txt_query.Text);

			//показываем форму как диалог
			frm_cre_edit_item.ShowDialog();

			if (((customer_info_form)frm_cre_edit_item).args["is_done"].f_bool())
			{
				//в результате деактивации текущего окна (окна поиска)
				//оно скроется так как предыдущее окно было диалогом
				//сюда мы попадем когда его закроют - данные введут
				//поэтом вновь показываем себя
				Show();

				//selected_item = ((customer_info.customer_info_form)frm_cre_edit_item).customer;

				t created_customer = ((customer_info_form)frm_cre_edit_item).args["item"];


				//формируем guid для нового контрагента
				created_customer["uid"].f_set(Guid.NewGuid().ToString());
				created_customer["dtcre"].f_set(DateTime.Now);
				created_customer["wd_customer_guid"].f_set(Guid.NewGuid().ToString());

				//this._args["selected_item"]["str1"] = this._args["selected_item"]["item"]["name"];
				//this._args["selected_item"]["str2"] = this._args["selected_item"]["item"]["phone"];

				//добавляем созданный элемент в кэш времени выполнения
				//запрос f_find() выполниться из кеша что бы не обращаться к серверу
				this._args["new_items"].Add(new t()
				{
					{"str1", created_customer["name"]},
					{"str2", created_customer["phone"]},
					{"item", created_customer}
				});


				f_find(new t()
				{
					{
						"f_done", new t_f<t,t> (delegate (t args1)
							{

								f_touch_lbx_item();

								t.f_f("f_done", this._args);

								return new t();
							})
					}
				});

				//сохраняем созданного контрагента
				kwj.f_tab_customer_add_mssql(new t() { { "item", created_customer } });
				//f_store(new t() { { "item", created_customer } });

			}

			frm_cre_edit_item = null;

			return new t();
		}

		override public t f_modify_item(t args)
		{

			t item = args["item"];

			//создаем форму ввода данных нового контрагента
			frm_cre_edit_item = new customer_info_form(new t(){{"item", item["item"]}});

			//показываем форму как диалог
			frm_cre_edit_item.ShowDialog();

			if (((customer_info_form)frm_cre_edit_item).args["is_done"].f_bool())
			{
				//в результате деактивации текущего окна (окна поиска)
				//оно скроется так как предыдущее окно было диалогом
				//сюда мы попадем когда его закроют - данные введут
				//поэтом вновь показываем себя
				Show();

				//selected_item = ((customer_info.customer_info_form)frm_cre_edit_item).customer;

				t created_customer = ((customer_info_form)frm_cre_edit_item).args["item"];

				

				lbx_items.SelectedItem = item;

				f_fill_lbx(new t());

				//f_touch_lbx_item();

				//сохраняем созданного контрагента
				kwj.f_tab_customer_modify_mssql(new t() { { "item", created_customer } });
			}

			frm_cre_edit_item = null;

			return new t();

		}

		override public t f_pick_item(t args)
		{

			t item = args["item"];
			Button btn = args["btn"].f_val<Button>();

			//если элемен сейчас пришпилин
			//отшпиливаем. и наоборот
			if (item["item"]["tab_pick_id"].f_is_empty())
			{
				//нажимаем кнопку визуально
				f_btn_push(btn);

				//крепим элемент
				kwj.f_tab_customer_pick_mssql(new t() 
				{ 
					{ "item", item["item"] },
					{ "wd_seller_guid", this._args["wd_seller_guid"]}
				});
			}
			else
			{
				//отпускаем кнопку визуально
				f_btn_up(btn);

				//открепляем элемент
				kwj.f_tab_customer_unpick_mssql(new t() 
				{ 
					{ "item", item["item"] },
					{ "wd_seller_guid", this._args["wd_seller_guid"]}
				});
			}
			
			return new t();

		}

		//удаление элемента
		override public t f_drop_item(t args)
		{

			t item = args["item"];
			Button btn = args["btn"].f_val<Button>();

			
			//удаляем элемент
			kwj.f_tab_customer_drop_mssql(new t() 
			{ 
				{ "item", item["item"] },
				{ "wd_seller_guid", this._args["wd_seller_guid"]}
			});
			
			//вызываем проверку удаления, чтобы выключить ненужные теперь кнопки
			f_check_drop(args);

			return new t();

		}

		//восстановление элемента
		override public t f_revert_item(t args)
		{

			t item = args["item"];
			Button btn = args["btn"].f_val<Button>();


			//восстанавливаем элемент
			kwj.f_tab_customer_revert_mssql(new t() 
			{ 
				{ "item", item["item"] },
				{ "wd_seller_guid", this._args["wd_seller_guid"]}
			});

			//вызываем проверку удаления, чтобы включить кнопки
			f_check_drop(args);

			return new t();

		}

		//сохранение связи между выбранный клиентом и адресом
		public t f_store_related_address(t args)
		{
			if (!this._args["tab_address_uid"].f_is_empty())
			{
				kwj.f_store_customer_address_relat(new t()
				{
					{"tab_address_uid", this._args["tab_address_uid"]},
					{"tab_customer_uid", this._args["selected_item"]["item"]["uid"]}
				});
			}
			return new t();
		}

		public t f_check_pick_btn(t args)
		{
			t item = args["item"];
			Button btn = args["btn"].f_val<Button>();

			if (item["item"]["tab_pick_id"].f_is_empty())
			{
				f_btn_up(btn);
			}
			else
			{
				f_btn_push(btn);
			}

			return new t();
		}

		public t f_check_drop(t args)
		{
			t item = args["item"];
			Button btn = args["btn"].f_val<Button>();

			//если deleted пуст то показываем кнопки
			if (item["item"]["deleted"].f_is_empty())
			{
				btn_pick.Visible = true;
				btn_change.Visible = true;
				btn_drop.Visible = true;

				btn_revert.Visible = false;

			}
			else
			{
				btn_pick.Visible = false;
				btn_change.Visible = false;
				btn_drop.Visible = false;

				btn_revert.Visible = true;
			}

			return new t();
		}

		public t f_store(t args)
		{

			kwj.f_tab_customer_add_mssql(args);

			return new t();
		}
	
	}
}
