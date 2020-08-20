using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient; // database client
namespace AMS_S_1._1
{
    class Logs
    {
        //declarations
        Data_Base db_obj = new Data_Base();
        int machine_num, machine_num1, verify, mode, year, month, day, hour, min, enabled, priv, backup, userid, count ,current_semaster = 0;
        string[,] temp = new string[60000, 4];
        int temp_c = 0;
        //constructor
        public Logs(Data_Base x)
        {
            db_obj = x; // 
        }
        // functions
        public void get_daily_logs() // connect to every machine and get daily logs
        {
            Console.WriteLine("---------- Get Logs -------------");
            current_semaster = db_obj.detect_semaster();
            for (int i = 0; i < db_obj.Machines_count; i++)
            {
                if (db_obj.Machines_D[i, 2] == "1")
                {
                    if (Program.zk_obj.Connect_Net(db_obj.Machines_D[i, 0], int.Parse(db_obj.Machines_D[i, 1])))       // Connect To Machine With Connect_Net Function in zklib
                    {
                        //start get logs from machine
                        while (Program.zk_obj.GetAllGLogData(Program.zk_obj.MachineNumber, ref machine_num, ref userid, ref machine_num1, ref verify, ref mode, ref year, ref month, ref day, ref hour, ref min))
                        {
                            //convert to 12 hour
                            if (hour > 12)
                                hour -= 12;

                            // get day
                            int day2 = 0;

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


                            for (int j = 0; j < 7; j++)
                            {

                                if (days[j, 1] == time.DayOfWeek.ToString())
                                {
                                    day2 = int.Parse(days[j, 0].ToString());
                                }
                            }

                            //end detect day

                            string time_user = hour + ":" + min;

                            // detect course 

                            string[] detected_data = db_obj.detect_lec_id_2(time_user, time_user, day2, userid);

                            // end of detection

                            //insert into logs
                            SqlCommand cmd = new SqlCommand();
                            int status = 0; // must removed
                            string com = "insert into machine_log values ({0},{1},'{2}',{3},{4},{5},{6},{7},{8},0,{9},'{10}','{11}')"; // Basic OF Query //viewd = 0
                            string q = string.Format(com, machine_num, userid, year, month, day, hour, min, detected_data[0], status, current_semaster, detected_data[2], detected_data[1]);  // query  | Replace index with its Value
                            cmd.CommandText = q;
                            cmd.Connection = db_obj.connection;
                            try
                            {
                                cmd.ExecuteReader();
                            }
                            catch (Exception e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e.ToString());
                            }


                        }
                        Program.zk_obj.ClearKeeperData(Program.zk_obj.MachineNumber);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Machine {0} -> Done", db_obj.Machines_D[i, 0]);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Could Not Connect To Machine -> {0}", db_obj.Machines_D[i, 0]);
                    }
                }
            }
        }

        bool check_temp(string user_id,string code,string prog_id)
        {
            for(int i = 0; i < temp_c; i++)
            {
                if (temp[i, 0] == user_id && temp[i, 1] == code && temp[i, 2] == prog_id)
                    return true;
            }
            return false;
        }

        //function to take attendance
        public void take_att()
        {
            Console.WriteLine("-------- Take attendance -----------");
            DateTime time = DateTime.Now;
            string year = time.Year.ToString();
            string month = time.Month.ToString();
           // month = "11";
            string day = time.Day.ToString();
            //day = "5";
            int semester_id = db_obj.detect_semaster();
           
            

            // query exexution 
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = db_obj.connection;
            SqlDataReader data;
            string q = "select userID ,course_code , program_id, lec__id from machine_log where log_year = {0} and log_day = {1} and log_month = {2};";
            string qu = string.Format(q, year, day, month);
            string q2 = "update new_absance set Att_c = Att_c+1 where c_code = '{0}' and UserID = {1} and semaster = {2}";

            
            cmd.CommandText = qu;
            Console.WriteLine(qu);
            try
            {
                data = cmd.ExecuteReader();
                
                string user_id, code, prog_id, lec__id = "NULL";
                while (data.Read())
                {
                    user_id = data.GetValue(0).ToString();
                    code = data.GetValue(1).ToString();
                    prog_id = data.GetValue(2).ToString();
                    lec__id = data.GetValue(3).ToString();
                    
                    if(code != "NULL" &&!check_temp(user_id,code,prog_id))
                    {
                        //add to temp
                        temp[temp_c, 0] = user_id;
                        temp[temp_c, 1] = code;
                        temp[temp_c, 2] = prog_id;
                        temp_c++;
                        // take att
                        string qu2 = string.Format(q2, code, user_id, semester_id);
                        SqlCommand cmd2 = new SqlCommand();
                        cmd2.CommandText = qu2;
                        cmd2.Connection = db_obj.connection;

                        try
                        {
                            cmd2.ExecuteReader();
                            Console.ForegroundColor = ConsoleColor.Green;
                            //Console.WriteLine(qu2);
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(e.ToString());
                        }



                    }

                }

                }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString()); ;
            }



        }
    }
}
