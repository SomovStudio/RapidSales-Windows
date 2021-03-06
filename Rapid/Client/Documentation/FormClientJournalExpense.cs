﻿/*
 * Сделано в SharpDevelop.
 * Пользователь: Catfish
 * Дата: 15.03.2014
 * Время: 13:12
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Rapid.MSSQL;

namespace Rapid
{
	/// <summary>
	/// Description of FormClientJournalExpense.
	/// </summary>
	public partial class FormClientJournalExpense : Form
	{
		/* Глобальные переменные */
		private MsSQLFull _jurnalMySQL = new MsSQLFull();
		private DataSet _jurnalDataSet = new DataSet(); // элементы
		private int selectTableLine = 0;	// выбранная строка в таблице
		
		public FormClientJournalExpense()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		/* ОТКРЫТИЕ ФОРМЫ --------------------------------------------------- */
		void FormClientJournalExpenseLoad(object sender, EventArgs e)
		{
			ClassForms.OpenCloseFormJournalExpense = true;
			ClassForms.Rapid_Client.MessageConsole("Журнал Расходных накладных: открыт.", false);
			TableUpdate(); // Загрузка данных из базы данных
		}
		
		/* ТАБЛИЦА: Загружаем данные из базы данных в таблицу ----------------*/
		public void TableUpdate()
		{
			//Загрузка данных в таблицу
			try{
				listView1.Items.Clear();
				
				_jurnalDataSet.Clear();
				_jurnalDataSet.DataSetName = "journal";
				
				_jurnalMySQL.SelectSqlCommand = "SELECT * FROM journal WHERE (journal_date BETWEEN '" + dateTimePicker1.Text + "' AND '" + dateTimePicker2.Text + "' AND journal_type = 'Расходная Накладная' AND (journal_number LIKE '%" + textBox1.Text + "%' OR journal_total LIKE '%" + textBox1.Text + "%' OR journal_user_autor LIKE '%" + textBox1.Text + "%')) ORDER BY journal_date ASC";
				
				if(_jurnalMySQL.ExecuteFill(_jurnalDataSet, "journal") == false){
					ClassForms.Rapid_Client.MessageConsole("Журнал Расходных накладных: Ошибка выполнения запроса к таблице 'Журнал' при отборе документов.", true);
					return;
				}
				DataTable _table = _jurnalDataSet.Tables["journal"];
			
				// ОТОБРАЖЕНИЕ "Элементов"
				foreach(DataRow rowElement in _table.Rows)
        		{
					ListViewItem ListViewItem_add = new ListViewItem();
					ListViewItem_add.SubItems.Add(rowElement["journal_date"].ToString());
					if(rowElement["journal_delete"].ToString() == "0") //отметка удаления папки
						ListViewItem_add.StateImageIndex = 2; // папка не удалена
					else ListViewItem_add.StateImageIndex = 3; // папка удалена
					ListViewItem_add.SubItems.Add(rowElement["journal_number"].ToString());
					ListViewItem_add.SubItems.Add(rowElement["journal_type"].ToString());
					ListViewItem_add.SubItems.Add(ClassConversion.StringToMoney(rowElement["journal_total"].ToString()));
					ListViewItem_add.SubItems.Add(rowElement["journal_user_autor"].ToString());
					ListViewItem_add.SubItems.Add(rowElement["id_journal"].ToString());
					listView1.Items.Add(ListViewItem_add);
				}
				
				// ВЫБОР: выдиляем ранее выбранный элемент.
				listView1.SelectedIndices.IndexOf(selectTableLine);
			}catch{
				ClassForms.Rapid_Client.MessageConsole("Журнал Расходных накладных: Ошибка вывода информации выбранной из таблицы 'Журнал'.", true);
			}
		}
		
		/* ЗАКРЫТИЕ ОКНА ---------------------------------------------------- */
		void FormClientJournalExpenseClosed(object sender, EventArgs e)
		{
			ClassForms.Rapid_Client.MessageConsole("Журнал Расходных накладных: закрыт.", false);
			ClassForms.OpenCloseFormJournalExpense = false; // форма закрыта
		}
		
		void Button8Click(object sender, EventArgs e)
		{
			Close();			
		}
		/*--------------------------------------------------------------------*/
		
		/* Применение фильтров ---------------------------------------------- */
		/* Фильтр: период по дате */
		void Button5Click(object sender, EventArgs e)
		{
			TableUpdate();			
		}
		
		/* Поиск */
		void Button7Click(object sender, EventArgs e)
		{
			TableUpdate();
		}
		/*--------------------------------------------------------------------*/
		
		/* Создание документа ----------------------------------------------- */
		void CreateExpense()
		{
			FormClientDocExpense Rapid_ClientDocExpense = new FormClientDocExpense();
			Rapid_ClientDocExpense.MdiParent = ClassForms.Rapid_Client;
			Rapid_ClientDocExpense.Text = "Новая документ.";
			Rapid_ClientDocExpense.Show();	
		}
		
		void Button3Click(object sender, EventArgs e)
		{
			CreateExpense();
		}
		
		void РасходнаяНакладнаяToolStripMenuItemClick(object sender, EventArgs e)
		{
			CreateExpense();		
		}
		/*--------------------------------------------------------------------*/
		
		/* Редактировать выбранный документ ----------------------------------*/
		void OpenEditDoc()
		{
			if(listView1.SelectedIndices.Count > 0){ // проверка выбранного элемента
				if(listView1.SelectedItems[0].StateImageIndex == 2){ // документ не удалён.
					if(listView1.Items[listView1.SelectedIndices[0]].SubItems[3].Text.ToString() == "Расходная Накладная")
					{
						FormClientDocExpense Rapid_ClientDocExpense = new FormClientDocExpense();
						Rapid_ClientDocExpense.MdiParent = ClassForms.Rapid_Client;
						Rapid_ClientDocExpense.Text = "Изменить документ.";
						Rapid_ClientDocExpense.ActionID = listView1.Items[listView1.SelectedIndices[0]].SubItems[6].Text.ToString();
						Rapid_ClientDocExpense.Show();
					}
				}
			}
		}
		
		void Button4Click(object sender, EventArgs e)
		{
			OpenEditDoc();			
		}
		
		void ИзменитьДокументToolStripMenuItemClick(object sender, EventArgs e)
		{
			OpenEditDoc();			
		}
		/*--------------------------------------------------------------------*/
		
		/* ВЫБОР: при выборе строки в таблице */
		void ListView1SelectedIndexChanged(object sender, EventArgs e)
		{
			// выбранная строка таблицы
			if(listView1.SelectedItems.Count > 0)
				selectTableLine = listView1.SelectedItems[0].Index; // индекс выбраной строки
			// удалить или восстановить
			if(listView1.SelectedIndices.Count > 0){ // проверка выбранного элемента
				// документ
				if(listView1.SelectedItems[0].StateImageIndex == 3)
					удалитьДокументToolStripMenuItem.Text = "Восстановить документ.";
				else удалитьДокументToolStripMenuItem.Text = "Удалить документ.";
			}
		}
		/*--------------------------------------------------------------------*/
		
		/* Удаление выбранного документа */
		void DeleteDoc()
		{
			if(ClassConfig.Rapid_Client_UserRight == "admin" || ClassConfig.Rapid_Client_UserName == listView1.Items[listView1.SelectedIndices[0]].SubItems[5].Text.ToString()){
				if(listView1.SelectedIndices.Count > 0){ // проверка выбранного элемента
					// Удалить документ Расходная Накладная *************************
					if(listView1.Items[listView1.SelectedIndices[0]].SubItems[3].Text.ToString() == "Расходная Накладная")
					{
						if(listView1.SelectedItems[0].StateImageIndex == 2){ // не удалён
							// Установка отметки удаления 
							if(MessageBox.Show("Пометить документ на удаление?", "Вопрос:", MessageBoxButtons.YesNo) == DialogResult.Yes){
								MsSQLShort SQLCommand = new MsSQLShort();
								SQLCommand.SqlCommand = "UPDATE journal SET journal_delete = 1 WHERE (id_journal = " + listView1.Items[listView1.SelectedIndices[0]].SubItems[6].Text.ToString() + ")";
								if(SQLCommand.ExecuteNonQuery()){
									// ОСТАТКИ: увеличить остатки при удалении
									ClassBalance.BalanceRemoval(listView1.Items[listView1.SelectedIndices[0]].SubItems[6].Text.ToString(), true);
									// ИСТОРИЯ: Запись в журнал истории обновлений
									ClassServer.SaveUpdateInBase(9, DateTime.Now.ToString(), "", "Удаление документа.", "");
									ClassForms.Rapid_Client.MessageConsole("Полный журнал: успешное удаление документа.", false);
								} else ClassForms.Rapid_Client.MessageConsole("Полный журнал: Ошибка выполнения запроса к таблице 'Журнал' при удалении документа.", true);
							}
						}else{ // уже уданён
							// Восстановление записи
							if(MessageBox.Show("Восстановить документ?", "Вопрос:", MessageBoxButtons.YesNo) == DialogResult.Yes){
								MsSQLShort SQLCommand = new MsSQLShort();
								SQLCommand.SqlCommand = "UPDATE journal SET journal_delete = 0 WHERE (id_journal = " + listView1.Items[listView1.SelectedIndices[0]].SubItems[6].Text.ToString() + ")";
								if(SQLCommand.ExecuteNonQuery()){
									// ОСТАТКИ: уменьшить остатки при восстановлении
									ClassBalance.BalanceRecovery(listView1.Items[listView1.SelectedIndices[0]].SubItems[6].Text.ToString(), false);
									// ИСТОРИЯ: Запись в журнал истории обновлений
									ClassServer.SaveUpdateInBase(9, DateTime.Now.ToString(), "", "Восстановление документа.", "");
									ClassForms.Rapid_Client.MessageConsole("Полный журнал: успешное восстановление документа.", false);
								} else ClassForms.Rapid_Client.MessageConsole("Склады: Ошибка выполнения запроса к таблице 'Журнал' при восстановлении документа.", true);
							}
						}
					}
					//***************************************************************
				}
			}else ClassForms.Rapid_Client.MessageConsole("Полный журнал: у вас недостаточно прав для удаления выбранного документа.", false);
					
		}
				
		void Button9Click(object sender, EventArgs e)
		{
			DeleteDoc();			
		}
		
		void УдалитьДокументToolStripMenuItemClick(object sender, EventArgs e)
		{
			DeleteDoc();			
		}
		/*--------------------------------------------------------------------*/
	}
}
