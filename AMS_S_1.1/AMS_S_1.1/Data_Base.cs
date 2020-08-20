using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient; // database client
namespace AMS_S_1._1
{
    class Data_Base
    {
        // database connection strings
        string connection_D = @"" ;

        public SqlConnection connection; // connection object
        
        // Declarations 
        public string[,] Machines_D = new string[100,4];
        public int Machines_count = 0;
        DateTime time = DateTime.Now;
        // end Declartion

        // Functions
        public bool start_connection()  // start and ensure connection to database
        {

            try
            {
                connection = new SqlConnection(connection_D);
                connection.Open();  // Start connection
                Console.WriteLine("DataBase Connected ");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }


        public void get_machines()  // get machines deteils
        {
            SqlCommand cmd = new SqlCommand(); // object 
            SqlDataReader data;                 //data reader from data base
            string com = "select * from Machines";
            cmd.CommandText = com;
            cmd.Connection = connection;  // Connection object
            try
            {
                data = cmd.ExecuteReader(); // excute query
                int count = 0;
                while (data.Read())
                {

                    // Save Machines Details Into Arrays 
                    Machines_D[count, 0] = data.GetValue(1).ToString();
                    Machines_D[count, 1] = data.GetValue(2).ToString();
                    Machines_D[count, 2] = "0"; // 0 offline by defult before test connection
                    Machines_D[count, 3] = data.GetValue(0).ToString();
                    count++;
                    Machines_count++;
                    System.Console.WriteLine();
                }
                data.Close();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }  
      
        }

        public void connect_to_machines()
        {
            Console.WriteLine("------ Machine Connection Test -------");
            for (int i = 0; i < Machines_count; i++)
            {
                if (Program.zk_obj.Connect_Net(Machines_D[i, 0], int.Parse(Machines_D[i, 1])))       // Connect To Machine With Connect_Net Function in zklib
                {
                    //sync time
                    Program.zk_obj.SetDeviceTime2(Program.zk_obj.MachineNumber, time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                    //Machines_D[i, 1] -> ip | Machines_D[i, 2] -> port
                    Machines_D[i, 2] = "1"; // Flag prove that machine i6s online
                    Program.zk_obj.Disconnect();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} -> Online ",Machines_D[i,0]);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} -> Offline ", Machines_D[i, 0]);
                    Program.zk_obj.Disconnect();
                }
            }
            Console.WriteLine("------ END TEST -------");
            Console.WriteLine();
        }


