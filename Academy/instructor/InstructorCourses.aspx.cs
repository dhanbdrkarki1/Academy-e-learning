﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Configuration;

namespace Academy
{
    public partial class WebForm6 : System.Web.UI.Page
    {
        string connectionString = WebConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {

            listDropDownCategory();
            populateGridView();
        }


        // get instructor id from db
        int getInstructorId()
        {
            int instructorId = -1;

            string username = "";
            if (Session["username"] != null)
            {
                username = Session["username"].ToString();
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("select AccountId from UserAccount where username='" + username + "'", connection);
                command.Parameters.AddWithValue("@username", username);

                connection.Open();
                SqlDataReader sdr = command.ExecuteReader();

                if (sdr.Read())
                {
                    return instructorId = sdr.GetInt32(0);
                }
            }
            return 0;
        }

        string getUsername()
        {
            string username = "";
            if (Session["username"] != null)
            {
                username = Session["username"].ToString();
                return username;
            }
            else
            {
                Response.Redirect("~/Login.aspx");
            }
            return null;
        }


        //add courses in db
        protected void btnAddCourse(object sender, EventArgs e)
        {
            int instructorId = getInstructorId();
            var dateTime = DateTime.Now;
            var dateTimeVal = dateTime.ToString("yyyy/MM/dd");
            String query = "insert into Courses(Title,Category,OverView,Rate,CreatedAt, InstructorId) values(@title,@category,@overview,@rate,@createdAt,@id)";
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@title", txtCourseTitle.Text);
                cmd.Parameters.AddWithValue("@category", ddCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@overview", txtOverView.Text);
                cmd.Parameters.AddWithValue("@rate", txtRate.Text);
                cmd.Parameters.AddWithValue("@createdAt", dateTimeVal);
                cmd.Parameters.AddWithValue("@id", instructorId);
                cmd.ExecuteNonQuery();
                ClearField();
                populateGridView();
                lblSuccess.Text = "New course added.";
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        //clears field
        void ClearField()
        {
            txtCourseTitle.Text = txtOverView.Text = txtRate.Text = "";
        }

        // populate all the data from db in gridview
        void populateGridView()
        {
            int instructorId = getInstructorId();
            string query = "select c.CourseId, c.Title, cc.Category, c.OverView, c.Rate, c.CreatedAt from Courses c join CourseCategory cc on c.Category = cc.CourseCatId where c.InstructorId='" + instructorId + "'";

            //String query = "select Courses.Title, CourseCategory.Category, Courses.OverView, Courses.Rate, Courses.CreatedAt from Courses inner join CourseCategory on Courses.Category=CourseCategory.CourseCatId where Courses.InstructorId='" + instructorId + "' ";
            //string query = "select Courses.Title, CourseCategory.Category, Courses.OverView, Courses.Rate, Courses.CreatedAt from Courses inner join CourseCategory on Courses.Category=CourseCategory.CourseCatId where Courses.InstructorId=1";
            string query1 = "select * from Courses where InstructorId='" + instructorId + "' ";
            SqlConnection con = new SqlConnection(connectionString);

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                cmd.ExecuteNonQuery();
                DataTable dt = new DataTable();
                sda.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    gvManageCourse.DataSource = dt;
                    gvManageCourse.DataBind();
                }
                else
                {
                    dt.Rows.Add(dt.NewRow());
                    gvManageCourse.DataSource = dt;
                    gvManageCourse.DataBind();
                    gvManageCourse.Rows[0].Cells.Clear();
                    gvManageCourse.Rows[0].Cells.Add(new TableCell());
                    gvManageCourse.Rows[0].Cells[0].ColumnSpan = dt.Columns.Count;
                    gvManageCourse.Rows[0].Cells[0].Text = "No data found...";
                    gvManageCourse.Rows[0].Cells[0].HorizontalAlign = HorizontalAlign.Center;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong.");
            }
        }

        // triggers when edit btn is pressed
        protected void gvManageCourse_RowEditing(object sender, GridViewEditEventArgs e)
        {
            listDropDownCategory();
            gvManageCourse.EditIndex = e.NewEditIndex;

            populateGridView();
        }

        //triggers when cancel btn is pressed
        protected void gvManageCourse_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvManageCourse.EditIndex = -1;
            populateGridView();
        }


        //row updating
        protected void gvManageCourse_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            // Get the updated values for the row being edited
            int courseId = Convert.ToInt32(gvManageCourse.DataKeys[e.RowIndex].Value);
            string title = (gvManageCourse.Rows[e.RowIndex].FindControl("txtTitle") as TextBox).Text;
            string overview = (gvManageCourse.Rows[e.RowIndex].FindControl("txtOverView") as TextBox).Text;
            decimal rate = Convert.ToDecimal((gvManageCourse.Rows[e.RowIndex].FindControl("txtRate") as TextBox).Text);
            DateTime createdAt = Convert.ToDateTime((gvManageCourse.Rows[e.RowIndex].FindControl("txtCreatedAt") as TextBox).Text);
            string category = (gvManageCourse.Rows[e.RowIndex].FindControl("ddlCat") as DropDownList).SelectedValue;

            // Update the database with the updated values
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string query = "UPDATE Courses SET Title = @Title, Category = @Category, OverView = @OverView, Rate = @Rate, CreatedAt = @CreatedAt WHERE CourseId = @CourseId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@OverView", overview);
                    cmd.Parameters.AddWithValue("@Rate", rate);
                    cmd.Parameters.AddWithValue("@CreatedAt", createdAt);
                    cmd.Parameters.AddWithValue("@CourseId", courseId);
                    cmd.ExecuteNonQuery();
                }
            }

            // Cancel the edit mode for the row
            gvManageCourse.EditIndex = -1;

            // Rebind the GridView to show the updated data
            populateGridView();
        }


        //triggers on clicking update btn and update value in database
        //protected void gvManageCourse_RowUpdating(object sender, GridViewUpdateEventArgs e)
        //{

        //    String query = "update Courses set Title=@Title, OverView=@OverView, Rate=@Rate, CreatedAt=@CreatedAt, Category=@Category where CourseId=1";
        //    SqlConnection con = new SqlConnection(connectionString);
        //    try
        //    {
        //        con.Open();
        //        SqlCommand cmd = new SqlCommand(query, con);
        //        string s = (gvManageCourse.DataKeys[e.RowIndex].Value.ToString()) + "<br>" +
        //            (gvManageCourse.Rows[e.RowIndex].FindControl("txtTitle") as TextBox).Text.Trim() + "<br>" +
        //            (gvManageCourse.Rows[e.RowIndex].FindControl("ddlCat") as DropDownList).SelectedValue + "<br>" +
        //            (gvManageCourse.Rows[e.RowIndex].FindControl("txtOverView") as TextBox).Text.Trim() + "<br>" +
        //            (gvManageCourse.Rows[e.RowIndex].FindControl("txtRate") as TextBox).Text.Trim() + "<br>" +
        //            (gvManageCourse.Rows[e.RowIndex].FindControl("txtCreatedAt") as TextBox).Text.Trim();




        //        cmd.Parameters.AddWithValue("@CourseId", (gvManageCourse.DataKeys[e.RowIndex].Value.ToString()));
        //        cmd.Parameters.AddWithValue("@Title", (gvManageCourse.Rows[e.RowIndex].FindControl("txtTitle") as TextBox).Text.Trim());
        //        cmd.Parameters.AddWithValue("@Category", (gvManageCourse.Rows[e.RowIndex].FindControl("ddlCat") as DropDownList).SelectedValue);
        //        cmd.Parameters.AddWithValue("@OverView", (gvManageCourse.Rows[e.RowIndex].FindControl("txtOverView") as TextBox).Text.Trim());
        //        cmd.Parameters.AddWithValue("@Rate", 100); // (gvManageCourse.Rows[e.RowIndex].FindControl("txtRate") as TextBox).Text.Trim());
        //        cmd.Parameters.AddWithValue("@CreatedAt", (gvManageCourse.Rows[e.RowIndex].FindControl("txtCreatedAt") as TextBox).Text.Trim());

        //        int i = cmd.ExecuteNonQuery();
        //        lblSuccess.Text = s + "<br> Rows affected = " + i;
        //        if (i > 0)
        //        {
        //            Response.Write("<script>alert('Rows affected');</script>");

        //        }
        //        // cancel the edit operation
        //        gvManageCourse.EditIndex = -1;
        //        con.Close();
        //        // Rebind the GridView to the data source
        //        populateGridView();
        //    }
        //    catch (Exception ex)
        //    {
        //        lblError.Text = ex.Message;
        //    }

        //}

        // delete item from db
        protected void gvManageCourse_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            String query = "DELETE from Courses where CourseID=@id";
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", Convert.ToInt32(gvManageCourse.DataKeys[e.RowIndex].Value.ToString()));
                cmd.ExecuteNonQuery();
                populateGridView();
                lblSuccess.Text = "Selected item deleted.";
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }


        // bind all types of category from database
        void listDropDownCategory()
        {
            String query = "select * from CourseCategory";
            SqlConnection con = new SqlConnection(connectionString);
            try
            {

                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;

                ddCategory.DataSource = cmd.ExecuteReader();
                ddCategory.DataTextField = "Category";
                ddCategory.DataValueField = "CourseCatId";
                ddCategory.DataBind();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong.");
            }
        }

        // bind all types of categories from database into gridview of edittemplate
        protected void gvManageCourse_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                    String query = "select * from CourseCategory";

                    SqlConnection con = new SqlConnection(connectionString);
                    try
                    {
                        DropDownList ddList = (DropDownList)e.Row.FindControl("ddlCat");
                        con.Open();
                        SqlCommand cmd = new SqlCommand(query, con);
                        SqlDataAdapter sda = new SqlDataAdapter(cmd);
                        cmd.ExecuteNonQuery();
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        ddList.DataSource = dt;
                        ddList.DataTextField = "Category";
                        ddList.DataValueField = "CourseCatId";
                        ddList.DataBind();
                        string selectedCategory = DataBinder.Eval(e.Row.DataItem, "Category").ToString();
                        ddList.Items.FindByValue(selectedCategory).Selected = true;
                    }
                    catch (Exception ex)
                    {
                        Response.Write(ex.Message);
                    }
                }
            }

        }

        protected void AddContent_Click(object sender, EventArgs e)
        {
            // Get a reference to the button that was clicked
            Button btn = sender as Button;

            // Get a reference to the row that contains the button
            GridViewRow row = btn.NamingContainer as GridViewRow;
            // Get the title of the course
            Label lblTitle = row.FindControl("lblTitle") as Label;

            Label lblCourseId = row.FindControl("lblCourseId") as Label;
            //adding courseid as cookie
            HttpCookie cookie = new HttpCookie("CourseId");
            cookie.Value = lblCourseId.Text;
            Response.Cookies.Add(cookie);


            string title = lblTitle.Text;
            txtCTitle.Text = title;

            string script1 = "$(document).ready(function(){ $('#btnAddContent').modal('show'); });";
            ClientScript.RegisterStartupScript(this.GetType(), "ShowModalScript", script1, true);
        }

        protected void btnAddCourseContent_Click(object sender, EventArgs e)
        {

            // fetching courseid from cookie
            string course_id = "";
            HttpCookie cookie = Request.Cookies["CourseId"];
            if (cookie != null)
            {
                course_id = cookie.Value;
            }
            Response.Write("<script>alert('text: ' + '" + txtCContent.Text + "')</script>");

            // saving image a& file locally
            string imgPath = "";
            string filePath = "";
            string imgfolderPath = Server.MapPath("~/Images/Content/");

            if (ftImage.HasFile)
            {
                string imgName = ftImage.FileName;
                imgPath = "~/Images/Content/" + imgName;
                ftImage.SaveAs(imgfolderPath + Path.GetFileName(ftImage.FileName));
            }

            if (ftFile.HasFile)
            {
                string fileName = ftFile.FileName;
                filePath = "~/Images/Content/" + fileName;
                ftImage.SaveAs(imgfolderPath + Path.GetFileName(ftFile.FileName));
            }

            //adding into database
            String query = "insert into CourseContent (CId, ContTitle, TextCont, ImageCont, ContentUrl, FileCont) values(@cid,@title,@text,@image,@url,@file)";
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query,con);
                cmd.Parameters.AddWithValue("@cid",course_id);
                cmd.Parameters.AddWithValue("@title", txtContentTitle.Text);
                cmd.Parameters.AddWithValue("@text", txtCContent.Text);
                cmd.Parameters.AddWithValue("@image", imgPath);
                cmd.Parameters.AddWithValue("@url", txtUrl.Text);
                cmd.Parameters.AddWithValue("@file", filePath);

                cmd.ExecuteNonQuery();
                txtContentTitle.Text = txtCContent.Text = txtUrl.Text = "";
                Response.Cookies["CourseId"].Expires = DateTime.Now.AddDays(-1);

                lblSuccess.Text = "Course content added.";
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }



        protected void lnkUpdateContent_Click(object sender, EventArgs e)
        {
            // Get a reference to the button that was clicked
            Button btn = sender as Button;

            // Get a reference to the row that contains the button
            GridViewRow row = btn.NamingContainer as GridViewRow;

            // Get the courseid of the course
            Label lblCourseId = row.FindControl("lblCourseId") as Label;

            //redirecting to update page with id
            int CourseId = Convert.ToInt32(lblCourseId.Text);
            Response.Redirect(Page.ResolveUrl("~/InstructorCourseContentUpdate.aspx?Cid=" + CourseId));
        }
    }
}