        public void get_enroll_users()
        {
            Console.WriteLine("-------- Get_Enroll_Users ---------");
            // detect day 
            SqlCommand cmd = new SqlCommand(); // object 
            SqlDataReader data;                 //data reader from data base
            SqlDataReader data2;
            int day = 0;

            DateTime time = DateTime.Now;
            string[,] days = new string[7, 2];
            
            days[0, 0] = "1";
            days[0, 1] = "Saturday";

            days[1, 0] = "2";
            days[1, 1] = "Sunday";

            days[2, 0] = "3";
            days[2, 1] = "Monday";

            days[3, 0] = "4";
            days[3, 1] = "Tuesday";

            days[4, 0] = "5";
            days[4, 1] = "Wednesday";

            days[5, 0] = "6";
            days[5, 1] = "Thursday";

            days[6, 0] = "7";
            days[6, 1] = "Friday";

           
            for (int i = 0; i < 7; i++)
            {

                if (days[i, 1] == time.DayOfWeek.ToString())
                {
                    day = int.Parse(days[i, 0].ToString());
                }
            }
            //day = 4;
            // query exexution 
            string q = "select distinct lec_ID,Doctor_ID,T_From,T_To,T_Absance_time_our,Place_ID,S_ID,S_Card_Number,[lecture].Course_ID from [lecture] join [Student] on [lecture].Course_ID = [Student].Course_ID and [lecture].Lec_date = {0}  order by T_From;";
            //string q2 = "select D_ID, D_Card_Number from Doctor join [lecture] on [lecture].Doctor_ID = D_ID and[lecture].Lec_date = {0}";
            string q2 = "select D_ID, D_Card_Number from Doctor join [lecture] on [lecture].Doctor_ID = D_ID ";
            string qu = string.Format(q, day);
            //string qu2 = string.Format(q2, day);
            string qu2 = q2;
            cmd.CommandText = qu;
            cmd.Connection = connection;
            try
            {
                for(int i = 0; i < Machines_count; i++) // enroll users for every online machine
                {
                    if(Machines_D[i,2] == "1")  // if machine online
                    {
                        if (Program.zk_obj.Connect_Net(Machines_D[i, 0], int.Parse(Machines_D[i, 1])))       // Connect To Machine With Connect_Net Function in zklib
                        {
                            Program.zk_obj.ClearKeeperData(Program.zk_obj.MachineNumber);   // clear data
                            //start enroll
                            if (Program.zk_obj.BeginBatchUpdate(0, 1))  // Begine Batch To Update , Upload Finger Print
                            {
                                cmd.CommandText = qu;
                                data = cmd.ExecuteReader(); // get users from db
                                
                                while (data.Read())
                                {
                                   
                                        string User_ID = data.GetValue(6).ToString();
                                        string Card_ID = data.GetValue(7).ToString();
                                        Program.zk_obj.set_CardNumber(0, int.Parse(Card_ID));
                                    /*
                                        if (Program.zk_obj.SetUserInfo(int.Parse(Machines_D[i,3]), int.Parse(User_ID), "", "", 0, true))  // enroll user
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Program.zk_obj.SetEnrollDataStr((int.Parse(Machines_D[i, 3])), int.Parse(User_ID), Program.zk_obj.MachineNumber, 11, 0, "", 0); // connect user with card
                                            Console.WriteLine("User : {0} -> Enrolled",User_ID);

                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("User : {0} -> Failed", User_ID);
                                        }
                                       
                                    */
                                }
                                data.Close();
                                cmd.CommandText = qu2; // query for doctors
                                data = cmd.ExecuteReader();

                                while (data.Read())
                                {
                                   
                                        string doc_id = data.GetValue(0).ToString();
                                        string doc_card = data.GetValue(1).ToString();
                                        Program.zk_obj.set_CardNumber(0, int.Parse(doc_card));
                                        if (Program.zk_obj.SetUserInfo(int.Parse(Machines_D[i, 3]), int.Parse(doc_id), "", "", 0, true))  // enroll user
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Program.zk_obj.SetEnrollDataStr((int.Parse(Machines_D[i, 3])), int.Parse(doc_id), Program.zk_obj.MachineNumber, 11, 0, "", 0); // connect user with card
                                            Console.WriteLine("Doctor : {0} -> Enrolled", doc_id);

                                        }
                                        else
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Doctor : {0} -> Failed", doc_id);
                                        }
                                        

                                    
                                }



                                Program.zk_obj.RefreshData(4370);
                                if (Program.zk_obj.BatchUpdate(int.Parse(Machines_D[i, 3])))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    System.Console.WriteLine("Updated");    // Update Finger Prints 
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    System.Console.WriteLine("Update Failed");
                                }

                                if (Program.zk_obj.EnableDevice(int.Parse(Machines_D[i, 3]), true))
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    System.Console.WriteLine("Enabled");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Machine -> {0} : Failed to update",Machines_D[i,0]);
                                }

                               
                                data.Close();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Machine begine batch failed -> {0}", Machines_D[i, 0]);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Could Not Connect To Machine -> {0}", Machines_D[i, 0]);
                        }
                           

                    }
                    
                }
                

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString()); ;
            }

            Console.WriteLine("----------- END -------------");
            Console.WriteLine();
        }
        

        //detect current semaster
        public int detect_semaster()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;
            SqlDataReader data;
            string com = "select * from semaster_time";
            cmd.CommandText = com;
            int semaster_id = 0; // semaster id
            string from = "NULL"; // semaster From
            string to = "NULL";  // semaster To

            try
            {
                data = cmd.ExecuteReader(); // excute query
                while (data.Read())
                {
                    semaster_id = int.Parse(data.GetValue(0).ToString());
                    from = data.GetValue(1).ToString();
                    to = data.GetValue(2).ToString();

                    DateTime tmp_from = DateTime.Parse(from);
                    DateTime tmp_to = DateTime.Parse(to);
                    DateTime time = DateTime.Now;

                    TimeSpan hours1 = tmp_to.Subtract(time);
                    TimeSpan hours2 = time.Subtract(tmp_from);

                    int h1 = hours1.Days;
                    int h2 = hours2.Days;

                    if (h1 >= 0 && h2 >= 0)
                    {
                        return semaster_id;
                    }


                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

            return 0;
        }

        public string[] detect_lec_id(string from,string to, int day,int user_id)
        {
            
            // query exexution 
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;
            SqlDataReader data;
            string q = "select distinct lec_ID,T_From,T_To,[lecture].Program_id,[lecture].Course_code from [lecture] join [Student] on [lecture].Course_ID = [Student].Course_ID and [lecture].Lec_date = {0} and ([Student].S_ID = {1} or Doctor_ID = {2})  order by T_From;";
            string qu = string.Format(q, day,user_id,user_id);
            string[] detected_data = new string[3];
            cmd.CommandText = qu;
            cmd.Connection = connection;
            try
            {
                data = cmd.ExecuteReader(); // get lectures from db
                string T_From, T_to, lec_id, p_id, c_code = "NULL";
                DateTime lec_from, lec_to, user_from, user_to;
                
                detected_data[0] = "NULL";  // defualt 
                detected_data[1] = "NULL";  // default
                detected_data[2] = "NULL";  // default
                while (data.Read())
                {
                    
                    T_From = data.GetValue(1).ToString();   // get lect start from
                    T_to = data.GetValue(2).ToString();     // get lec end to
                    lec_id = data.GetValue(0).ToString();   // get lec id
                    p_id = data.GetValue(3).ToString();     // get lec program_code
                    c_code = data.GetValue(4).ToString();   // get course code
                    lec_from = DateTime.Parse(T_From);
                    lec_to =   DateTime.Parse(T_to);
                    user_from = DateTime.Parse(from.ToString());
                    user_to = DateTime.Parse(to.ToString());
                    //begine compare
                    if ((user_from >= lec_from) && (user_to <= lec_to))
                    {
                        
                        detected_data[0] = lec_id;
                        detected_data[1] = p_id;
                        detected_data[2] = c_code;
                        return detected_data;
                    }


                }

            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
            return detected_data;
        }
        // End Functions

        public string[] detect_lec_id_2(string from, string to, int day, int user_id)   // with one paramter
        {

            // query exexution 
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;
            SqlDataReader data;
            string q = "select distinct lec_ID,T_From,T_To,[lecture].Program_id,[lecture].Course_code from [lecture] join [Student] on [lecture].Course_ID = [Student].Course_ID and [lecture].Lec_date = {0} and ([Student].S_ID = {1} or Doctor_ID = {2})  order by T_From;";
            string qu = string.Format(q, day, user_id, user_id);
            string[] detected_data = new string[3];
            cmd.CommandText = qu;
            cmd.Connection = connection;
            try
            {
                data = cmd.ExecuteReader(); // get lectures from db
                string T_From, T_to, lec_id, p_id, c_code = "NULL";
                DateTime lec_from, lec_to, user_from, user_to;

                detected_data[0] = "NULL";  // defualt 
                detected_data[1] = "NULL";  // default
                detected_data[2] = "NULL";  // default
                while (data.Read())
                {

                    T_From = data.GetValue(1).ToString();   // get lect start from
                    T_to = data.GetValue(2).ToString();     // get lec end to
                    lec_id = data.GetValue(0).ToString();   // get lec id
                    p_id = data.GetValue(3).ToString();     // get lec program_code
                    c_code = data.GetValue(4).ToString();   // get course code
                    lec_from = DateTime.Parse(T_From);
                    lec_to = DateTime.Parse(T_to);
                    user_from = DateTime.Parse(from.ToString());
                    user_to = DateTime.Parse(to.ToString());
                    //begine compare
                    if ((user_from >= lec_from) && (user_from <= lec_to))
                    {

                        detected_data[0] = lec_id;
                        detected_data[1] = p_id;
                        detected_data[2] = c_code;
                        return detected_data;
                    }


                }

            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
            return detected_data;
        }
    }
}
